﻿using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BarrelTop : JavaBlockData
    {
        public const string BlockId = "barrel_top";
        public const string BlockName = "Barrel Top";


        public BarrelTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BarrelTop.BlockId,
                MinVersion = "1.14",
            });
        }
    }
}
