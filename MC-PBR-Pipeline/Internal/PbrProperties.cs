﻿using System;
using System.Collections.Generic;
using System.Linq;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;

namespace McPbrPipeline.Internal
{
    public class PbrProperties : PropertiesFile
    {
        public string FileName {get; set;}
        public string Name {get; set;}
        public string Alias {get; set;}
        public string Path {get; set;}
        public bool UseGlobalMatching {get; set;}

        public string DisplayName => Alias != null ? $"{Alias}:{Name}" : Name;

        public bool Wrap => Get("wrap", true);
        public bool ResizeEnabled => Get("resize.enabled", true);
        public string InputFormat => Get<string>("input.format");
        public int? RangeMin => Get<int?>("range.min");
        public int? RangeMax => Get<int?>("range.max");

        public string AlbedoTexture => Get<string>("albedo.texture");
        public byte? AlbedoValueR => Get<byte?>("albedo.value.r");
        public byte? AlbedoValueG => Get<byte?>("albedo.value.g");
        public byte? AlbedoValueB => Get<byte?>("albedo.value.b");
        public byte? AlbedoValueA => Get<byte?>("albedo.value.a");
        public float? AlbedoScaleR => Get<float?>("albedo.scale.r");
        public float? AlbedoScaleG => Get<float?>("albedo.scale.g");
        public float? AlbedoScaleB => Get<float?>("albedo.scale.b");
        public float? AlbedoScaleA => Get<float?>("albedo.scale.a");

        public string HeightTexture => Get<string>("height.texture");
        public byte? HeightValue => Get<byte?>("height.value");
        public float? HeightScale => Get<float?>("height.scale");

        public string NormalTexture => Get<string>("normal.texture");
        public byte? NormalValueX => Get<byte?>("normal.value.x");
        public byte? NormalValueY => Get<byte?>("normal.value.y");
        public byte? NormalValueZ => Get<byte?>("normal.value.z");
        public float? NormalStrength => Get<float?>("normal.strength");
        public float? NormalNoise => Get<float?>("normal.noise");

        public string OcclusionTexture => Get<string>("occlusion.texture");
        public byte? OcclusionValue => Get<byte?>("occlusion.value");
        public float? OcclusionScale => Get<float?>("occlusion.scale");
        public float OcclusionZScale => Get("occlusion.z-scale", 16f);
        public float OcclusionZBias => Get("occlusion.z-bias", 0.1f);
        public float OcclusionQuality => Get("occlusion.quality", 0.1f);
        public int OcclusionSteps => Get("occlusion.steps", 32);
        public bool? OcclusionClipEmissive => Get<bool?>("occlusion.clip-emissive");

        public string SpecularTexture => Get<string>("specular.texture");

        public string SmoothTexture => Get<string>("smooth.texture");
        public byte? SmoothValue => Get<byte?>("smooth.value");
        public float? SmoothScale => Get<float?>("smooth.scale");

        public string RoughTexture => Get<string>("rough.texture");
        public byte? RoughValue => Get<byte?>("rough.value");
        public float? RoughScale => Get<float?>("rough.scale");

        public string MetalTexture => Get<string>("metal.texture");
        public byte? MetalValue => Get<byte?>("metal.value");
        public float? MetalScale => Get<float?>("metal.scale");

        public string PorosityTexture => Get<string>("porosity.texture");
        public byte? PorosityValue => Get<byte?>("porosity.value");
        public float? PorosityScale => Get<float?>("porosity.scale");

        public string SubSurfaceScatteringTexture => Get<string>("sss.texture");
        public byte? SubSurfaceScatteringValue => Get<byte?>("sss.value");
        public float? SubSurfaceScatteringScale => Get<float?>("sss.scale");

        public string EmissiveTexture => Get<string>("emissive.texture");
        public byte? EmissiveValue => Get<byte?>("emissive.value");
        public float? EmissiveScale => Get<float?>("emissive.scale");


        internal IEnumerable<string> GetAllTextures(IInputReader reader)
        {
            return TextureTags.All
                .SelectMany(tag => reader.EnumerateTextures(this, tag))
                .Where(file => file != null).Distinct();
        }

        public PbrProperties Clone()
        {
            return new PbrProperties {
                FileName = FileName,
                Name = Name,
                Path = Path,
                UseGlobalMatching = UseGlobalMatching,
                Properties = new Dictionary<string, string>(Properties, StringComparer.InvariantCultureIgnoreCase),
            };
        }
    }
}
