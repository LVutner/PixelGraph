﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class BilinearSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            SampleScaled(in x, in y, out var vector);
            pixel.FromScaledVector4(vector);
        }

        public override void SampleScaled(in double x, in double y, out Vector4 vector)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            var pxMin = (int)fx; //(fx + 0.5f);
            var pxMax = pxMin + 1;
            var pyMin = (int)fy; //(fy + 0.5f);
            var pyMax = pyMin + 1;

            var px = fx - pxMin;// - 0.5f;
            var py = fy - pyMin;// - 0.5f;

            if (WrapX) {
                WrapCoordX(ref pxMin);
                WrapCoordX(ref pxMax);
            }
            else {
                ClampCoordX(ref pxMin);
                ClampCoordX(ref pxMax);
            }

            if (WrapY) {
                WrapCoordY(ref pyMin);
                WrapCoordY(ref pyMax);
            }
            else {
                ClampCoordY(ref pyMin);
                ClampCoordY(ref pyMax);
            }

            var rowMin = Image.DangerousGetPixelRowMemory(pyMin).Span;
            var rowMax = Image.DangerousGetPixelRowMemory(pyMax).Span;

            var pixelMatrix = new Vector4[4];
            pixelMatrix[0] = rowMin[pxMin].ToScaledVector4();
            pixelMatrix[1] = rowMin[pxMax].ToScaledVector4();
            pixelMatrix[2] = rowMax[pxMin].ToScaledVector4();
            pixelMatrix[3] = rowMax[pxMax].ToScaledVector4();

            MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
            MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
            MathEx.Lerp(in zMin, in zMax, in py, out vector);
        }

        public override void Sample(in double fx, in double fy, in ColorChannel color, out byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in fx, in fy, ref pixel);
            pixel.GetChannelValue(color, out pixelValue);
        }

        public override void SampleScaled(in double fx, in double fy, in ColorChannel color, out float pixelValue)
        {
            SampleScaled(in fx, in fy, out var vector);
            vector.GetChannelValue(color, out pixelValue);
        }
    }
}
