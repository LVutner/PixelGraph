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

        public static byte Saturate(float value)
        {
            return (byte)Math.Clamp(value * 255f, 0f, 255f);
        }

        public static byte Saturate(double value)
        {
            return (byte)Math.Clamp(value * 255, 0, 255);
        }

        public static float Max(params float[] values)
        {
            float result = 0;

            for (var i = 0; i < values.Length; i++) {
                if (i == 0) result = values[i];
                else if (values[i] > result)
                    result = values[i];
            }

            return result;
        }
    }
}
