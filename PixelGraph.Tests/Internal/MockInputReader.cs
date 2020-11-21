﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Tests.Internal
{
    internal class MockInputReader : BaseInputReader
    {
        public MockFileContent Content {get;}
        public string Root {get; set;} = ".";


        public MockInputReader(MockFileContent content, INamingStructure naming) : base(naming)
        {
            Content = content;
        }

        public override void SetRoot(string absolutePath)
        {
            Root = absolutePath;
        }

        public override string GetFullPath(string localPath)
        {
            return PathEx.Join(Root, localPath);
        }

        public override IEnumerable<string> EnumerateDirectories(string localPath, string pattern = default)
        {
            var fullPath = GetFullPath(localPath);

            foreach (var directory in Content.EnumerateDirectories(fullPath, pattern)) {
                var directoryName = Path.GetFileName(directory);
                yield return PathEx.Join(localPath, directoryName);
            }
        }

        public override IEnumerable<string> EnumerateFiles(string localPath, string pattern = default)
        {
            var fullPath = GetFullPath(localPath);

            foreach (var file in Content.EnumerateFiles(fullPath, pattern)) {
                var fileName = Path.GetFileName(file.Filename) ?? string.Empty;
                yield return PathEx.Join(localPath, fileName);
            }
        }

        public override bool FileExists(string localFile)
        {
            var fullFile = GetFullPath(localFile);
            return Content.FileExists(fullFile);
        }

        public override Stream Open(string localFile)
        {
            var fullFile = GetFullPath(localFile);
            return Content.OpenRead(fullFile);
        }

        public override DateTime? GetWriteTime(string localFile) => null;
    }
}
