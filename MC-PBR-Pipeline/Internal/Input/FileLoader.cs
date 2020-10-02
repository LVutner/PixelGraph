﻿using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Input
{
    internal interface IFileLoader
    {
        IAsyncEnumerable<object> LoadAsync(CancellationToken token = default);
    }

    internal class FileLoader : IFileLoader
    {
        private readonly IInputReader reader;
        private readonly ILogger logger;


        public FileLoader(
            IServiceProvider provider,
            IInputReader reader)
        {
            this.reader = reader;

            logger = provider.GetRequiredService<ILogger<FileLoader>>();
        }

        public IAsyncEnumerable<object> LoadAsync(CancellationToken token = default)
        {
            return LoadRecursiveAsync(".", token);
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in reader.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = Path.Combine(directory, "pbr.json");

                if (reader.FileExists(mapFile)) {
                    TextureCollection texture = null;

                    try {
                        texture = await LoadLocalTextureAsync(mapFile, token);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                    continue;
                }

                await foreach (var texture in LoadRecursiveAsync(directory, token))
                    yield return texture;

                var ignoreList = new List<string>();

                foreach (var filename in reader.EnumerateFiles(directory, "*.pbr")) {
                    TextureCollection texture = null;

                    try {
                        texture = await LoadGlobalTextureAsync(filename, token);

                        ignoreList.AddRange(reader.EnumerateFiles(directory, $"{texture.Name}.*"));
                        ignoreList.AddRange(reader.EnumerateFiles(directory, $"{texture.Name}_h.*"));
                        ignoreList.AddRange(reader.EnumerateFiles(directory, $"{texture.Name}_n.*"));
                        ignoreList.AddRange(reader.EnumerateFiles(directory, $"{texture.Name}_s.*"));
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                }

                foreach (var filename in reader.EnumerateFiles(directory, "*")) {
                    if (ignoreList.Contains(filename, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var extension = Path.GetExtension(filename);
                    if (IgnoredExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase)) {
                        logger.LogDebug($"Ignoring file '{filename}'.");
                        continue;
                    }

                    yield return filename;
                }
            }
        }

        public async Task<TextureCollection> LoadGlobalTextureAsync(string localFile, CancellationToken token = default)
        {
            return new TextureCollection {
                Name = Path.GetFileNameWithoutExtension(localFile),
                Path = Path.GetDirectoryName(localFile),
                Map = await reader.ReadJsonAsync<TextureMap>(localFile, token),
                UseGlobalMatching = true,
            };
        }

        public async Task<TextureCollection> LoadLocalTextureAsync(string localFile, CancellationToken token = default)
        {
            var itemPath = Path.GetDirectoryName(localFile);

            return new TextureCollection {
                Name = Path.GetFileName(itemPath),
                Path = Path.GetDirectoryName(itemPath),
                Map = await reader.ReadJsonAsync<TextureMap>(localFile, token),
            };
        }

        private static readonly string[] IgnoredExtensions = {".zip", ".db", ".cmd", ".sh", ".xcf", ".psd"};
    }
}
