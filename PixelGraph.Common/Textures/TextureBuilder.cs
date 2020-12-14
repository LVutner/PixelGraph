﻿using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    internal interface ITextureBuilder : IDisposable
    {
        ITextureGraph Graph {get; set;}
        MaterialProperties Material {get; set;}
        ResourcePackChannelProperties[] InputChannels {get; set;}
        ResourcePackChannelProperties[] OutputChannels {get; set;}
        Image<Rgba32> ImageResult {get;}
        bool CreateEmpty {get; set;}

        Task BuildAsync(CancellationToken token = default);
    }

    internal class TextureBuilder : ITextureBuilder
    {
        private readonly IInputReader reader;
        private Rgba32 defaultValues;

        public ITextureGraph Graph {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackChannelProperties[] InputChannels {get; set;}
        public ResourcePackChannelProperties[] OutputChannels {get; set;}
        public Image<Rgba32> ImageResult {get; private set;}
        public bool CreateEmpty {get; set;}


        public TextureBuilder(IInputReader reader)
        {
            this.reader = reader;

            defaultValues = new Rgba32();
            CreateEmpty = true;
        }

        public async Task BuildAsync(CancellationToken token = default)
        {
            var mappings = new List<TextureChannelMapping>();

            foreach (var channel in OutputChannels) {
                if (TryBuildMapping(channel, out var mapping))
                    mappings.Add(mapping);
            }

            if (!CreateEmpty && mappings.Count == 0) return;

            foreach (var mapping in mappings)
                await ApplyMappingAsync(mapping, token);

            if (ImageResult == null) {
                var size = Graph.Context.Profile.TextureSize ?? 1;
                ImageResult = new Image<Rgba32>(Configuration.Default, size, size, defaultValues);
            }
        }

        public void Dispose()
        {
            ImageResult?.Dispose();
        }

        private bool TryBuildMapping(ResourcePackChannelProperties outputChannel, out TextureChannelMapping mapping)
        {
            mapping = new TextureChannelMapping {
                OutputColor = outputChannel.Color ?? ColorChannel.None,
                OutputMin = outputChannel.MinValue ?? 0,
                OutputMax = outputChannel.MaxValue ?? 255,
                OutputShift = outputChannel.Shift ?? 0,
                OutputPower = (float?)outputChannel.Power ?? 1f,
                InvertOutput = outputChannel.Invert ?? false,

                Scale = GetChannelScale(outputChannel.ID),
            };

            var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
            var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);
            //var isOutputOcclusion = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Occlusion);

            if (TryGetChannelValue(outputChannel.ID, out var value)) {
                mapping.InputValue = value;
                return true;
            }

            var inputChannel = InputChannels.FirstOrDefault(i
                => EncodingChannel.Is(i.ID, outputChannel.ID));

            if (inputChannel?.Texture != null) {
                if (TextureTags.Is(inputChannel.Texture, TextureTags.NormalGenerated)) {
                    mapping.SourceTag = TextureTags.NormalGenerated;
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (TryGetSourceFilename(inputChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                    mapping.SourceTag = TextureTags.OcclusionGenerated;
                    //mapping.ApplyInputChannel(inputChannel);
                    mapping.InputColor = ColorChannel.Red;
                    mapping.InputMin = 0;
                    mapping.InputMax = 255;
                    mapping.InputShift = 0;
                    mapping.InputPower = 1f;
                    mapping.InvertInput = true; //(inputChannel.Invert ?? false);
                    //InputMin = channel.MinValue ?? 0;
                    //InputMax = channel.MaxValue ?? 255;
                    //InputShift = channel.Shift ?? 0;
                    //InputPower = (float?)channel.Power ?? 1f;
                    //InvertInput = channel.Invert ?? false;
                    return true;
                }
            }

            //if (isOutputOcclusion) {
            //    mapping.SourceTag = TextureTags.OcclusionGenerated;
            //    mapping.InputColor = ColorChannel.Red;
            //    return true;
            //}

            // Rough > Smooth
            if (isOutputSmooth && TryGetInputChannel(EncodingChannel.Rough, out var roughChannel)
                               && TryGetSourceFilename(roughChannel.Texture, out mapping.SourceFilename)) {
                mapping.ApplyInputChannel(roughChannel);
                mapping.InvertInput = true;
                return true;
            }

            // Smooth > Rough
            if (isOutputRough && TryGetInputChannel(EncodingChannel.Smooth, out var smoothChannel)) {
                if (TryGetSourceFilename(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InvertInput = true;
                    return true;
                }
            }

            //return false;
            if (CreateEmpty) {
                // WARN: TESTING! fallback to allow default value
                mapping.InputValue = outputChannel.MinValue ?? 0;
                //var color = outputChannel.Color ?? ColorChannel.None;
                //var defaultValue = outputChannel.MinValue ?? 0;
                //defaultValues.SetChannelValue(color, defaultValue);
                return true;
            }

            return false;
        }

        private bool TryGetInputChannel(string id, out ResourcePackChannelProperties channel)
        {
            channel = InputChannels.FirstOrDefault(i => EncodingChannel.Is(i.ID, id));
            return channel != null;
        }

        private async Task ApplyMappingAsync(TextureChannelMapping mapping, CancellationToken token)
        {
            Image<Rgba32> sourceImage = null;
            var disposeSource = false;

            try {
                var options = new OverlayProcessor.Options {
                    InputColor = mapping.InputColor,
                    InputValue = mapping.InputValue,
                    InputMin = mapping.InputMin,
                    InputMax = mapping.InputMax,
                    InputShift = mapping.InputShift,
                    InputPower = mapping.InputPower,
                    InvertInput = mapping.InvertInput,

                    OutputColor = mapping.OutputColor,
                    OutputMin = mapping.OutputMin,
                    OutputMax = mapping.OutputMax,
                    OutputShift = mapping.OutputShift,
                    OutputPower = mapping.OutputPower,
                    InvertOutput = mapping.InvertOutput,

                    Scale = mapping.Scale,
                };

                if (mapping.SourceTag != null) {
                    if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated)) {
                        sourceImage = Graph.NormalTexture;
                        options.Source = sourceImage;
                    }
                    else if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated)) {
                        sourceImage = await Graph.GetGeneratedOcclusionAsync(token);
                        options.Source = sourceImage;
                    }
                    else throw new ApplicationException($"No source mapped for tag '{mapping.SourceTag}'!");
                }
                else if (mapping.SourceFilename != null) {
                    await using var sourceStream = reader.Open(mapping.SourceFilename);
                    sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);
                    options.Source = sourceImage;
                    disposeSource = true;
                }

                if (ImageResult == null) {
                    if (sourceImage == null) {
                        var value = (mapping.InputValue ?? 0) / 255f * mapping.Scale;

                        if (MathF.Abs(options.OutputPower - 1f) > float.Epsilon) {
                            value = MathF.Pow(value, options.OutputPower);
                        }

                        if (options.InvertOutput) value = 1f - value;

                        MathEx.Saturate(value, out var finalValue);
                        MathEx.Cycle(ref finalValue, in options.OutputShift);

                        defaultValues.SetChannelValue(mapping.OutputColor, finalValue);
                        return;
                    }
                    else {
                        int width, height;
                        (width, height) = sourceImage.Size();

                        ImageResult = new Image<Rgba32>(width, height, defaultValues);
                    }
                }

                if (options.InputValue.HasValue || options.Source != null) {
                    var processor = new OverlayProcessor(options);
                    ImageResult.Mutate(context => context.ApplyProcessor(processor));
                }
            }
            finally {
                if (disposeSource) sourceImage?.Dispose();
            }
        }

        private bool TryGetSourceFilename(string tag, out string filename)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            foreach (var file in reader.EnumerateTextures(Material, tag)) {
                if (!reader.FileExists(file)) continue;

                filename = file;
                return true;
            }

            filename = null;
            return false;
        }

        private bool TryGetChannelValue(string encodingChannel, out byte value)
        {
            byte? result = null;

            if (byte.TryParse(encodingChannel, out value)) return true;

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(Material);
                value = result ?? 0;
            }
            else value = 0;

            return result.HasValue;
        }

        private float GetChannelScale(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? (float)value(Material) : 1f;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, byte?>> valueMap = new Dictionary<string, Func<MaterialProperties, byte?>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Value,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ValueRed,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ValueGreen,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ValueBlue,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ValueRed,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ValueGreen,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ValueBlue,
            [EncodingChannel.Height] = mat => mat.Height?.Value,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Value,
            [EncodingChannel.NormalX] = mat => mat.Normal?.ValueX,
            [EncodingChannel.NormalY] = mat => mat.Normal?.ValueY,
            [EncodingChannel.NormalZ] = mat => mat.Normal?.ValueZ,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.Rough] = mat => mat.Rough?.Value,
            [EncodingChannel.Metal] = mat => mat.Metal?.Value,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Value,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Value,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Value,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new Dictionary<string, Func<MaterialProperties, decimal>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Scale ?? 1m,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ScaleRed ?? 1m,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ScaleGreen ?? 1m,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ScaleBlue ?? 1m,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ScaleRed ?? 1m,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ScaleGreen ?? 1m,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ScaleBlue ?? 1m,
            [EncodingChannel.Height] = mat => mat.Height?.Scale ?? 1m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Scale ?? 1m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Scale ?? 1m,
            [EncodingChannel.Metal] = mat => mat.Metal?.Scale ?? 1m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Scale ?? 1m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Scale ?? 1m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Scale ?? 1m,
        };
    }

    internal class TextureChannelMapping
    {
        public ColorChannel InputColor;
        public byte? InputValue;
        public byte InputMin;
        public byte InputMax;
        public short InputShift;
        public float InputPower;
        public bool InvertInput;

        public ColorChannel OutputColor;
        public byte OutputMin;
        public byte OutputMax;
        public short OutputShift;
        public float OutputPower;
        public bool InvertOutput;

        public string SourceTag;
        public string SourceFilename;
        public float Scale;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            InputColor = channel.Color ?? ColorChannel.None;
            InputMin = channel.MinValue ?? 0;
            InputMax = channel.MaxValue ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (float?)channel.Power ?? 1f;
            InvertInput = channel.Invert ?? false;
        }
    }
}
