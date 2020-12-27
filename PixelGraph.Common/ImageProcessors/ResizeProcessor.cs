﻿using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ResizeProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public ResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TPixel2>(in PixelRowContext context, Span<TPixel2> row)
        {
            var scaleX = options.SourceWidth / context.Bounds.Width;
            var scaleY = options.SourceHeight / context.Bounds.Height;
            var fy = context.Y / scaleY;

            var pixel = new Rgba32();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var fx = x * scaleX;
                options.Sampler.Sample(fx, fy, ref pixel);
                row[x].FromRgba32(pixel);
            }
        }

        public class Options
        {
            public ISampler<TPixel> Sampler {get; set;}
            public int SourceWidth;
            public int SourceHeight;
        }
    }
}
