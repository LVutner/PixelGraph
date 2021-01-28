﻿using PixelGraph.Common.Textures;
using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public abstract class ResourcePackChannelProperties
    {
        [YamlIgnore]
        public string ID {get;}

        public string Texture {get; set;}
        public ColorChannel? Color {get; set;}
        public string Sampler {get; set;}
        public decimal? MinValue {get; set;}
        public decimal? MaxValue {get; set;}
        public byte? RangeMin {get; set;}
        public byte? RangeMax {get; set;}
        public int? Shift {get; set;}
        public decimal? Power {get; set;}
        //public bool? Perceptual {get; set;}
        public bool? Invert {get; set;}

        [YamlIgnore]
        public bool HasTexture => Texture != null && !TextureTags.Is(Texture, TextureTags.None);

        [YamlIgnore]
        public bool HasColor => Color.HasValue && Color.Value != ColorChannel.None;

        [YamlIgnore]
        public bool HasMapping => HasTexture && HasColor;


        protected ResourcePackChannelProperties(string id)
        {
            ID = id;
        }

        protected ResourcePackChannelProperties(string id, string texture, ColorChannel color)
        {
            ID = id;
            Texture = texture;
            Color = color;
        }

        public void Merge(ResourcePackChannelProperties channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            if (channel.Texture != null) Texture = channel.Texture;
            if (channel.Sampler != null) Sampler = channel.Sampler;
            if (channel.Color.HasValue) Color = channel.Color.Value;
            if (channel.MinValue.HasValue) MinValue = channel.MinValue.Value;
            if (channel.MaxValue.HasValue) MaxValue = channel.MaxValue.Value;
            if (channel.RangeMin.HasValue) RangeMin = channel.RangeMin.Value;
            if (channel.RangeMax.HasValue) RangeMax = channel.RangeMax.Value;
            if (channel.Shift.HasValue) Shift = channel.Shift.Value;
            if (channel.Power.HasValue) Power = channel.Power.Value;
            //if (channel.Perceptual.HasValue) Perceptual = channel.Perceptual.Value;
            if (channel.Invert.HasValue) Invert = channel.Invert.Value;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            // TODO: use StringBuilder and show all options
            return $"{{{ID} {Texture}:{Color}}}";
        }
    }
}
