﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraphBuilder
    {
        bool UseGlobalOutput {get; set;}

        ITextureGraph BuildInputGraph(MaterialContext context);
        Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default);
        Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default);
    }

    internal class TextureGraphBuilder : ITextureGraphBuilder
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly IImageWriter imageWriter;
        private readonly INamingStructure naming;
        private readonly ILogger logger;

        public bool UseGlobalOutput {get; set;}


        public TextureGraphBuilder(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            writer = provider.GetRequiredService<IOutputWriter>();
            imageWriter = provider.GetRequiredService<IImageWriter>();
            naming = provider.GetRequiredService<INamingStructure>();
            logger = provider.GetRequiredService<ILogger<TextureGraphBuilder>>();
        }

        public ITextureGraph BuildInputGraph(MaterialContext context)
        {
            var graph = new TextureGraph(provider, reader, context);
            graph.BuildFromInput();
            return graph;
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            using var graph = BuildInputGraph(context);

            foreach (var tag in TextureTags.All) {
                var encoding = context.Profile.Output.GetFinalTextureEncoding(tag);

                if (encoding?.Include ?? false)
                    await ProcessTextureAsync(graph, encoding, tag, token);
            }
        }

        /// <summary>
        /// Output -> Input; for importing textures
        /// </summary>
        public async Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            //using var graph = BuildOutputGraph(context);
            using var graph = new TextureGraph(provider, reader, context);
            graph.BuildFromOutput();
            graph.CreateEmpty = false;

            foreach (var tag in TextureTags.All) {
                var encoding = context.Input.GetFormatEncoding(tag);

                //if (encoding?.Include ?? false)
                await ProcessTextureAsync(graph, encoding, tag, token);
            }
        }

        private async Task ProcessTextureAsync(ITextureGraph graph, TextureEncoding encoding, string tag, CancellationToken token)
        {
            using var image = await graph.BuildFinalImageAsync(encoding, token);

            var imageFormat = graph.Context.Profile.ImageEncoding
                ?? ResourcePackProfileProperties.DefaultImageEncoding;

            if (image != null) {
                var p = graph.Context.Material.LocalPath;
                if (!UseGlobalOutput) p = PathEx.Join(p, graph.Context.Material.Name);

                if (graph.Context.Material.IsMultiPart) {
                    foreach (var region in graph.Context.Material.Parts) {
                        var name = naming.GetOutputTextureName(graph.Context.Profile, region.Name, tag, UseGlobalOutput);
                        var destFile = PathEx.Join(p, name);

                        if (image.Width == 1 && image.Height == 1) {
                            await imageWriter.WriteAsync(image, destFile, imageFormat, token);
                        }
                        else {
                            using var regionImage = GetImageRegion(image, region.GetRectangle());

                            // TODO: move resize before regions; then scale regions to match
                            Resize(graph.Context, regionImage, tag);

                            await imageWriter.WriteAsync(regionImage, destFile, imageFormat, token);
                        }

                        logger.LogInformation("Published texture region {Name} tag {tag}.", region.Name, tag);
                    }
                }
                else {
                    var name = naming.GetOutputTextureName(graph.Context.Profile, graph.Context.Material.Name, tag, UseGlobalOutput);
                    var destFile = PathEx.Join(p, name);

                    Resize(graph.Context, image, tag);

                    await imageWriter.WriteAsync(image, destFile, imageFormat, token);

                    logger.LogInformation("Published texture {DisplayName} tag {tag}.", graph.Context.Material.DisplayName, tag);
                }
            }

            await CopyMetaAsync(graph.Context, tag, token);
        }

        private static void Resize(MaterialContext context, Image image, string tag)
        {
            if (!(context.Material?.ResizeEnabled ?? true)) return;
            if (!context.Profile.TextureSize.HasValue && !context.Profile.TextureScale.HasValue) return;

            var (width, height) = image.Size();

            var encoding = context.Profile.Output.GetFinalTextureEncoding(tag);
            var samplerName = encoding.Sampler;

            var resampler = KnownResamplers.Bicubic;
            if (samplerName != null && Samplers.TryParse(samplerName, out var _resampler))
                resampler = _resampler;

            if (context.Profile.TextureSize.HasValue) {
                if (width == context.Profile.TextureSize) return;

                image.Mutate(c => c.Resize(context.Profile.TextureSize.Value, 0, resampler));
            }
            else {
                var targetWidth = (int)Math.Max(width * context.Profile.TextureScale.Value, 1f);
                var targetHeight = (int)Math.Max(height * context.Profile.TextureScale.Value, 1f);

                image.Mutate(c => c.Resize(targetWidth, targetHeight, resampler));
            }
        }

        private Image<Rgba32> GetImageRegion(Image<Rgba32> sourceImage, Rectangle bounds)
        {
            Image<Rgba32> regionImage = null;
            try {
                regionImage = new Image<Rgba32>(Configuration.Default, bounds.Width, bounds.Height);

                // TODO: copy region from source
                var options = new RegionProcessor.Options {
                    Source = sourceImage,
                    Offset = bounds.Location,
                };

                regionImage.Mutate(context => {
                    var processor = new RegionProcessor(options);
                    context.ApplyProcessor(processor);
                });

                return regionImage;
            }
            catch {
                regionImage?.Dispose();
                throw;
            }
        }

        private async Task CopyMetaAsync(MaterialContext context, string tag, CancellationToken token)
        {
            var metaFileIn = naming.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) return;

            var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, UseGlobalOutput);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }
    }
}
