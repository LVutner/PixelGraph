﻿using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Filters
{
    internal class ResizeFilter : IImageFilter
    {
        public string Sampler {get; set;}
        public int TargetSize {get; set;}


        public void Apply(IImageProcessingContext context)
        {
            var size = context.GetCurrentSize();
            if (size.Width == TargetSize) return;

            var resampler = KnownResamplers.Bicubic;

            if (Sampler != null && SamplerMap.TryGetValue(Sampler, out var _resampler))
                resampler = _resampler;

            context.Resize(TargetSize, 0, resampler);
        }

        private static readonly Dictionary<string, IResampler> SamplerMap = new Dictionary<string, IResampler>(StringComparer.InvariantCultureIgnoreCase) {
            ["point"] = KnownResamplers.NearestNeighbor,
            ["box"] = KnownResamplers.Box,
            ["bilinear"] = KnownResamplers.Triangle,
            ["triangle"] = KnownResamplers.Triangle,
            ["cubic"] = KnownResamplers.CatmullRom,
            ["catmull-rom"] = KnownResamplers.CatmullRom,
            ["bicubic"] = KnownResamplers.Bicubic,
            ["hermite"] = KnownResamplers.Hermite,
            ["spline"] = KnownResamplers.Spline,
            ["welch"] = KnownResamplers.Welch,
            ["lanczos-2"] = KnownResamplers.Lanczos2,
            ["lanczos-3"] = KnownResamplers.Lanczos3,
            ["lanczos-5"] = KnownResamplers.Lanczos5,
            ["lanczos-8"] = KnownResamplers.Lanczos8,
        };
    }
}
