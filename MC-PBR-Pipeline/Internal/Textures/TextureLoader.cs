﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal interface ITextureLoader
    {
        IAsyncEnumerable<TextureCollection> LoadAsync(string path, CancellationToken token = default);
    }

    internal class TextureLoader : ITextureLoader
    {
        private readonly ILogger logger;


        public TextureLoader(ILogger<TextureLoader> logger)
        {
            this.logger = logger;
        }

        public IAsyncEnumerable<TextureCollection> LoadAsync(string rootPath, CancellationToken token = default)
        {
            return LoadRecursiveAsync(rootPath, rootPath, token);
        }

        private async IAsyncEnumerable<TextureCollection> LoadRecursiveAsync(string rootPath, string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in Directory.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = Path.Combine(directory, "pbr.json");

                if (File.Exists(mapFile)) {
                    TextureCollection texture = null;

                    try {
                        texture = await LoadLocalTextureAsync(rootPath, mapFile, token);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                    continue;
                }

                await foreach (var texture in LoadRecursiveAsync(rootPath, directory, token))
                    yield return texture;

                var ignoreList = new List<string> {
                    Path.Combine(rootPath, "pack.json"),
                };

                foreach (var filename in Directory.EnumerateFiles(directory, "*.pbr")) {
                    var texture = await LoadGlobalTextureAsync(rootPath, filename, token);

                    ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_h.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_n.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_s.*"));

                    yield return texture;
                }

                foreach (var filename in Directory.EnumerateFiles(directory, "*")) {
                    if (ignoreList.Contains(filename, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var localFile = filename[rootPath.Length..].TrimStart('\\', '/');
                    logger.LogInformation($"Found other file '{localFile}'.");
                }
            }
        }

        public async Task<TextureCollection> LoadGlobalTextureAsync(string rootPath, string filename, CancellationToken token = default)
        {
            return new TextureCollection {
                Name = Path.GetFileNameWithoutExtension(filename),
                Path = Path.GetDirectoryName(filename)?[rootPath.Length..].TrimStart('\\', '/'),
                Map = await JsonFile.ReadAsync<TextureMap>(filename, token),
                UseGlobalMatching = true,
            };
        }

        public async Task<TextureCollection> LoadLocalTextureAsync(string rootPath, string filename, CancellationToken token = default)
        {
            var itemPath = Path.GetDirectoryName(filename);

            return new TextureCollection {
                Name = Path.GetFileName(itemPath),
                Path = Path.GetDirectoryName(itemPath)?[rootPath.Length..].TrimStart('\\', '/'),
                Map = await JsonFile.ReadAsync<TextureMap>(filename, token),
            };
        }
    }
}
