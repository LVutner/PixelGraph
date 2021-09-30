﻿using PixelGraph.Common.IO;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.TextureFormats.Bedrock;
using PixelGraph.Common.TextureFormats.Java;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class AllTextureFormatValues : List<TextureFormatValueItem>
    {
        public AllTextureFormatValues()
        {
            Add(new TextureFormatValueItem {
                Text = "Raw",
                Value = TextureFormat.Format_Raw,
                Hint = RawFormat.Description,
                GameEditions = new [] {GameEditions.Java, GameEditions.Bedrock},
            });
            Add(new TextureFormatValueItem {
                Text = "Color",
                Value = TextureFormat.Format_Color,
                Hint = ColorFormat.Description,
                GameEditions = new [] {GameEditions.Java, GameEditions.Bedrock},
            });
            Add(new TextureFormatValueItem {
                Text = "Specular",
                Value = TextureFormat.Format_Specular,
                Hint = SpecularFormat.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "Vanilla",
                Value = TextureFormat.Format_VanillaPbr,
                Hint = VanillaPbrFormat.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "OldPbr",
                Value = TextureFormat.Format_OldPbr,
                Hint = OldPbrFormat.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "LabPbr 1.1",
                Value = TextureFormat.Format_Lab11,
                Hint = LabPbr11Format.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "LabPbr 1.2",
                Value = TextureFormat.Format_Lab12,
                Hint = LabPbr12Format.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "LabPbr 1.3",
                Value = TextureFormat.Format_Lab13,
                Hint = LabPbr13Format.Description,
                GameEditions = new [] {GameEditions.Java},
            });
            Add(new TextureFormatValueItem {
                Text = "RTX",
                Value = TextureFormat.Format_Rtx,
                Hint = RtxFormat.Description,
                GameEditions = new [] {GameEditions.Bedrock},
            });
        }
    }

    public class TextureFormatValueItem
    {
        public string Text {get; set;}
        public string Value {get; set;}
        public string Hint {get; set;}
        public string[] GameEditions {get; set;}
    }
}
