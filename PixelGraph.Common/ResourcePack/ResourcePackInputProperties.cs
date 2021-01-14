﻿namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackInputProperties : ResourcePackEncoding
    {
        public string Format {get; set;}


        public override object Clone()
        {
            var clone = (ResourcePackInputProperties)base.Clone();

            clone.Format = Format;

            return clone;
        }
    }
}
