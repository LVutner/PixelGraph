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
    public class HeightTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public HeightTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            packInput = new ResourcePackInputProperties {
                Height = {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    Power = 1,
                    Invert = true,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Height = {
                        Texture = TextureTags.Height,
                        Color = ColorChannel.Red,
                        Power = 1,
                        Invert = true,
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
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/height.png", value, 0, 0);
            await graph.ProcessAsync();
            
            using var image = await graph.GetImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(value, image);
        }

        //[InlineData(0.000, 0.00, 255)]
        //[InlineData(1.000, 0.00, 255)]
        //[InlineData(0.392, 1.00, 155)]
        //[InlineData(0.392, 0.50, 205)]
        //[InlineData(0.392, 2.00,  55)]
        //[InlineData(0.392, 3.00,   0)]
        //[InlineData(0.784, 0.01, 253)]
        //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Height = new MaterialHeightProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_h.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(  0, 0.00, 255)]
        [InlineData(255, 0.00, 255)]
        [InlineData(100, 1.00, 100)]
        [InlineData(155, 0.50, 205)]
        [InlineData(155, 2.00,  55)]
        [InlineData(155, 3.00,   0)]
        [InlineData( 55, 0.01, 253)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Height = new MaterialHeightProperties {
                    Scale = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/height.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
