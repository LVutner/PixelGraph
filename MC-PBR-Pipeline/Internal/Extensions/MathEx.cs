﻿using System;
using System.Numerics;

namespace McPbrPipeline.Internal.Extensions
{
    internal static class MathEx
    {
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

        public static byte Clamp(float value)
        {
            return (byte)Math.Clamp(value, 0f, 255f);
        }

        public static void Saturate(float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        public static byte Saturate(float value)
        {
            return (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        public static byte Saturate(double value)
        {
            return (byte)Math.Clamp(value * 255d + 0.5d, 0, 255);
        }
    }
}
