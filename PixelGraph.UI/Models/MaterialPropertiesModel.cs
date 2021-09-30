﻿using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;

namespace PixelGraph.UI.Models
{
    internal class MaterialPropertiesModel : ModelBase
    {
        private MaterialProperties _material;
        private string _selectedTag;
        private decimal? _iorToF0Value;
        private decimal? _iorDefaultValue;

        public event EventHandler DataChanged;

        public GeneralPropertyCollection GeneralProperties {get;}
        public OpacityPropertyCollection OpacityProperties {get;}
        public ColorPropertyCollection ColorProperties {get;}
        public ColorOtherPropertyCollection ColorOtherProperties {get;}
        public HeightPropertyCollection HeightProperties {get;}
        public HeightEdgeFadingPropertyCollection HeightEdgeProperties {get;}
        public NormalPropertyCollection NormalProperties {get;}
        public NormalFilterPropertyCollection NormalFilterProperties {get;}
        public NormalGeneratorPropertyCollection NormalGenerationProperties {get;}
        public OcclusionPropertyCollection OcclusionProperties {get;}
        public OcclusionGeneratorPropertyCollection OcclusionGenerationProperties {get;}
        public SpecularPropertyCollection SpecularProperties {get;}
        public SmoothPropertyCollection SmoothProperties {get;}
        public RoughPropertyCollection RoughProperties {get;}
        public MetalPropertyCollection MetalProperties {get;}
        public F0PropertyCollection F0Properties {get;}
        public PorosityPropertyCollection PorosityProperties {get;}
        public SssPropertyCollection SssProperties {get; set;}
        public EmissivePropertyCollection EmissiveProperties {get;}

        public bool HasMaterial => _material != null;
        public bool IsGeneralSelected => TextureTags.Is(_selectedTag, TextureTags.General);
        public bool IsOpacitySelected => TextureTags.Is(_selectedTag, TextureTags.Opacity);
        public bool IsColorSelected => TextureTags.Is(_selectedTag, TextureTags.Color);
        public bool IsHeightSelected => TextureTags.Is(_selectedTag, TextureTags.Height);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);
        public bool IsOcclusionSelected => TextureTags.Is(_selectedTag, TextureTags.Occlusion);
        public bool IsSpecularSelected => TextureTags.Is(_selectedTag, TextureTags.Specular);
        public bool IsSmoothSelected => TextureTags.Is(_selectedTag, TextureTags.Smooth);
        public bool IsRoughSelected => TextureTags.Is(_selectedTag, TextureTags.Rough);
        public bool IsMetalSelected => TextureTags.Is(_selectedTag, TextureTags.Metal);
        public bool IsF0Selected => TextureTags.Is(_selectedTag, TextureTags.F0);
        public bool IsPorositySelected => TextureTags.Is(_selectedTag, TextureTags.Porosity);
        public bool IsSssSelected => TextureTags.Is(_selectedTag, TextureTags.SubSurfaceScattering);
        public bool IsEmissiveSelected => TextureTags.Is(_selectedTag, TextureTags.Emissive);

        public decimal? IorActualValue => _iorToF0Value;

