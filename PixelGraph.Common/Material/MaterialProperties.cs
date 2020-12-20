﻿using PixelGraph.Common.Encoding;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialProperties
    {
        public const bool DefaultWrap = true;
        public const string DefaultInputFormat = TextureEncoding.Format_Raw;

        [YamlIgnore]
        public string Name {get; set;}

        [YamlIgnore]
        public bool UseGlobalMatching {get; set;}

        [YamlIgnore]
        public string LocalFilename {get; set;}

        [YamlIgnore]
        public string LocalPath {get; set;}

        [YamlIgnore]
        public string Alias {get; internal set;}

        [YamlIgnore]
        public string DisplayName => Alias != null ? $"{Alias}:{Name}" : Name;

        [YamlIgnore]
        public bool IsMultiPart => Parts?.Any() ?? false;

        [YamlMember(Order = 0)]
        public string InputFormat {get; set;}

        public bool? Wrap {get; set;}

        public bool? ResizeEnabled {get; set;}

        public int? RangeMin {get; set;}

        public int? RangeMax {get; set;}

        public MaterialAlphaProperties Alpha {get; set;}

        public MaterialAlbedoProperties Albedo {get; set;}

        public MaterialDiffuseProperties Diffuse {get; set;}

        public MaterialHeightProperties Height {get; set;}

        public MaterialNormalProperties Normal {get; set;}

        public MaterialOcclusionProperties Occlusion {get; set;}

        public MaterialSpecularProperties Specular {get; set;}

        public MaterialSmoothProperties Smooth {get; set;}

        public MaterialRoughProperties Rough {get; set;}

        public MaterialMetalProperties Metal {get; set;}

        public MaterialPorosityProperties Porosity {get; set;}

        [YamlMember(Alias = "sss", ApplyNamingConventions = false)]
        public MaterialSssProperties SSS {get; set;}

        public MaterialEmissiveProperties Emissive {get; set;}

        [YamlMember(Order = 99)]
        public List<MaterialPart> Parts {get; set;}


        public bool TryGetSourceBounds(out Size size)
        {
            if (!IsMultiPart) {
                size = Size.Empty;
                return false;
            }

            size = new Size();
            foreach (var part in Parts) {
                if (part.Left.HasValue && part.Width.HasValue) {
                    var partWidth = part.Left.Value + part.Width.Value;
                    if (partWidth > size.Width) size.Width = partWidth;
                }

                if (part.Top.HasValue && part.Height.HasValue) {
                    var partHeight = part.Top.Value + part.Height.Value;
                    if (partHeight > size.Height) size.Height = partHeight;
                }
            }

            return true;
        }

        public MaterialProperties Clone()
        {
            var clone = (MaterialProperties)MemberwiseClone();

            // TODO: clone child data

            return clone;
        }

        public bool TryGetChannelValue(string encodingChannel, out byte value)
        {
            byte? result = null;

            if (byte.TryParse(encodingChannel, out value)) return true;

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(this);
                value = result ?? 0;
            }
            else value = 0;

            return result.HasValue;
        }

        public float GetChannelScale(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? (float)value(this) : 1f;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, byte?>> valueMap = new Dictionary<string, Func<MaterialProperties, byte?>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Value,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ValueRed,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ValueGreen,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ValueBlue,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ValueRed,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ValueGreen,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ValueBlue,
            [EncodingChannel.Height] = mat => mat.Height?.Value,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Value,
            [EncodingChannel.NormalX] = mat => mat.Normal?.ValueX,
            [EncodingChannel.NormalY] = mat => mat.Normal?.ValueY,
            [EncodingChannel.NormalZ] = mat => mat.Normal?.ValueZ,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.Rough] = mat => mat.Rough?.Value,
            [EncodingChannel.Metal] = mat => mat.Metal?.Value,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Value,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Value,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Value,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new Dictionary<string, Func<MaterialProperties, decimal>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Scale ?? 1m,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ScaleRed ?? 1m,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ScaleGreen ?? 1m,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ScaleBlue ?? 1m,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ScaleRed ?? 1m,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ScaleGreen ?? 1m,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ScaleBlue ?? 1m,
            [EncodingChannel.Height] = mat => mat.Height?.Scale ?? 1m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Scale ?? 1m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Scale ?? 1m,
            [EncodingChannel.Metal] = mat => mat.Metal?.Scale ?? 1m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Scale ?? 1m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Scale ?? 1m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Scale ?? 1m,
        };
    }
}
