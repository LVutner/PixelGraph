﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImageTests
{
    public class PorosityTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PorosityTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Porosity = {
                        Red = EncodingChannel.Porosity,
                        Include = true,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            Content.Add("assets/test/porosity.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = {
                        Input = {
                            Red = EncodingChannel.Porosity,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_p.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,  0.0,   0)]
        [InlineData(100,  1.0, 100)]
        [InlineData(100,  0.5,  50)]
        [InlineData(100,  2.0, 200)]
        [InlineData(100,  3.0, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task Scale(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_p.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
