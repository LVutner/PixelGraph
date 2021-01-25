﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class HeightVarianceProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public HeightVarianceProcessor(Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelHigh = new Rgba32();
            var pixelLow = new Rgba32();
            var pixelOut = new Rgba32(0, 0, 0, 255);

            var highFreqRow = options.HighFreqHeightImage.GetPixelRowSpan(context.Y);
            var lowFreqRow = options.LowFreqHeightImage.GetPixelRowSpan(context.Y);

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                highFreqRow[x].ToRgba32(ref pixelHigh);
                lowFreqRow[x].ToRgba32(ref pixelLow);

                pixelHigh.GetChannelValueScaledF(options.HeightChannel, out var heightLow);
                pixelLow.GetChannelValueScaledF(options.HeightChannel, out var heightHigh);

                var variance = MathF.Abs(heightHigh - heightLow); // * options.Strength;
                //MathEx.Clamp(ref variance, 0f, 1f);

                //variance = MathF.Sqrt(variance);
                //variance = MathF.Pow(variance, 2) * options.Strength;
                variance = 1f - MathF.Sqrt(1f - variance);
                variance *= options.Strength;

                MathEx.Clamp(ref variance, 0f, 1f);

                //MathEx.SmoothLerp(0f, 1f, variance, out variance);

                pixelOut.SetChannelValueScaledF(ColorChannel.Red, in variance);
                pixelOut.SetChannelValueScaledF(ColorChannel.Green, in variance);
                pixelOut.SetChannelValueScaledF(ColorChannel.Blue, in variance);
                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public Image<TPixel> HighFreqHeightImage;
            public Image<TPixel> LowFreqHeightImage;
            public ColorChannel HeightChannel = ColorChannel.Red;
            public float Strength = 1f;
        }
    }
}
