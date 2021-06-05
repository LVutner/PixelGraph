﻿using Microsoft.Extensions.Logging;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;

namespace PixelGraph.UI.Models
{
    internal class ImportPackModel : ModelBase
    {
        private ImportTreeNode _rootNode;
        private string _rootDirectory;
        private string _importSource;
        private volatile bool _isReady;
        private volatile bool _isActive;
        private volatile bool _showLog;

        public event EventHandler<LogEventArgs> LogEvent;

        public bool IsArchive {get; set;}
        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public string SourceFormat {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}

        public bool IsReady {
            get => _isReady;
            set {
                _isReady = value;
                OnPropertyChanged();
            }
        }

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();
            }
        }

        public string ImportSource {
            get => _importSource;
            set {
                _importSource = value;
                OnPropertyChanged();
            }
        }

        public ImportTreeNode RootNode {
            get => _rootNode;
            set {
                _rootNode = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLog {
            get => _showLog;
            set {
                _showLog = value;
                OnPropertyChanged();
            }
        }


        public ImportPackModel()
        {
            SourceFormat = null;
            CopyUntracked = true;
            AsGlobal = false;
        }

        public void AppendLog(LogLevel level, string text)
        {
            var e = new LogEventArgs(level, text);
            LogEvent?.Invoke(this, e);
        }
    }

    internal class ImportPackDesignVM : ImportPackModel
    {
        public ImportPackDesignVM()
        {
            ImportSource = "C:\\SomePath\\File.zip";
            IsArchive = true;

            AppendLog(LogLevel.Information, "Hello World!");

            ShowLog = true;
        }
    }
}
