﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRotateProcessor : PixelRowProcessor
    {
        private readonly Options options;
        private readonly bool hasRotation, hasNoise;
        private readonly bool hasCurveTop, hasCurveBottom, hasCurveLeft, hasCurveRight;
        private readonly float offsetTop, offsetBottom, offsetLeft, offsetRight;
        private readonly float invRadiusTop, invRadiusBottom, invRadiusLeft, invRadiusRight;


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            hasCurveTop = MathF.Abs(options.CurveTop) > float.Epsilon && options.RadiusTop > float.Epsilon;
            hasCurveBottom = MathF.Abs(options.CurveBottom) > float.Epsilon && options.RadiusBottom > float.Epsilon;
            hasCurveLeft = MathF.Abs(options.CurveLeft) > float.Epsilon && options.RadiusLeft > float.Epsilon;
            hasCurveRight = MathF.Abs(options.CurveRight) > float.Epsilon && options.RadiusRight > float.Epsilon;

            hasRotation = hasCurveTop || hasCurveBottom || hasCurveLeft || hasCurveRight;
            hasNoise = options.Noise > float.Epsilon;

            if (hasCurveTop) {
                offsetTop = 1f - options.RadiusTop;
                invRadiusTop = 1f / options.RadiusTop;
            }
            else invRadiusTop = offsetTop = 0f;

            if (hasCurveBottom) {
                offsetBottom = 1f - options.RadiusBottom;
                invRadiusBottom = 1f / options.RadiusBottom;
            }
            else invRadiusBottom = offsetBottom = 0f;

            if (hasCurveLeft) {
                offsetLeft = 1f - options.RadiusLeft;
                invRadiusLeft = 1f / options.RadiusLeft;
            }
            else invRadiusLeft = offsetLeft = 0f;

            if (hasCurveRight) {
                offsetRight = 1f - options.RadiusRight;
                invRadiusRight = 1f / options.RadiusRight;
            }
            else invRadiusRight = offsetRight = 0f;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixel = new Rgba32();
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                if (x < 0 || x >= row.Length) continue;

                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(ColorChannel.Red, out var normalX);
                pixel.GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (!options.RestoreNormalZ) {
                    pixel.GetChannelValue(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ / 127f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                ProcessPixel(in context, ref v, in noiseX, in noiseY, in x);

                pixel.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);

                row[x].FromRgba32(pixel);
            }
        }

        protected override void ProcessRow(in PixelRowContext context, Span<Rgba32> row)
        {
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].GetChannelValue(ColorChannel.Red, out var normalX);
                row[x].GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (!options.RestoreNormalZ) {
                    row[x].GetChannelValueScaled(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ / 127f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                ProcessPixel(in context, ref v, in noiseX, in noiseY, in x);

                row[x].SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                row[x].SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                row[x].SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);
            }
        }

        private void ProcessPixel(in PixelRowContext context, ref Vector3 v, in byte[] noiseX, in byte[] noiseY, in int x)
        {
            if (hasRotation || hasNoise) {
                var angleX = MathEx.AsinD(v.X);
                var angleY = MathEx.AsinD(v.Y);
                float fx, fy, rx, ry;

                if (hasCurveTop) {
                    fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;
                    ry = MathF.Min(fy + offsetTop, 0f) * invRadiusTop;
                    angleY += options.CurveTop * ry;
                }

                if (hasCurveBottom) {
                    fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;
                    ry = MathF.Max(fy - offsetBottom, 0f) * invRadiusBottom;
                    angleY += options.CurveBottom * ry;
                }

                if (hasCurveLeft) {
                    fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                    rx = MathF.Min(fx + offsetLeft, 0f) * invRadiusLeft;
                    angleX += options.CurveLeft * rx;
                }

                if (hasCurveRight) {
                    fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                    rx = MathF.Max(fx - offsetRight, 0f) * invRadiusRight;
                    angleX += options.CurveRight * rx;
                }

                if (hasNoise) {
                    var z = x - context.Bounds.Left;
                    angleX += (noiseX[z] / 127f - 1f) * options.Noise;
                    angleY += (noiseY[z] / 127f - 1f) * options.Noise;
                }

                AngleToVector(in angleX, in angleY, out v);
            }
            else if (options.RestoreNormalZ) {
                var v2 = new Vector2(v.X, v.Y);
                var d = Vector2.Dot(v2, v2);
                v.Z = MathF.Sqrt(1f - d);
            }
        }

        private static void AngleToVector(in float angleX, in float angleY, out Vector3 vector)
        {
            //MathEx.Clamp(ref angleX, -90f, 90f);
            //MathEx.Clamp(ref angleY, -90f, 90f);

            var sinX = MathEx.SinD(angleX);
            var cosX = MathEx.CosD(angleX);
            var sinY = MathEx.SinD(angleY);
            var cosY = MathEx.CosD(angleY);

            vector.X = sinX * cosY;
            vector.Y = sinY * cosX;
            vector.Z = cosX * cosY;
            MathEx.Normalize(ref vector);
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }

        public class Options
        {
            public bool RestoreNormalZ;
            public float CurveTop = 0f;
            public float CurveBottom = 0f;
            public float CurveLeft = 0f;
            public float CurveRight = 0f;
            public float RadiusTop = 1f;
            public float RadiusBottom = 1f;
            public float RadiusLeft = 1f;
            public float RadiusRight = 1f;
            public float Noise = 0f;
        }
    }
}