        public MaterialProperties Material {
            get => _material;
            set {
                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                GeneralProperties.SetData(value);
                OpacityProperties.SetData(value?.Opacity);
                ColorProperties.SetData(value?.Color);
                ColorOtherProperties.SetData(value?.Color);
                HeightProperties.SetData(value?.Height);
                HeightEdgeProperties.SetData(value?.Height);
                NormalProperties.SetData(value?.Normal);
                NormalFilterProperties.SetData(value?.Normal);
                NormalGenerationProperties.SetData(value?.Normal);
                OcclusionProperties.SetData(value?.Occlusion);
                OcclusionGenerationProperties.SetData(value?.Occlusion);
                SpecularProperties.SetData(value?.Specular);
                SmoothProperties.SetData(value?.Smooth);
                RoughProperties.SetData(value?.Rough);
                MetalProperties.SetData(value?.Metal);
                F0Properties.SetData(value?.F0);
                PorosityProperties.SetData(value?.Porosity);
                SssProperties.SetData(value?.SSS);
                EmissiveProperties.SetData(value?.Emissive);

                _iorToF0Value = null;
                _iorDefaultValue = CalculateIorFromF0();
                OnPropertyChanged(nameof(IorActualValue));
                OnPropertyChanged(nameof(IorEditValue));
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsGeneralSelected));
                OnPropertyChanged(nameof(IsOpacitySelected));
                OnPropertyChanged(nameof(IsColorSelected));
                OnPropertyChanged(nameof(IsHeightSelected));
                OnPropertyChanged(nameof(IsNormalSelected));
                OnPropertyChanged(nameof(IsOcclusionSelected));
                OnPropertyChanged(nameof(IsSpecularSelected));
                OnPropertyChanged(nameof(IsSmoothSelected));
                OnPropertyChanged(nameof(IsRoughSelected));
                OnPropertyChanged(nameof(IsMetalSelected));
                OnPropertyChanged(nameof(IsF0Selected));
                OnPropertyChanged(nameof(IsPorositySelected));
                OnPropertyChanged(nameof(IsSssSelected));
                OnPropertyChanged(nameof(IsEmissiveSelected));
            }
        }

        public decimal? IorEditValue {
            get => IorActualValue ?? _iorDefaultValue;
            set {
                _iorToF0Value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IorActualValue));
            }
        }


        public MaterialPropertiesModel()
        {
            GeneralProperties = new GeneralPropertyCollection();
            GeneralProperties.PropertyChanged += OnPropertyValueChanged;

            OpacityProperties = new OpacityPropertyCollection();
            OpacityProperties.PropertyChanged += OnPropertyValueChanged;

            ColorProperties = new ColorPropertyCollection();
            ColorProperties.PropertyChanged += OnPropertyValueChanged;

            ColorOtherProperties = new ColorOtherPropertyCollection();
            ColorOtherProperties.PropertyChanged += OnPropertyValueChanged;

            HeightProperties = new HeightPropertyCollection();
            HeightProperties.PropertyChanged += OnPropertyValueChanged;

            HeightEdgeProperties = new HeightEdgeFadingPropertyCollection();
            HeightEdgeProperties.PropertyChanged += OnPropertyValueChanged;

            NormalProperties = new NormalPropertyCollection();
            NormalProperties.PropertyChanged += OnPropertyValueChanged;

            NormalFilterProperties = new NormalFilterPropertyCollection();
            NormalFilterProperties.PropertyChanged += OnPropertyValueChanged;

            NormalGenerationProperties = new NormalGeneratorPropertyCollection();
            NormalGenerationProperties.PropertyChanged += OnPropertyValueChanged;

            OcclusionProperties = new OcclusionPropertyCollection();
            OcclusionProperties.PropertyChanged += OnPropertyValueChanged;

            OcclusionGenerationProperties = new OcclusionGeneratorPropertyCollection();
            OcclusionGenerationProperties.PropertyChanged += OnPropertyValueChanged;

            SpecularProperties = new SpecularPropertyCollection();
            SpecularProperties.PropertyChanged += OnPropertyValueChanged;

            SmoothProperties = new SmoothPropertyCollection();
            SmoothProperties.PropertyChanged += OnPropertyValueChanged;

            RoughProperties = new RoughPropertyCollection();
            RoughProperties.PropertyChanged += OnPropertyValueChanged;

            MetalProperties = new MetalPropertyCollection();
            MetalProperties.PropertyChanged += OnPropertyValueChanged;

            F0Properties = new F0PropertyCollection();
            F0Properties.PropertyChanged += OnF0PropertyValueChanged;

            PorosityProperties = new PorosityPropertyCollection();
            PorosityProperties.PropertyChanged += OnPropertyValueChanged;

            SssProperties = new SssPropertyCollection();
            SssProperties.PropertyChanged += OnPropertyValueChanged;

            EmissiveProperties = new EmissivePropertyCollection();
            EmissiveProperties.PropertyChanged += OnPropertyValueChanged;
        }

        public void ConvertIorToF0()
        {
            if (!_iorToF0Value.HasValue) return;

            var t = (_iorToF0Value - 1) / (_iorToF0Value + 1);
            var f0 = Math.Round((decimal)(t * t), 3);

            Material.F0.Value = f0;
            OnDataChanged();
            F0Properties.Invalidate();

            _iorDefaultValue = _iorToF0Value;
            _iorToF0Value = null;
            OnPropertyChanged(nameof(IorActualValue));
            OnPropertyChanged(nameof(IorEditValue));
        }

        private decimal? CalculateIorFromF0()
        {
            if (!(Material?.F0?.Value.HasValue ?? false)) return null;

            var f0 = (double)Material.F0.Value.Value;
            if (f0 < 0) return null;

            var d = -(f0 + 1 + 2 * Math.Sqrt(f0)) / (f0 - 1);
            return Math.Round((decimal) d, 3);
        }

        private void OnF0PropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(MaterialF0Properties.Value)) ?? false) {
                _iorToF0Value = null;
                _iorDefaultValue = CalculateIorFromF0();
                OnPropertyChanged(nameof(IorActualValue));
                OnPropertyChanged(nameof(IorEditValue));
            }

            OnDataChanged();
        }

        private void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class GeneralPropertyCollection : PropertyCollectionBase<MaterialProperties>
    {
        public GeneralPropertyCollection()
        {
            var options = new SelectPropertyRowOptions(new AllTextureFormatValues(), "Text", "Value");

            AddSelect("Input Format", nameof(MaterialProperties.InputFormat), options, MaterialProperties.DefaultInputFormat);
            AddBool<bool?>("Publish Texture", nameof(MaterialProperties.Publish), MaterialProperties.DefaultPublish);
            AddBool<bool?>("Publish Item Texture", nameof(MaterialProperties.PublishItem), MaterialProperties.DefaultPublishItem);
            AddBool<bool?>("Wrap Horizontally", nameof(MaterialProperties.WrapX), MaterialProperties.DefaultWrap);
            AddBool<bool?>("Wrap Vertically", nameof(MaterialProperties.WrapY), MaterialProperties.DefaultWrap);
        }
    }

    public class OpacityPropertyCollection : PropertyCollectionBase<MaterialOpacityProperties>
    {
        public OpacityPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialOpacityProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialOpacityProperties.Value), 255m);
            AddText<decimal?>("Scale", nameof(MaterialOpacityProperties.Scale), 1m);
        }
    }

    public class ColorPropertyCollection : PropertyCollectionBase<MaterialColorProperties>
    {
        public ColorPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialColorProperties.Texture));
            AddText<decimal?>("Red Value", nameof(MaterialColorProperties.ValueRed), 0);
            AddText<decimal?>("Green Value", nameof(MaterialColorProperties.ValueGreen), 0);
            AddText<decimal?>("Blue Value", nameof(MaterialColorProperties.ValueBlue), 0);
            AddText<decimal?>("Red Scale", nameof(MaterialColorProperties.ScaleRed), 1.0m);
            AddText<decimal?>("Green Scale", nameof(MaterialColorProperties.ScaleGreen), 1.0m);
            AddText<decimal?>("Blue Scale", nameof(MaterialColorProperties.ScaleBlue), 1.0m);
        }
    }

    public class ColorOtherPropertyCollection : PropertyCollectionBase<MaterialColorProperties>
    {
        public ColorOtherPropertyCollection()
        {
            AddBool<bool?>("Bake Occlusion", nameof(MaterialColorProperties.BakeOcclusion), MaterialColorProperties.DefaultBakeOcclusion);
            AddText<string>("Preview Tint", nameof(MaterialColorProperties.PreviewTint));
        }
    }

    public class HeightPropertyCollection : PropertyCollectionBase<MaterialHeightProperties>
    {
        public HeightPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialHeightProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialHeightProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialHeightProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialHeightProperties.Scale), 1m);
            AddBool<bool?>("Auto-Level", nameof(MaterialHeightProperties.AutoLevel));
        }
    }

    public class HeightEdgeFadingPropertyCollection : PropertyCollectionBase<MaterialHeightProperties>
    {
        public HeightEdgeFadingPropertyCollection()
        {
            AddText<decimal?>("Horizontal", nameof(MaterialHeightProperties.EdgeFadeX));
            AddText<decimal?>("Vertical", nameof(MaterialHeightProperties.EdgeFadeY));
            AddText<decimal?>("Strength", nameof(MaterialHeightProperties.EdgeFadeStrength), MaterialHeightProperties.DefaultEdgeFadeStrength);
        }
    }

    public class NormalPropertyCollection : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialNormalProperties.Texture));
        }
    }

    public class NormalFilterPropertyCollection : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalFilterPropertyCollection()
        {
            AddText<decimal?>("Noise Angle", nameof(MaterialNormalProperties.Noise));
            AddText<decimal?>("Curve Angle X", nameof(MaterialNormalProperties.CurveX));
            AddText<decimal?>("Curve Angle Y", nameof(MaterialNormalProperties.CurveY));
            AddText<decimal?>("Radius Size X", nameof(MaterialNormalProperties.RadiusX));
            AddText<decimal?>("Radius Size Y", nameof(MaterialNormalProperties.RadiusY));
        }
    }

    public class NormalGeneratorPropertyCollection : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalGeneratorPropertyCollection()
        {
            var methodOptions = new SelectPropertyRowOptions(new NormalMethodValues(), "Text", "Value");

            AddSelect("Method", nameof(MaterialNormalProperties.Method), methodOptions, "sobel3");
            AddText<decimal?>("Strength", nameof(MaterialNormalProperties.Strength), MaterialNormalProperties.DefaultStrength);
        }
    }

    public class OcclusionPropertyCollection : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialOcclusionProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialOcclusionProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialOcclusionProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialOcclusionProperties.Scale), 1m);
        }
    }

    public class OcclusionGeneratorPropertyCollection : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionGeneratorPropertyCollection()
        {
            AddText<decimal?>("Step Length", nameof(MaterialOcclusionProperties.StepDistance), MaterialOcclusionProperties.DefaultStepDistance);
            AddText<decimal?>("Z Bias", nameof(MaterialOcclusionProperties.ZBias), MaterialOcclusionProperties.DefaultZBias);
            AddText<decimal?>("Z Scale", nameof(MaterialOcclusionProperties.ZScale), MaterialOcclusionProperties.DefaultZScale);
        }
    }

    public class SpecularPropertyCollection : PropertyCollectionBase<MaterialSpecularProperties>
    {
        public SpecularPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSpecularProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSpecularProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSpecularProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSpecularProperties.Scale), 1m);
        }
    }

    public class SmoothPropertyCollection : PropertyCollectionBase<MaterialSmoothProperties>
    {
        public SmoothPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSmoothProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSmoothProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSmoothProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSmoothProperties.Scale), 1m);
        }
    }

    public class RoughPropertyCollection : PropertyCollectionBase<MaterialRoughProperties>
    {
        public RoughPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialRoughProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialRoughProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialRoughProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialRoughProperties.Scale), 1m);
        }
    }

    public class MetalPropertyCollection : PropertyCollectionBase<MaterialMetalProperties>
    {
        public MetalPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialMetalProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialMetalProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialMetalProperties.Scale), 1m);
        }
    }

    public class F0PropertyCollection : PropertyCollectionBase<MaterialF0Properties>
    {
        public F0PropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialF0Properties.Texture));
            AddText<decimal?>("Value", nameof(MaterialF0Properties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialF0Properties.Scale), 1m);
        }
    }

    public class PorosityPropertyCollection : PropertyCollectionBase<MaterialPorosityProperties>
    {
        public PorosityPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialPorosityProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialPorosityProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialPorosityProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialPorosityProperties.Scale), 1m);
        }
    }

    public class SssPropertyCollection : PropertyCollectionBase<MaterialSssProperties>
    {
        public SssPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSssProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSssProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSssProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSssProperties.Scale), 1m);
        }
    }

    public class EmissivePropertyCollection : PropertyCollectionBase<MaterialEmissiveProperties>
    {
        public EmissivePropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialEmissiveProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialEmissiveProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialEmissiveProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialEmissiveProperties.Scale), 1m);
        }
    }
}
