﻿using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ResizeProcessor<TPixel> : PixelProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public ResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var fx = context.X / (float)context.Bounds.Width * options.SourceWidth;
            var fy = context.Y / (float)context.Bounds.Height * options.SourceHeight;

            options.Sampler.Sample(fx, fy, ref pixelOut);
        }

        public class Options
        {
            public ISampler<TPixel> Sampler {get; set;}
            public int SourceWidth;
            public int SourceHeight;
        }
    }
}
