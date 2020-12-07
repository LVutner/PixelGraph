﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImageTests
{
    public class SubSurfaceScatteringTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public SubSurfaceScatteringTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                SSS = {
                    Texture = TextureTags.SubSurfaceScattering,
                    Color = ColorChannel.Red,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    SSS = {
                        Texture = TextureTags.SubSurfaceScattering,
                        Color = ColorChannel.Red,
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
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            await content.AddAsync("assets/test/sss.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_sss.png");
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
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = new MaterialSssProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_sss.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
