using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class NormalTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public NormalTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            packInput = new ResourcePackInputProperties {
                NormalX = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalY = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalZ = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 1m,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    NormalX = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    NormalY = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    NormalZ = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 1m,
                    },
                },
            };
        }

        [InlineData(127, 127, 255)]
        [InlineData(  0, 127, 127)]
        [InlineData(255, 127, 127)]
        [InlineData(127,   0, 127)]
        [InlineData(127, 255, 127)]
        [Theory] public async Task Passthrough(byte valueX, byte valueY, byte valueZ)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/normal.png", valueX, valueY, valueZ);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(valueX, image);
            PixelAssert.GreenEquals(valueY, image);
            PixelAssert.BlueEquals(valueZ, image);
        }

        [InlineData(128, 128, 255)]
        [InlineData(  0, 128, 128)]
        [InlineData(128,   0, 128)]
        [Theory] public async Task RestoreZ(byte valueX, byte valueY, byte expectedZ)
        {
            await using var graph = Graph();

            graph.PackInput = new ResourcePackInputProperties {
                NormalX = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalY = {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
            };

            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/normal.png", valueX, valueY, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(valueX, image);
            PixelAssert.GreenEquals(valueY, image);
            PixelAssert.BlueEquals(expectedZ, image);
        }
    }
}
