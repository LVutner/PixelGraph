﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverlayProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelOut = new Rgba32();

            var samplerCount = options.Samplers.Length;
            //var samplerKeys = options.Samplers.Keys.ToArray();

            float value;
            double fx, fy;
            byte finalValue, pixelValue;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixelOut);

                for (var i = 0; i < samplerCount; i++) {
                    var samplerOptions = options.Samplers[i];
                    var mapping = samplerOptions.PixelMap;

                    pixelValue = 0;
                    if (samplerOptions.Sampler != null) {
                        GetTexCoord(in context, in x, out fx, out fy);
                        samplerOptions.Sampler.Sample(fx, fy, in samplerOptions.InputColor, out pixelValue);
                    }

                    if (!mapping.TryUnmap(in pixelValue, out value)) continue;

                    if (!ProcessConversions(samplerOptions.PixelMap, ref value)) continue;

                    if (!mapping.TryMap(ref value, out finalValue)) continue;

                    if (mapping.OutputClipValue.HasValue && value.NearEqual(mapping.OutputClipValue.Value)) continue;

                    if (options.IsGrayscale) {
                        pixelOut.R = finalValue;
                        pixelOut.G = finalValue;
                        pixelOut.B = finalValue;
                    }
                    else {
                        pixelOut.SetChannelValue(samplerOptions.OutputColor, finalValue);
                    }
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        private static bool ProcessConversions(PixelMapping map, ref float value)
        {
            if (map.Convert_HcmToMetal) {
                value = value >= 230f
                    ? map.OutputMaxValue
                    : map.OutputMinValue;
            }
            else if (map.Convert_MetalToHcm) {
                var threshold = (map.InputMinValue + map.InputMaxValue) * 0.5f;
                if (value < threshold) return false;

                value = map.OutputMaxValue;
            }

            return true;
        }

        public class Options
        {
            public SamplerOptions[] Samplers;
            public bool IsGrayscale;
        }

        public class SamplerOptions
        {
            public PixelMapping PixelMap;
            public ISampler<TPixel> Sampler;
            public ColorChannel InputColor;
            public ColorChannel OutputColor;
        }
    }
}
