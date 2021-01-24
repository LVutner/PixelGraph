﻿using System;
using System.Numerics;

namespace PixelGraph.Common.Extensions
{
    internal static class MathEx
    {
        public const float Deg2Rad = (float)(Math.PI / 180);
        //public const float Deg2RadF = (float)Deg2Rad;


        public static bool Equal(this float valueA, float valueB)
        {
            return MathF.Abs(valueA - valueB) < float.Epsilon;
        }

        public static bool Equal(this double valueA, double valueB)
        {
            return Math.Abs(valueA - valueB) < double.Epsilon;
        }

        public static void Normalize(ref Vector3 value)
        {
            float length;
            if (Vector.IsHardwareAccelerated) {
                length = value.Length();
            }
            else {
                var ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                length = MathF.Sqrt(ls);
            }

            value.X /= length;
            value.Y /= length;
            value.Z /= length;
        }

        //public static void Invert(ref byte value)
        //{
        //    value = (byte) (255 - value);
        //}

        public static void Invert(ref float value)
        {
            value = 1f - value;
        }

        //public static void Invert(ref float value, in float minValue, in float maxValue)
        //{
        //    value = maxValue - (value - minValue);
        //}

        public static void Invert(ref double value, in double minValue, in double maxValue)
        {
            value = maxValue - (value - minValue);
        }

        //public static byte Clamp(int value)
        //{
        //    return (byte)Math.Clamp(value, 0, 255);
        //}

        //public static byte Clamp(int value, byte min, byte max)
        //{
        //    return (byte)Math.Clamp(value, min, max);
        //}

        //public static byte Clamp(float value, byte min, byte max)
        //{
        //    return (byte)Math.Clamp((int)(value + 0.5f), min, max);
        //}

        public static byte ClampRound(double value, byte min, byte max)
        {
            return (byte)Math.Clamp((int)(value + 0.5d), min, max);
        }

        //public static byte Clamp(float value)
        //{
        //    return (byte)Math.Clamp(value + 0.5f, 0f, 255f);
        //}

        public static void Clamp(ref float value, in float min, in float max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
        }

        public static void Clamp(ref double value, in double min, in double max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
        }

        public static void Saturate(in float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        public static void SaturateFloor(in float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f, 0f, 255f);
        }

        //public static byte Saturate(float value)
        //{
        //    return (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        //}

        //public static byte Saturate(double value)
        //{
        //    return (byte)Math.Clamp(value * 255d + 0.5d, 0, 255);
        //}

        public static void Lerp(in float min, in float max, in float mix, out float result)
        {
            result = min * (1f - mix) + max * mix;
        }

        public static void Lerp(in Vector4 min, in Vector4 max, in float mix, out Vector4 result)
        {
            Lerp(in min.X, in max.X, in mix, out result.X);
            Lerp(in min.Y, in max.Y, in mix, out result.Y);
            Lerp(in min.Z, in max.Z, in mix, out result.Z);
            Lerp(in min.W, in max.W, in mix, out result.W);
        }

        //public static void Cycle(ref byte value, in int offset)
        //{
        //    var x = value + offset;
        //    while (x < 0) x += 256;
        //    while (x >= 256) x -= 256;
        //    value = (byte)x;
        //}

        public static void Cycle(ref byte value, in int offset, in byte min, in byte max)
        {
            var x = value + offset;
            while (x < min) x += max - min + 1;
            while (x > max) x -= max - min + 1;
            value = (byte)x;
        }

        //public static void Cycle(ref float value, in int offset)
        //{
        //    const float r = (float)(256d / 255d);

        //    value += offset / 255f;
        //    while (value < 0f) value += r;
        //    while (value >= r) value -= r;
        //}

        public static void PerceptualToLinear(ref double value)
        {
            value = 1d - value;
            value *= value;
            value = 1d - value;
        }

        public static void LinearToPerceptual(ref double value)
        {
            value = 1d - Math.Sqrt(1d - value);
        }
    }
}
