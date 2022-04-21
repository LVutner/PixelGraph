﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class BicubicSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override IRowSampler<TPixel> ForRow(in double y)
        {
            throw new System.NotImplementedException();
        }

        public override void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            SampleScaled(in x, in y, out var vector);
            pixel.FromScaledVector4(vector);
        }

        public override void SampleScaled(in double x, in double y, out Vector4 vector)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            var pxMin = (int)fx;//(fx + 0.5f);
            var pyMin = (int)fy;//(fy + 0.5f);

            var px = fx - pxMin;// + 0.5f;
            var py = fy - pyMin;// + 0.5f;

            var k = new Vector4[4][];

            for (var ky = 0; ky < 4; ky++) {
                k[ky] = new Vector4[4];
                var ly = pyMin + ky - 1;

                NormalizeTexCoordY(ref ly);

                var row = Image.DangerousGetPixelRowMemory(ly).Span;

                for (var kx = 0; kx < 4; kx++) {
                    var lx = pxMin + kx - 1;

                    NormalizeTexCoordX(ref lx);

                    k[ky][kx] = row[lx].ToScaledVector4();
                }
            }

            var col = new Vector4[4];
            CubicHermite(in k[0], in px, out col[0]);
            CubicHermite(in k[1], in px, out col[1]);
            CubicHermite(in k[2], in px, out col[2]);
            CubicHermite(in k[3], in px, out col[3]);
            CubicHermite(in col, in py, out vector);
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

        private static void CubicHermite(in Vector4[] row, in float t, out Vector4 result)
        {
            result.X = CubicHermite(in row[0].X, in row[1].X, in row[2].X, in row[3].X, in t);
            result.Y = CubicHermite(in row[0].Y, in row[1].Y, in row[2].Y, in row[3].Y, in t);
            result.Z = CubicHermite(in row[0].Z, in row[1].Z, in row[2].Z, in row[3].Z, in t);
            result.W = CubicHermite(in row[0].W, in row[1].W, in row[2].W, in row[3].W, in t);
        }

        private static float CubicHermite(in float A, in float B, in float C, in float D, in float t)
        {
            var a = -A / 2f + 3f*B / 2f - 3f*C / 2f + D / 2f;
            var b = A - 5f*B / 2f + 2f*C - D / 2f;
            var c = -A / 2f + C / 2f;
 
            return a*t*t*t + b*t*t + c*t + B;
        }
    }
}
