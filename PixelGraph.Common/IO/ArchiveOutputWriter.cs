﻿using Nito.AsyncEx;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    internal class ArchiveOutputWriter : IOutputWriter
    {
        private readonly AsyncLock writeLock;
        private Stream fileStream;
        private ZipArchive archive;

        //public bool AllowConcurrency => false;


        public ArchiveOutputWriter()
        {
            writeLock = new AsyncLock();
        }

        public void SetRoot(string absolutePath)
        {
            archive?.Dispose();
            fileStream?.Dispose();

            fileStream = File.Open(absolutePath, FileMode.Create, FileAccess.Write);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        }

        public void Prepare() {}

        public async Task OpenReadAsync(string localFilename, Func<Stream, Task> readFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await writeLock.LockAsync(token);
            var entry = archive.GetEntry(localFilename);
            if (entry == null) throw new FileNotFoundException("File not found!", localFilename);

            await using var stream = entry.Open();
            await readFunc(stream);
        }

        public async Task OpenWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await writeLock.LockAsync(token);
            var entry = archive.CreateEntry(localFilename);

            await using var stream = entry.Open();
            await writeFunc(stream);
        }

        public async Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> readWriteFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await writeLock.LockAsync(token);
            var entry = archive.GetEntry(localFilename)
                     ?? archive.CreateEntry(localFilename);

            await using var stream = entry.Open();
            await readWriteFunc(stream);
        }

        public bool FileExists(string localFile) => false;

        public DateTime? GetWriteTime(string localFile) => null;

        public void Delete(string localFile)
        {
            archive.GetEntry(localFile)?.Delete();
        }

        public void Clean() {}

        public void Dispose()
        {
            archive?.Dispose();
            fileStream?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            archive?.Dispose();

            if (fileStream != null)
                await fileStream.DisposeAsync();
        }
    }
}
