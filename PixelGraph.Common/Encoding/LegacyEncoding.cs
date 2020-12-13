﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class LegacyEncoding : ITextureEncodingFactory
    {
        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseRed = new ResourcePackDiffuseRedChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseGreen = new ResourcePackDiffuseGreenChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseBlue = new ResourcePackDiffuseBlueChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Specular = new ResourcePackSpecularChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },
            };
        }
    }
}
