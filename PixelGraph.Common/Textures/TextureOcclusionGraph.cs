﻿using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureOcclusionGraph
    {
        ResourcePackChannelProperties Channel {get;}
        int FrameCount {get;}
        int FrameWidth {get;}
        int FrameHeight {get;}

        Task<Image<Rgba32>> GetTextureAsync(CancellationToken token = default);
        Task ApplyMagnitudeAsync(Image normalImage, ResourcePackChannelProperties magnitudeChannel, CancellationToken token = default);
        Task<Image<Rgba32>> GenerateAsync(CancellationToken token = default);
        Task<ISampler<Rgba32>> GetSamplerAsync(CancellationToken token = default);
    }
    
    internal class TextureOcclusionGraph : ITextureOcclusionGraph, IDisposable
    {
        private readonly IInputReader reader;
        private readonly ITextureGraphContext context;
        private readonly ITextureSourceGraph sourceGraph;
        private readonly ITextureRegionEnumerator regions;
        private Image<Rgba32> texture;
        private bool isLoaded;

        public ResourcePackChannelProperties Channel {get; private set;}
        public int FrameCount {get; private set;}
        public int FrameWidth {get; private set;}
        public int FrameHeight {get; private set;}


        public TextureOcclusionGraph(
            IInputReader reader,
            ITextureGraphContext context,
            ITextureSourceGraph sourceGraph,
            ITextureRegionEnumerator regions)
        {
            this.reader = reader;
            this.context = context;
            this.sourceGraph = sourceGraph;
            this.regions = regions;

            FrameCount = 1;
        }

        public void Dispose()
        {
            texture?.Dispose();
        }

        public async Task<Image<Rgba32>> GetTextureAsync(CancellationToken token = default)
        {
            if (isLoaded) return texture;
            isLoaded = true;

            var occlusionChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Occlusion));

            // TODO: load file
            var occlusionFile = reader.EnumerateTextures(context.Material, TextureTags.Occlusion).FirstOrDefault();

            if (occlusionFile != null) {
                await using var stream = reader.Open(occlusionFile);
                texture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                Channel = occlusionChannel;
            }

            if (texture == null) {
                texture = await GenerateAsync(token);

                Channel = new ResourcePackOcclusionChannelProperties {
                    Sampler = occlusionChannel?.Sampler,
                    Color = ColorChannel.Red,
                    Invert = true,
                };
            }

            if (texture != null) {
                FrameWidth = texture.Width;
                FrameHeight = texture.Height;
                if (FrameCount > 1) FrameHeight /= FrameCount;
            }

            return texture;
        }

        public async Task<ISampler<Rgba32>> GetSamplerAsync(CancellationToken token = default)
        {
            var occlusionTexture = await GetTextureAsync(token);
            if (occlusionTexture == null) return null;

            var samplerName = Channel?.Sampler ?? context.DefaultSampler;
            var sampler = Sampler<Rgba32>.Create(samplerName);
            sampler.Image = occlusionTexture;
            sampler.WrapX = context.MaterialWrapX;
            sampler.WrapY = context.MaterialWrapY;

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }

        public async Task<Image<Rgba32>> GenerateAsync(CancellationToken token = default)
        {
            var heightChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));
            if (heightChannel == null) return null;

            var info = await GetHeightSourceAsync(token);
            if (info == null) return null;

            await using var stream = reader.Open(info.LocalFile);
            using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
            FrameCount = info.FrameCount;

            var aspect = (float)heightTexture.Height / heightTexture.Width;
            var bufferSize = context.GetBufferSize(aspect);

            var occlusionWidth = bufferSize?.Width ?? heightTexture.Width;
            var occlusionHeight = bufferSize?.Height ?? heightTexture.Height;

            var heightScale = 1f;
            if (bufferSize.HasValue)
                heightScale = (float)bufferSize.Value.Width / heightTexture.Width;

            var samplerName = context.Profile?.Encoding?.Height?.Sampler ?? context.DefaultSampler;
            var heightSampler = Sampler<Rgba32>.Create(samplerName);
            heightSampler.Image = heightTexture;
            heightSampler.WrapX = context.MaterialWrapX;
            heightSampler.WrapY = context.MaterialWrapY;

            heightSampler.RangeX = (float)heightTexture.Width / occlusionWidth;
            heightSampler.RangeY = (float)heightTexture.Height / occlusionHeight;

            var stepDistance = (float?)context.Material.Occlusion?.StepDistance ?? MaterialOcclusionProperties.DefaultStepDistance;

            var options = new OcclusionProcessor<Rgba32>.Options {
                HeightSampler = heightSampler,
                HeightChannel = heightChannel.Color ?? ColorChannel.Red,
                HeightMinValue = (float?)heightChannel.MinValue ?? 0f,
                HeightMaxValue = (float?)heightChannel.MaxValue ?? 1f,
                HeightRangeMin = heightChannel.RangeMin ?? 0,
                HeightRangeMax = heightChannel.RangeMax ?? 255,
                HeightShift = heightChannel.Shift ?? 0,
                HeightPower = (float?)heightChannel.Power ?? 0f,
                HeightInvert = heightChannel.Invert ?? false,

                Quality = (float?)context.Material.Occlusion?.Quality ?? MaterialOcclusionProperties.DefaultQuality,
                ZScale = (float?)context.Material.Occlusion?.ZScale ?? MaterialOcclusionProperties.DefaultZScale,
                ZBias = (float?)context.Material.Occlusion?.ZBias ?? MaterialOcclusionProperties.DefaultZBias,

                Token = token,
            };

            // adjust volume height with texture scale
            options.ZBias *= heightScale;
            options.ZScale *= heightScale;

            var processor = new OcclusionProcessor<Rgba32>(options);
            var occlusionTexture = new Image<Rgba32>(Configuration.Default, occlusionWidth, occlusionHeight);

            try {
                foreach (var frame in regions.GetAllRenderRegions(null, FrameCount)) {
                    foreach (var tile in frame.Tiles) {
                        heightSampler.Bounds = tile.Bounds;

                        var outBounds = tile.Bounds.ScaleTo(occlusionWidth, occlusionHeight);
                        options.StepCount = (int) MathF.Max(outBounds.Width * stepDistance, 1f);

                        occlusionTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }

                return occlusionTexture;
            }
            catch (ImageProcessingException) when (token.IsCancellationRequested) {
                throw new OperationCanceledException();
            }
            catch {
                occlusionTexture.Dispose();
                throw;
            }
        }

        public async Task ApplyMagnitudeAsync(Image normalImage, ResourcePackChannelProperties magnitudeChannel, CancellationToken token = default)
        {
            var options = new NormalMagnitudeProcessor<Rgba32>.Options {
                MagSource = await GetTextureAsync(token),
                Scale = context.Material.GetChannelScale(magnitudeChannel.ID),
                InputChannel = Channel?.Color ?? ColorChannel.Red,
                InputMinValue = (float?)Channel?.MinValue ?? 0f,
                InputMaxValue = (float?)Channel?.MaxValue ?? 1f,
                InputRangeMin = Channel?.RangeMin ?? 0,
                InputRangeMax = Channel?.RangeMax ?? 255,
                InputPower = (float?)Channel?.Power ?? 1f,
                InputInvert = Channel?.Invert ?? false,
            };

            if (options.MagSource == null) return;
            options.ApplyOutputChannel(magnitudeChannel);

            var processor = new NormalMagnitudeProcessor<Rgba32>(options);
            normalImage.Mutate(c => c.ApplyProcessor(processor));
        }

        private async Task<TextureSource> GetHeightSourceAsync(CancellationToken token)
        {
            var file = reader.EnumerateTextures(context.Material, TextureTags.Bump).FirstOrDefault();

            if (file != null) {
                var info = await sourceGraph.GetOrCreateAsync(file, token);
                if (info != null) return info;
            }

            file = reader.EnumerateTextures(context.Material, TextureTags.Height).FirstOrDefault();

            if (file != null) {
                var info = await sourceGraph.GetOrCreateAsync(file, token);
                if (info != null) return info;
            }

            return null;
        }
    }
}
