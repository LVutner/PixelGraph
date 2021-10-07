﻿using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BarrelSide : BedrockBlockData
    {
        public const string BlockId = "barrel_side";
        public const string BlockName = "Barrel Side";


        public BarrelSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BarrelSide.BlockId,
                MinVersion = new GameVersion(1, 9),
            });
        }
    }
}
