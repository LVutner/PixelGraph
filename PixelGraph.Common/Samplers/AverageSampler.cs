﻿using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float x, in float y, ref Rgba32 pixel)
        {
            var fx = x * Image.Width;
            var fy = y * Image.Height;

            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)Math.Ceiling(minRangeX);
            var stepY = (int)Math.Ceiling(minRangeY);

            var pxMin = (int)fx;
            var pyMin = (int)fy;
            var pxMax = pxMin + stepX;
            var pyMax = pyMin + stepY;

            //var f = 1f / (stepX * stepY);
            var color = new Vector4();

            for (var py = pyMin; py < pyMax; py++) {
                var _py = py;

                if (WrapY) WrapCoordY(ref _py);
                else ClampCoordY(ref _py);

                for (var px = pxMin; px < pxMax; px++) {
                    var _px = px;

                    if (WrapX) WrapCoordX(ref _px);
                    else ClampCoordX(ref _px);

                    color += Image[_px, _py].ToScaledVector4();
                }
            }

            var h = stepX * stepY;
            pixel.FromScaledVector4(color / h);
        }
    }
}
