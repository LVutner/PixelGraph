﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Effects;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface ITexturePreviewBuilder : IDisposable
    {
        ResourcePackInputProperties Input {get; set;}
        MaterialProperties Material {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        CancellationToken Token {get;}

        Task<Image> BuildAsync(string tag, int? targetFrame = null, int? targetPart = null);
        void Cancel();
    }

    internal abstract class TexturePreviewBuilderBase : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly CancellationTokenSource tokenSource;

        public ResourcePackInputProperties Input {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}

        protected IDictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> TagMap {get; set;}
        public CancellationToken Token => tokenSource.Token;


        protected TexturePreviewBuilderBase(IServiceProvider provider)
        {
            this.provider = provider;
            tokenSource = new CancellationTokenSource();
        }

        public async Task<Image> BuildAsync(string tag, int? targetFrame = 0, int? targetPart = null)
        {
            var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graph = scope.ServiceProvider.GetRequiredService<ITextureGraph>();
            var regions = scope.ServiceProvider.GetRequiredService<ITextureRegionEnumerator>();
            var edgeFadeEffect = scope.ServiceProvider.GetRequiredService<IEdgeFadeImageEffect>();
            var reader = scope.ServiceProvider.GetRequiredService<IInputReader>();

            context.Input = Input;
            context.Profile = Profile;
            context.Material = Material;

            var matMetaFileIn = context.GetMetaInputFilename();
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            var inputEncoding = GetEncoding(Input?.Format);
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);
            context.InputEncoding = inputEncoding.GetMapped().ToList();

            if (TryGetChannels(tag, out var channels))
                context.OutputEncoding.AddRange(channels);

            if (TextureTags.Is(tag, TextureTags.Normal))
                await graph.PreBuildNormalTextureAsync(tokenSource.Token);

            await graph.MapAsync(tag, true, targetFrame, targetPart, Token);

            var image = await graph.CreateImageAsync<Rgba32>(tag, true, tokenSource.Token);
            if (image == null) return null;

            try {
                if (TextureTags.Is(tag, TextureTags.Height) && (image.Width > 1 || image.Height > 1)) {
                    foreach (var part in regions.GetAllPublishRegions(context.MaxFrameCount, targetFrame, targetPart)) {
                        foreach (var frame in part.Frames) {
                            var outBounds = targetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                                : frame.SourceBounds.ScaleTo(image.Width, image.Height);

                            edgeFadeEffect.Apply(image, tag, outBounds);
                        }
                    }
                }

                return image;
            }
            catch {
                image.Dispose();
                throw;
            }
        }

        //public async Task<ImageSource> BuildSourceAsync(string tag, int targetFrame = 0)
        //{
        //    using var image = await BuildAsync(tag, targetFrame);
        //    return await CreateImageSourceAsync(image, tokenSource.Token);
        //}

        public void Cancel()
        {
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        private bool TryGetChannels(string textureTag, out ResourcePackChannelProperties[] channels)
        {
            if (TagMap.TryGetValue(textureTag, out var channelFunc)) {
                channels = channelFunc(Profile);
                return true;
            }

            channels = null;
            return false;
        }

        private static ResourcePackEncoding GetEncoding(string format)
        {
            ResourcePackEncoding encoding = null;

            if (format != null) {
                var factory = TextureFormat.GetFactory(format);
                if (factory != null) encoding = factory.Create();
            }

            return encoding ?? new ResourcePackEncoding();
        }
    }
}
