﻿using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Preview.Sky;
using SharpDX.Direct3D11;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    public class CustomPbrMaterialCore : MaterialCore
    {
        private IEnvironmentCube _environmentCube;
        private TextureModel _albedoAlphaMap;
        private TextureModel _normalHeightMap;
        private TextureModel _roughF0OcclusionMap;
        private TextureModel _porositySssEmissive;
        private SamplerStateDescription _surfaceMapSampler;
        private SamplerStateDescription _cubeMapSampler;
        private bool _renderEnvironmentMap;
        private bool _renderShadowMap;

        public string MaterialPassName {get;}

        public IEnvironmentCube EnvironmentCube {
            get => _environmentCube;
            set => Set(ref _environmentCube, value);
        }

        public TextureModel AlbedoAlphaMap {
            get => _albedoAlphaMap;
            set => Set(ref _albedoAlphaMap, value);
        }

        public TextureModel NormalHeightMap {
            get => _normalHeightMap;
            set => Set(ref _normalHeightMap, value);
        }

        public TextureModel RoughF0OcclusionMap {
            get => _roughF0OcclusionMap;
            set => Set(ref _roughF0OcclusionMap, value);
        }

        public TextureModel PorositySssEmissiveMap {
            get => _porositySssEmissive;
            set => Set(ref _porositySssEmissive, value);
        }

        public bool RenderShadowMap {
            get => _renderShadowMap; 
            set => Set(ref _renderShadowMap, value);
        }

        public bool RenderEnvironmentMap {
            get => _renderEnvironmentMap;
            set => Set(ref _renderEnvironmentMap, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => _surfaceMapSampler; 
            set => Set(ref _surfaceMapSampler, value); 
        }

        public SamplerStateDescription CubeMapSampler {
            get => _cubeMapSampler; 
            set => Set(ref _cubeMapSampler, value); 
        }


        public CustomPbrMaterialCore(string materialPassName)
        {
            this.MaterialPassName = materialPassName ?? throw new ArgumentNullException(nameof(materialPassName));

            _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni4;
            _cubeMapSampler = DefaultSamplers.IBLSampler;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new CustomPbrMaterialVariable(manager, technique, this, MaterialPassName);
        }
    }
}
