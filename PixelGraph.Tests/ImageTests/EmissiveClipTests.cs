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
    public class EmissiveClipTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public EmissiveClipTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Emissive = {
                    Texture = TextureTags.Emissive,
                    Color = ColorChannel.Red,
                    Shift = -1,
                }
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                        Shift = -1,
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

            using var emissiveImage = CreateImageR(value);
            await content.AddAsync("assets/test/emissive.png", emissiveImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,  0.0, 255)]
        [InlineData(100,  1.0,  99)]
        [InlineData(100,  0.5,  49)]
        [InlineData(100,  2.0, 199)]
        [InlineData(100,  3.0, 254)]
        [InlineData(200, 0.01,   1)]
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
                    Emissive = new MaterialEmissiveProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 255)]
        [InlineData(  1,   0)]
        [InlineData(128, 127)]
        [InlineData(255, 254)]
        [Theory] public async Task ConvertsEmissiveToEmissiveClipped(byte value, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;
            
            using var emissiveImage = CreateImageR(value);
            await content.AddAsync("assets/test/emissive.png", emissiveImage);

            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                    },
                },
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
