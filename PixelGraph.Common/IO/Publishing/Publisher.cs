﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Publishing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IPublisher
    {
        Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default);
    }

    internal class Publisher : IPublisher
    {
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly INamingStructure naming;
        private readonly ILogger logger;
        private readonly ITextureGraphBuilder graphBuilder;
        private readonly IFileLoader loader;


        public Publisher(
            ITextureGraphBuilder graphBuilder,
            IFileLoader loader,
            IInputReader reader,
            IOutputWriter writer,
            INamingStructure naming,
            ILogger<Publisher> logger)
        {
            this.graphBuilder = graphBuilder;
            this.loader = loader;
            this.reader = reader;
            this.writer = writer;
            this.naming = naming;
            this.logger = logger;
        }

        public async Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (clean) {
                logger.LogDebug("Cleaning destination...");

                try {
                    writer.Clean();

                    logger.LogInformation("Destination directory clean.");
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to clean destination!");
                    throw new ApplicationException("Failed to clean destination!", error);
                }
            }

            await PublishPackMetaAsync(context.Profile, token);

            await PublishContentAsync(context, token);
        }

        private async Task PublishContentAsync(ResourcePackContext context, CancellationToken token = default)
        {
            var genericPublisher = new GenericTexturePublisher(context.Profile, reader, writer);

            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;

            await foreach (var fileObj in loader.LoadAsync(token)) {
                token.ThrowIfCancellationRequested();
                DateTime? sourceTime, destinationTime = null;

                switch (fileObj) {
                    case MaterialProperties material:
                        sourceTime = reader.GetWriteTime(material.LocalFilename);
                        foreach (var texFile in reader.EnumerateAllTextures(material)) {
                            var z = reader.GetWriteTime(texFile);
                            if (!z.HasValue) continue;

                            if (!sourceTime.HasValue || z.Value > sourceTime.Value)
                                sourceTime = z.Value;
                        }

                        if (material.IsMultiPart) {
                            foreach (var part in material.Parts) {
                                var albedoOutputName = naming.GetOutputTextureName(context.Profile, part.Name, TextureTags.Albedo, true);
                                var albedoFile = PathEx.Join(material.LocalPath, albedoOutputName);
                                var writeTime = writer.GetWriteTime(albedoFile);
                                if (!writeTime.HasValue) continue;

                                if (!destinationTime.HasValue || writeTime.Value > destinationTime.Value)
                                    destinationTime = writeTime;
                            }
                        }
                        else {
                            var albedoOutputName = naming.GetOutputTextureName(context.Profile, material.Name, TextureTags.Albedo, true);
                            var albedoFile = PathEx.Join(material.LocalPath, albedoOutputName);
                            destinationTime = writer.GetWriteTime(albedoFile);
                        }

                        if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug("Skipping up-to-date texture {DisplayName}.", material.DisplayName);
                            continue;
                        }

                        logger.LogDebug("Publishing texture {DisplayName}.", material.DisplayName);

                        var materialContext = new MaterialContext {
                            Input = context.Input,
                            Profile = context.Profile,
                            Material = material,
                        };

                        await graphBuilder.ProcessOutputGraphAsync(materialContext, token);

                        // TODO: Publish mcmeta files

                        break;
                    case string localName:
                        sourceTime = reader.GetWriteTime(localName);
                        destinationTime = writer.GetWriteTime(localName);

                        if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug("Skipping up-to-date untracked file {localName}.", localName);
                            continue;
                        }

                        var extension = Path.GetExtension(localName);
                        var filterImage = ImageExtensions.Supports(extension)
                            && !pathIgnoreList.Any(x => localName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));

                        if (filterImage) {
                            await genericPublisher.PublishAsync(localName, token);
                        }
                        else {
                            await using var srcStream = reader.Open(localName);
                            await using var destStream = writer.Open(localName);
                            await srcStream.CopyToAsync(destStream, token);
                        }

                        logger.LogInformation("Published untracked file {localName}.", localName);
                        break;
                }
            }
        }

        private Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token)
        {
            var packMeta = new PackMetadata {
                PackFormat = pack.Format ?? ResourcePackProfileProperties.DefaultFormat,
                Description = pack.Description ?? string.Empty,
            };

            if (pack.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', pack.Tags)}";
            }

            var data = new {pack = packMeta};
            using var stream = writer.Open("pack.mcmeta");
            return WriteAsync(stream, data, Formatting.Indented, token);
        }

        private static async Task WriteAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
        {
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer) {Formatting = formatting};

            await JToken.FromObject(content).WriteToAsync(jsonWriter, token);
        }

        private static bool IsUpToDate(DateTime profileWriteTime, DateTime? sourceWriteTime, DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }

        private static readonly string[] pathIgnoreList = {
            Path.Combine("assets", "minecraft", "textures", "font"),
            Path.Combine("assets", "minecraft", "textures", "gui"),
            Path.Combine("assets", "minecraft", "textures", "colormap"),
            Path.Combine("assets", "minecraft", "textures", "misc"),
            Path.Combine("assets", "minecraft", "optifine", "colormap"),
        };
    }
}
