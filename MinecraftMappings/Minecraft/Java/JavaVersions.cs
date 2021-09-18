﻿using System;
using System.Linq;

namespace MinecraftMappings.Minecraft.Java
{
    public static class JavaVersions
    {
        private static readonly Lazy<Version[]> allParsedLazy = new Lazy<Version[]>(() => All.Select(Version.Parse).ToArray());

        public static Version[] AllParsed => allParsedLazy.Value;

        public static Version Latest => allParsedLazy.Value.Max();


        public static string[] All = {
            "1.18",

            "1.17.1",
            "1.17",

            "1.16.5",
            "1.16.4",
            "1.16.3",
            "1.16.2",
            "1.16.1",
            "1.16",

            "1.15.2",
            "1.15.1",
            "1.15",

            "1.14.4",
            "1.14.3",
            "1.14.2",
            "1.14.1",
            "1.14",

            "1.13.2",
            "1.13.1",
            "1.13",

            "1.12.2",
            "1.12.1",
            "1.12",

            "1.11.2",
            "1.11.1",
            "1.11",

            "1.10.2",
            "1.10.1",
            "1.10",

            "1.9.4",
            "1.9.3",
            "1.9.2",
            "1.9.1",
            "1.9",

            "1.8.9",
            "1.8.8",
            "1.8.7",
            "1.8.6",
            "1.8.5",
            "1.8.4",
            "1.8.3",
            "1.8.2",
            "1.8.1",
            "1.8",

            "1.7.10",
            "1.7.9",
            "1.7.8",
            "1.7.7",
            "1.7.6",
            "1.7.5",
            "1.7.4",
            "1.7.2",

            "1.6.4",
            "1.6.2",
            "1.6.1",

            "1.5.2",
            "1.5.1",
            "1.5",

            "1.4.7",
            "1.4.6",
            "1.4.5",
            "1.4.4",
            "1.4.2",

            "1.3.2",
            "1.3.1",

            "1.2.5",
            "1.2.4",
            "1.2.3",
            "1.2.2",
            "1.2.1",

            "1.1",

            "1.0",
        };
    }
}
