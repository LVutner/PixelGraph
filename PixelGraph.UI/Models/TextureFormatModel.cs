﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models
{
    internal class TextureFormatModel : ModelBase
    {
        private readonly TextureChannelMapping _alpha;
        private readonly TextureChannelMapping _diffuseRed;
        private readonly TextureChannelMapping _diffuseGreen;
        private readonly TextureChannelMapping _diffuseBlue;
        private readonly TextureChannelMapping _albedoRed;
        private readonly TextureChannelMapping _albedoGreen;
        private readonly TextureChannelMapping _albedoBlue;
        private readonly TextureChannelMapping _height;
        private readonly TextureChannelMapping _occlusion;
        private readonly TextureChannelMapping _normalX;
        private readonly TextureChannelMapping _normalY;
        private readonly TextureChannelMapping _normalZ;
        private readonly TextureChannelMapping _specular;
        private readonly TextureChannelMapping _smooth;
        private readonly TextureChannelMapping _rough;
        private readonly TextureChannelMapping _metal;
        private readonly TextureChannelMapping _f0;
        private readonly TextureChannelMapping _porosity;
        private readonly TextureChannelMapping _sss;
        private readonly TextureChannelMapping _emissive;
        
        private ResourcePackEncoding _encoding;
        private ResourcePackEncoding _defaultEncoding;
        //public string _format, _sampler;
        private string _defaultSampler;
        private bool _enableSampler;
        
        public event EventHandler DataChanged;

        public TextureChannelMapping[] EncodingChannels {get;}

        //public string TextureFormat {
        //    get => _format;
        //    set {
        //        _format = value;
        //        OnPropertyChanged();

        //        InvalidateValues();
        //        OnDataChanged();
        //    }
        //}

        //public string TextureSampler {
        //    get => _sampler;
        //    set {
        //        _sampler = value;
        //        OnPropertyChanged();

        //        InvalidateValues();
        //    }
        //}

        public ResourcePackEncoding Encoding {
            get => _encoding;
            set {
                _encoding = value;
                OnPropertyChanged();

                InvalidateValues();
            }
        }

        public bool EnableSampler {
            get => _enableSampler;
            set {
                _enableSampler = value;
                OnPropertyChanged();
            }
        }

        public ResourcePackEncoding DefaultEncoding {
            get => _defaultEncoding;
            set {
                _defaultEncoding = value;
                OnPropertyChanged();

                InvalidateValues();
            }
        }

        public string DefaultSampler {
            get => _defaultSampler;
            set {
                _defaultSampler = value;
                OnPropertyChanged();

                InvalidateValues();
            }
        }

        //public string EncodingSampler => _loadedProfile?.Encoding?.Sampler;

        //public string EditEncodingSampler {
        //    get => _loadedProfile?.Encoding?.Sampler ?? Samplers.Nearest;
        //    set {
        //        if (_loadedProfile == null) return;
        //        _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
        //        _loadedProfile.Encoding.Sampler = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(EncodingSampler));

        //        UpdateDefaultValues();
        //        OnDataChanged();
        //    }
        //}


        public TextureFormatModel()
        {
            EncodingChannels = new []{
                _alpha = new TextureChannelMapping("Alpha"),

                _diffuseRed = new TextureChannelMapping("Diffuse Red"),
                _diffuseGreen = new TextureChannelMapping("Diffuse Green"),
                _diffuseBlue = new TextureChannelMapping("Diffuse Blue"),

                _albedoRed = new TextureChannelMapping("Albedo Red"),
                _albedoGreen = new TextureChannelMapping("Albedo Green"),
                _albedoBlue = new TextureChannelMapping("Albedo Blue"),

                _height = new TextureChannelMapping("Height"),
                _occlusion = new TextureChannelMapping("Occlusion"),

                _normalX = new TextureChannelMapping("Normal-X"),
                _normalY = new TextureChannelMapping("Normal-Y"),
                _normalZ = new TextureChannelMapping("Normal-Z"),

                _specular = new TextureChannelMapping("Specular"),

                _smooth = new TextureChannelMapping("Smooth"),
                _rough = new TextureChannelMapping("Rough"),

                _metal = new TextureChannelMapping("Metal"),
                _f0 = new TextureChannelMapping("F0"),

                _porosity = new TextureChannelMapping("Porosity"),

                _sss = new TextureChannelMapping("SubSurface Scattering"),

                _emissive = new TextureChannelMapping("Emissive"),
            };

            _alpha.DataChanged += OnChannelDataChanged;
            
            _diffuseRed.DataChanged += OnChannelDataChanged;
            _diffuseGreen.DataChanged += OnChannelDataChanged;
            _diffuseBlue.DataChanged += OnChannelDataChanged;

            _albedoRed.DataChanged += OnChannelDataChanged;
            _albedoGreen.DataChanged += OnChannelDataChanged;
            _albedoBlue.DataChanged += OnChannelDataChanged;

            _height.DataChanged += OnChannelDataChanged;
            _occlusion.DataChanged += OnChannelDataChanged;

            _normalX.DataChanged += OnChannelDataChanged;
            _normalY.DataChanged += OnChannelDataChanged;
            _normalZ.DataChanged += OnChannelDataChanged;

            _specular.DataChanged += OnChannelDataChanged;

            _smooth.DataChanged += OnChannelDataChanged;
            _rough.DataChanged += OnChannelDataChanged;

            _metal.DataChanged += OnChannelDataChanged;
            _f0.DataChanged += OnChannelDataChanged;

            _porosity.DataChanged += OnChannelDataChanged;

            _sss.DataChanged += OnChannelDataChanged;

            _emissive.DataChanged += OnChannelDataChanged;
        }

        private void InvalidateValues()
        {
            //OnPropertyChanged(nameof(IsBedrockProfile));
            //OnPropertyChanged(nameof(IsJavaProfile));
            //OnPropertyChanged(nameof(HasLoadedProfile));
            //OnPropertyChanged(nameof(PackName));
            //OnPropertyChanged(nameof(EditPackName));
            //OnPropertyChanged(nameof(GameEdition));
            //OnPropertyChanged(nameof(PackDescription));
            //OnPropertyChanged(nameof(PackTags));
            //OnPropertyChanged(nameof(PackFormat));
            //OnPropertyChanged(nameof(PackHeaderUuid));
            //OnPropertyChanged(nameof(PackModuleUuid));
            //OnPropertyChanged(nameof(TextureFormat));
            //OnPropertyChanged(nameof(ImageEncoding));
            //OnPropertyChanged(nameof(EditImageEncoding));
            //OnPropertyChanged(nameof(TextureSampler));
            //OnPropertyChanged(nameof(EditEncodingSampler));
            //OnPropertyChanged(nameof(TextureSize));
            //OnPropertyChanged(nameof(BlockTextureSize));
            //OnPropertyChanged(nameof(TextureScale));
            //OnPropertyChanged(nameof(OcclusionQuality));
            //OnPropertyChanged(nameof(OcclusionPower));
            //OnPropertyChanged(nameof(AutoGenerateNormal));
            //OnPropertyChanged(nameof(AutoGenerateOcclusion));

            UpdateChannels();
            UpdateDefaultValues();
        }

        private void UpdateChannels()
        {
            _alpha.SetChannel(Encoding.Alpha);

            _diffuseRed.SetChannel(Encoding.DiffuseRed);
            _diffuseGreen.SetChannel(Encoding.DiffuseGreen);
            _diffuseBlue.SetChannel(Encoding.DiffuseBlue);

            _albedoRed.SetChannel(Encoding.AlbedoRed);
            _albedoGreen.SetChannel(Encoding.AlbedoGreen);
            _albedoBlue.SetChannel(Encoding.AlbedoBlue);

            _height.SetChannel(Encoding.Height);
            _occlusion.SetChannel(Encoding.Occlusion);

            _normalX.SetChannel(Encoding.NormalX);
            _normalY.SetChannel(Encoding.NormalY);
            _normalZ.SetChannel(Encoding.NormalZ);

            _specular.SetChannel(Encoding.Specular);

            _smooth.SetChannel(Encoding.Smooth);
            _rough.SetChannel(Encoding.Rough);

            _metal.SetChannel(Encoding.Metal);
            _f0.SetChannel(Encoding.F0);

            _porosity.SetChannel(Encoding.Porosity);

            _sss.SetChannel(Encoding.SSS);

            _emissive.SetChannel(Encoding.Emissive);
        }

        private void UpdateDefaultValues()
        {
            //var _defaultEncoding = Common.TextureFormats.TextureFormat.GetFactory(TextureFormat)?.Create();
            //var _defaultSampler = TextureSampler ?? Samplers.Nearest;

            _alpha.ApplyDefaultValues(DefaultEncoding?.Alpha, DefaultSampler);

            _diffuseRed.ApplyDefaultValues(DefaultEncoding?.DiffuseRed, DefaultSampler);
            _diffuseGreen.ApplyDefaultValues(DefaultEncoding?.DiffuseGreen, DefaultSampler);
            _diffuseBlue.ApplyDefaultValues(DefaultEncoding?.DiffuseBlue, DefaultSampler);

            _albedoRed.ApplyDefaultValues(DefaultEncoding?.AlbedoRed, DefaultSampler);
            _albedoGreen.ApplyDefaultValues(DefaultEncoding?.AlbedoGreen, DefaultSampler);
            _albedoBlue.ApplyDefaultValues(DefaultEncoding?.AlbedoBlue, DefaultSampler);

            _height.ApplyDefaultValues(DefaultEncoding?.Height, DefaultSampler);
            _occlusion.ApplyDefaultValues(DefaultEncoding?.Occlusion, DefaultSampler);

            _normalX.ApplyDefaultValues(DefaultEncoding?.NormalX, DefaultSampler);
            _normalY.ApplyDefaultValues(DefaultEncoding?.NormalY, DefaultSampler);
            _normalZ.ApplyDefaultValues(DefaultEncoding?.NormalZ, DefaultSampler);

            _specular.ApplyDefaultValues(DefaultEncoding?.Specular, DefaultSampler);

            _smooth.ApplyDefaultValues(DefaultEncoding?.Smooth, DefaultSampler);
            _rough.ApplyDefaultValues(DefaultEncoding?.Rough, DefaultSampler);

            _metal.ApplyDefaultValues(DefaultEncoding?.Metal, DefaultSampler);
            _f0.ApplyDefaultValues(DefaultEncoding?.F0, DefaultSampler);

            _porosity.ApplyDefaultValues(DefaultEncoding?.Porosity, DefaultSampler);

            _sss.ApplyDefaultValues(DefaultEncoding?.SSS, DefaultSampler);

            _emissive.ApplyDefaultValues(DefaultEncoding?.Emissive, DefaultSampler);
        }

        private void OnChannelDataChanged(object sender, EventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class TextureFormatDesignModel : TextureFormatModel
    {
        public TextureFormatDesignModel()
        {
            //TextureFormat = Common.TextureFormats.TextureFormat.Format_Lab13;
            DefaultEncoding = TextureFormat.GetFactory(TextureFormat.Format_Lab13)?.Create();
            EnableSampler = true;
        }
    }
}
