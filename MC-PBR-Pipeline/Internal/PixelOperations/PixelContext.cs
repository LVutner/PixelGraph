﻿namespace McPbrPipeline.Internal.PixelOperations
{
    internal struct PixelContext
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Y;
        public int X;


        public PixelContext(int width, int height, int y)
        {
            Width = width;
            Height = height;
            Y = y;
            X = 0;
        }

        public readonly void Wrap(ref int x, ref int y)
        {
            if (x < 0) x += Width;
            if (y < 0) y += Height;

            if (x >= Width) x -= Width;
            if (y >= Height) y -= Height;
        }

        public readonly void Clamp(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x >= Width) x = Width - 1;
            if (y >= Height) y = Height - 1;
        }
    }
}
