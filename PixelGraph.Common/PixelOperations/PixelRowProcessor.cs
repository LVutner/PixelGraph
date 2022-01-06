﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;

namespace PixelGraph.Common.PixelOperations
{
    internal abstract class PixelRowProcessor : IImageProcessor
    {
        protected const float HalfPixel = 0.5f - float.Epsilon;


        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, in sourceRectangle, ProcessRow);
        }

        //public IImageProcessor<Rgba32> CreatePixelSpecificProcessor(Configuration configuration, Image<Rgba32> source, Rectangle sourceRectangle)
        //{
        //    return new Processor<Rgba32>(configuration, source, in sourceRectangle, ProcessRow);
        //}

        protected virtual void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> row)
            where TPixel : unmanaged, IPixel<TPixel> {}

        protected virtual void ProcessRow(in PixelRowContext context, Span<Rgba32> row)
        {
            ProcessRow<Rgba32>(in context, row);
        }

        protected void GetTexCoord(in PixelRowContext context, in int x, out double fx, out double fy)
        {
            //fx = (x - context.Bounds.X + HalfPixel) / (context.Bounds.Width - 1);
            //fy = (context.Y - context.Bounds.Y + HalfPixel) / (context.Bounds.Height);

            GetTexCoord(in context, x, context.Y, out fx, out fy);
        }

        protected void GetTexCoord(in PixelRowContext context, in float x, in float y, out double fx, out double fy)
        {
            fx = (x - context.Bounds.X) / context.Bounds.Width;
            fy = (y - context.Bounds.Y) / context.Bounds.Height;
            //var innerWidth = context.Bounds.Width - 1;
            //var innerHeight = context.Bounds.Height - 1;

            //if (innerWidth <= 0 || innerHeight <= 0) {
            //    fx = fy = 0;
            //    return;
            //}

            //fx = (x - context.Bounds.X) / innerWidth;
            //fy = (y - context.Bounds.Y) / innerHeight;
        }

        private class Processor<TPixel> : ImageProcessor<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly PixelRowAction<TPixel> action;


            public Processor(Configuration configuration, Image<TPixel> source, in Rectangle sourceRectangle, PixelRowAction<TPixel> action)
                : base(configuration, source, sourceRectangle)
            {
                this.action = action;
            }

            protected override void OnFrameApply(ImageFrame<TPixel> source)
            {
                var operation = new FilterRowOperation(source, action, SourceRectangle);
                ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
            }

            private readonly struct FilterRowOperation : IRowOperation
            {
                private readonly ImageFrame<TPixel> frame;
                private readonly PixelRowAction<TPixel> action;
                private readonly Rectangle region;


                public FilterRowOperation(ImageFrame<TPixel> frame, PixelRowAction<TPixel> action, Rectangle region)
                {
                    this.frame = frame;
                    this.action = action;
                    this.region = region;
                }

                public void Invoke(int y)
                {
                    if (y < 0 || y >= frame.Height) return;

                    var context = new PixelRowContext(region, y);
                    var row = frame.GetPixelRowSpan(y);
                    action.Invoke(in context, row);
                }
            }
        }
    }
}
