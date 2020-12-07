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
    public class NormalTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public NormalTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Height = {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                },
                NormalX = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                },
                NormalY = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                },
                //NormalZ = {
                //    Texture = TextureTags.Normal,
                //    Color = ColorChannel.Blue,
                //},
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    NormalX = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                    },
                    NormalY = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                    },
                    NormalZ = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task PassthroughX(byte value)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var sourceImage = CreateImageR(value);
            await content.AddAsync("assets/test/normal.png", sourceImage);
            
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var resultImage = await content.OpenImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(value, resultImage);
        }

        [InlineData(127, 127, 255)]
        [InlineData(  0, 127,   0)]
        [InlineData(127,   0,   0)]
        [Theory] public async Task RestoreZ(byte valueX, byte valueY, byte expectedZ)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var sourceImage = CreateImage(valueX, valueY, 0);
            await content.AddAsync("assets/test/normal.png", sourceImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(valueX, image);
            PixelAssert.GreenEquals(valueY, image);
            PixelAssert.BlueEquals(expectedZ, image);
        }
    }
}
