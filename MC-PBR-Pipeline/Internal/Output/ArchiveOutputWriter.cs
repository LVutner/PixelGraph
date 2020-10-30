﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Output
{
    internal class ArchiveOutputWriter : IOutputWriter
    {
        private Stream fileStream;
        private ZipArchive archive;


        public void SetRoot(string absolutePath)
        {
            fileStream = File.Open(absolutePath, FileMode.Create, FileAccess.Write);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        }

        public void Prepare() {}

        public Stream WriteFile(string localFilename)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            var entry = archive.CreateEntry(localFilename);
            return entry.Open();
        }

        public bool FileExists(string localFile) => false;
        public DateTime? GetWriteTime(string localFile) => null;

        public void Clean() {}

        public async ValueTask DisposeAsync()
        {
            archive?.Dispose();

            if (fileStream != null)
                await fileStream.DisposeAsync();
        }
    }
}
