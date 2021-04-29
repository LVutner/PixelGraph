﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialNormalProperties
    {
        public const float DefaultStrength = 1f;
        public const NormalMapMethods DefaultMethod = NormalMapMethods.Sobel3;

        public string Texture {get; set;}
        public decimal? Strength {get; set;}
        public NormalMapMethods? Method {get; set;}
        
        public ResourcePackNormalXChannelProperties InputX {get; set;}

        [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
        public decimal? ValueX {get; set;}

        public ResourcePackNormalYChannelProperties InputY {get; set;}

        [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
        public decimal? ValueY {get; set;}

        public ResourcePackNormalZChannelProperties InputZ {get; set;}

        [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
        public decimal? ValueZ {get; set;}

        #region Deprecated

        [Obsolete("Rename usages of Filter to Method.")]
        public NormalMapMethods? Filter {
            get => null;
            set => Method = value;
        }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? Noise { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveX { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveY { get; set; }

        #endregion
    }
}
