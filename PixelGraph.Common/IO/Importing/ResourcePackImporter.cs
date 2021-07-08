﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Importing
{
    public interface IResourcePackImporter
    {
        bool AsGlobal {get; set;}
        bool CopyUntracked {get; set;}
        ResourcePackInputProperties PackInput {get; set;}
        ResourcePackProfileProperties PackProfile {get; set;}

        Task ImportAsync(CancellationToken token = default);
    }

    internal class ResourcePackImporter : IResourcePackImporter
    {
        private static readonly Regex ctmExp = new("^assets/minecraft/optifine/ctm/?$", RegexOptions.IgnoreCase);
        private static readonly PropertyFileSerializer<CtmProperties> ctmPropertySerializer;

        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly IMaterialImporter importer;

        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}
        public ResourcePackProfileProperties PackProfile {get; set;}


        static ResourcePackImporter()
        {
            ctmPropertySerializer = new PropertyFileSerializer<CtmProperties>();
        }

        public ResourcePackImporter(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            writer = provider.GetRequiredService<IOutputWriter>();
            importer = provider.GetRequiredService<IMaterialImporter>();
        }

        public async Task ImportAsync(CancellationToken token = default)
        {
            await ImportPathAsync(".", token);
        }

        private async Task ImportPathAsync(string localPath, CancellationToken token)
        {
            foreach (var childPath in reader.EnumerateDirectories(localPath, "*")) {
                var name = Path.GetFileName(childPath);
                if (IgnoredFilesPaths.Contains(name)) continue;

                await ImportPathAsync(childPath, token);
            }

            var files = reader.EnumerateFiles(localPath, "*.*")
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            // TODO: detect and remove CTM files first
            if (ctmExp.IsMatch(localPath)) {
                foreach (var file in files) {
                    var ext = Path.GetExtension(file);
                    if (!ext.Equals(".properties", StringComparison.InvariantCultureIgnoreCase)) continue;
                    var name = Path.GetFileNameWithoutExtension(file);

                    // parse ctm properties file
                    await using var stream = reader.Open(file);
                    using var streamReader = new StreamReader(stream);
                    var ctmProperties = await ctmPropertySerializer.ReadAsync(streamReader, token);

                    // only supporting repeat-ctm for now
                    if (!CtmTypes.Is(ctmProperties.Method, CtmTypes.Repeat)) continue;

                    // Build 1D tile array
                    var ctmWidth = ctmProperties.Width ?? 1;
                    var ctmHeight = ctmProperties.Height ?? 1;
                    var expectedLength = ctmWidth * ctmHeight;

                    var tileFiles = ParseCtmTiles(ctmProperties, localPath).ToArray();
                    if (expectedLength < 1) throw new ApplicationException($"Invalid ctm dimensions! expected count={expectedLength}");
                    if (tileFiles.Length != expectedLength) throw new ApplicationException($"Expected {expectedLength:N0} ctm tiles but found {tileFiles.Length:N0}!");

                    // Build 2D tile array
                    var tileMap = new string[ctmWidth, ctmHeight];
                    for (var y = 0; y < ctmHeight; y++) {
                        for (var x = 0; x < ctmWidth; x++) {
                            var f = tileFiles[y * ctmWidth + x];
                            tileMap[x, y] = f;

                            // TODO: remove f from files (if same folder)
                        }
                    }

                    importer.AsGlobal = AsGlobal;
                    importer.PackInput = PackInput;
                    importer.PackProfile = PackProfile;

                    // TODO: BuildMaterial() with ctm tileMap
                    //...

                    // TODO: create single material file
                    var material = await importer.CreateMaterialAsync(localPath, name);
                }
            }

            var names = GetMaterialNames(files).Distinct().ToArray();

            foreach (var name in names) {
                token.ThrowIfCancellationRequested();

                await ImportMaterialAsync(localPath, name, token);

                // Remove from untracked files
                _ = ExtractTextureFile(files, name);
                _ = ExtractTextureFile(files, $"{name}_n");
                _ = ExtractTextureFile(files, $"{name}_s");
                _ = ExtractTextureFile(files, $"{name}_e");
            }

            if (!CopyUntracked) return;

            foreach (var file in files) {
                token.ThrowIfCancellationRequested();

                await CopyFileAsync(file, token);
            }
        }

        private IEnumerable<string> ParseCtmTiles(CtmProperties properties, string localPath)
        {
            var parts = properties.Tiles?.Trim().Split(' ', '\t');
            if (parts == null || parts.Length == 0) yield break;

            foreach (var part in parts) {
                if (TryParseRange(part, out var rangeMin, out var rangeMax)) {
                    foreach (var i in Enumerable.Range(rangeMin, rangeMax - rangeMin + 1))
                        yield return GetTileFilename(localPath, i.ToString());
                }
                else {
                    yield return GetTileFilename(localPath, part);
                }
            }
        }

        private string GetTileFilename(string localPath, string part)
        {
            var partPath = Path.GetDirectoryName(part);

            if (partPath == null) {
                // TODO: scan local folder

            }
            else {
                // TODO: scan relative folder
            }

            throw new ApplicationException("Unable to locate tile");
        }

        private async Task ImportMaterialAsync(string localPath, string name, CancellationToken token)
        {
            importer.AsGlobal = AsGlobal;
            importer.PackInput = PackInput;
            importer.PackProfile = PackProfile;

            var material = await importer.CreateMaterialAsync(localPath, name);
            await importer.ImportAsync(material, token);
        }

        private async Task CopyFileAsync(string file, CancellationToken token)
        {
            await using var sourceStream = reader.Open(file);
            await using var destStream = writer.Open(file);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private static string ExtractTextureFile(ICollection<string> files, string name)
        {
            var file = files.FirstOrDefault(f => {
                var fName = Path.GetFileNameWithoutExtension(f);
                return string.Equals(fName, name, StringComparison.InvariantCultureIgnoreCase);
            });

            if (file != null)
                files.Remove(file);

            return file;
        }

        private static IEnumerable<string> GetMaterialNames(IEnumerable<string> files)
        {
            foreach (var file in files) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var name = Path.GetFileNameWithoutExtension(file);

                var isNormal = name.EndsWith("_n", StringComparison.InvariantCultureIgnoreCase);
                var isSpecular = name.EndsWith("_s", StringComparison.InvariantCultureIgnoreCase);
                var isEmissive = name.EndsWith("_e", StringComparison.InvariantCultureIgnoreCase);

                if (isNormal || isSpecular || isEmissive)
                    yield return name[..^2];
            }
        }

        private static readonly string[] IgnoredFilesPaths = {
            ".git",
            ".ignore",
        };



        private static bool TryParseRange(string part, out int min, out int max)
        {
            var separator = part.IndexOf('-');

            if (separator < 0) {
                min = max = 0;
                return false;
            }

            try {
                min = int.Parse(part[..separator]);
                max = int.Parse(part[(separator + 1)..]);
                return true;
            }
            catch {
                min = max = 0;
                return false;
            }
        }
    }
}
