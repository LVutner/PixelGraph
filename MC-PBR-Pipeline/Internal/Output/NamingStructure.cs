﻿using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;

namespace McPbrPipeline.Internal.Output
{
    internal static class NamingStructure
    {
        private static readonly Dictionary<string, Func<string, string>> localMap;
        private static readonly Dictionary<string, Func<string, string, string>> globalMap;


        static NamingStructure()
        {
            globalMap = new Dictionary<string, Func<string, string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = (name, ext) => $"{name}.{ext}",
                [TextureTags.Height] = (name, ext) => $"{name}_h.{ext}",
                [TextureTags.Normal] = (name, ext) => $"{name}_n.{ext}",
                [TextureTags.Specular] = (name, ext) => $"{name}_s.{ext}",
                [TextureTags.Emissive] = (name, ext) => $"{name}_e.{ext}",
                [TextureTags.Occlusion] = (name, ext) => $"{name}_ao.{ext}",
            };

            localMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = ext => $"albedo.{ext}",
                [TextureTags.Height] = ext => $"height.{ext}",
                [TextureTags.Normal] = ext => $"normal.{ext}",
                [TextureTags.Specular] = ext => $"specular.{ext}",
                [TextureTags.Emissive] = ext => $"emissive.{ext}",
                [TextureTags.Occlusion] = ext => $"occlusion.{ext}",
            };
        }

        public static string GetInputTextureName(string tag, string textureName, bool global)
        {
            return Get(tag, textureName, "*", global);
        }

        public static string GetOutputTextureName(string tag, string textureName, bool global)
        {
            return Get(tag, textureName, "png", global);
        }

        public static string GetInputMetaName(string tag, PbrProperties texture)
        {
            var path = GetPath(texture, texture.UseGlobalMatching);
            var file = Get(tag, texture.Name, "mcmeta", texture.UseGlobalMatching);
            return PathEx.Join(path, file);
        }

        public static string GetOutputMetaName(string tag, PbrProperties texture, bool global)
        {
            var path = GetPath(texture, global);
            var file = Get(tag, texture.Name, "png.mcmeta", global);
            return PathEx.Join(path, file);
        }

        public static string Get(string tag, string textureName, string extension, bool global)
        {
            return global
                ? globalMap[tag](textureName, extension)
                : localMap[tag](extension);
        }

        private static string GetPath(PbrProperties texture, bool global)
        {
            return global ? texture.Path : PathEx.Join(texture.Path, texture.Name);
        }
    }
}