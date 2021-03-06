using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Internal.Textures.Entity;
using MinecraftMappings.Minecraft;
using PixelGraph.Common.Material;
using PixelGraph.Rendering;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.LUTs;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Models;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace PixelGraph.UI.Models.Scene
{
    public class RenderPropertiesModel : ModelBase
    {
        private IEffectsManager _effectsManager;
        private ILutMapSource _dielectricBrdfLutMap;
        //private ICubeMapSource _environmentCube;
        private ICubeMapSource _irradianceCube;
        private ObservableElement3DCollection _meshParts;
        private PerspectiveCamera _camera;
        //private Stream _brdfLutMap;
        private MaterialProperties _missingMaterial;
        private bool _enableTiling;
        private RenderPreviewModes _renderMode;
        private float _parallaxDepth;
        private int _parallaxSamplesMin;
        private int _parallaxSamplesMax;
        private bool _enableLinearSampling;
        private bool _enableSlopeNormals;
        private bool _enableBloom;
        private int _waterMode;
        private OrthographicCamera _sunCamera;
        private PerspectiveCamera _lightCamera;
        //private Transform3D _meshTransform;
        private string _meshBlendMode;
        private string _meshTintColor;
        private bool _showIrradiance;

        public event EventHandler RenderModeChanged;
        public event EventHandler RenderModelChanged;

        internal ICubeMapSource DynamicSkyCubeSource {get; set;}
        internal ICubeMapSource ErpCubeSource {get; set;}

        public float ParallaxDepth {
            get => _parallaxDepth;
            set {
                _parallaxDepth = value;
                OnPropertyChanged();
            }
        }

        public int ParallaxSamplesMin {
            get => _parallaxSamplesMin;
            set {
                _parallaxSamplesMin = value;
                OnPropertyChanged();
            }
        }

        public int ParallaxSamplesMax {
            get => _parallaxSamplesMax;
            set {
                _parallaxSamplesMax = value;
                OnPropertyChanged();
            }
        }

        public bool EnableLinearSampling {
            get => _enableLinearSampling;
            set {
                _enableLinearSampling = value;
                OnPropertyChanged();
            }
        }

        public bool EnableSlopeNormals {
            get => _enableSlopeNormals;
            set {
                _enableSlopeNormals = value;
                OnPropertyChanged();
            }
        }

        public bool EnableBloom {
            get => _enableBloom;
            set {
                _enableBloom = value;
                OnPropertyChanged();
            }
        }

        public int WaterMode {
            get => _waterMode;
            set {
                _waterMode = value;
                OnPropertyChanged();
            }
        }

        public OrthographicCamera SunCamera {
            get => _sunCamera;
            set {
                _sunCamera = value;
                OnPropertyChanged();
            }
        }

        public PerspectiveCamera LightCamera {
            get => _lightCamera;
            set {
                _lightCamera = value;
                OnPropertyChanged();
            }
        }

        public RenderPreviewModes RenderMode {
            get => _renderMode;
            set {
                _renderMode = value;
                OnPropertyChanged();
                OnRenderModeChanged();
            }
        }

        public IEffectsManager EffectsManager {
            get => _effectsManager;
            set {
                _effectsManager = value;
                OnPropertyChanged();
            }
        }

        public bool EnableTiling {
            get => _enableTiling;
            set {
                _enableTiling = value;
                OnPropertyChanged();
                OnRenderModelChanged();
            }
        }

        public ILutMapSource DielectricBrdfLutMap {
            get => _dielectricBrdfLutMap;
            set {
                _dielectricBrdfLutMap = value;
                OnPropertyChanged();
            }
        }

        //public ICubeMapSource EnvironmentCube {
        //    get => _environmentCube;
        //    set {
        //        _environmentCube = value;
        //        OnPropertyChanged();
        //    }
        //}

        public ICubeMapSource IrradianceCube {
            get => _irradianceCube;
            set {
                _irradianceCube = value;
                OnPropertyChanged();
            }
        }

        //public Stream BrdfLutMap {
        //    get => _brdfLutMap;
        //    set {
        //        _brdfLutMap = value;
        //        OnPropertyChanged();
        //    }
        //}

        public MaterialProperties MissingMaterial {
            get => _missingMaterial;
            set {
                _missingMaterial = value;
                OnPropertyChanged();
            }
        }

        public PerspectiveCamera Camera {
            get => _camera;
            set {
                _camera = value;
                OnPropertyChanged();
            }
        }

        public ObservableElement3DCollection MeshParts {
            get => _meshParts;
            set {
                _meshParts = value;
                OnPropertyChanged();
            }
        }

        //public Transform3D MeshTransform {
        //    get => _meshTransform;
        //    set {
        //        _meshTransform = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string MeshBlendMode {
            get => _meshBlendMode;
            set {
                _meshBlendMode = value;
                OnPropertyChanged();
            }
        }

        public string MeshTintColor {
            get => _meshTintColor;
            set {
                _meshTintColor = value;
                OnPropertyChanged();
            }
        }

        public bool ShowIrradiance {
            get => _showIrradiance;
            set {
                if (_showIrradiance == value) return;
                _showIrradiance = value;
                OnPropertyChanged();
            }
        }


        public RenderPropertiesModel()
        {
            _meshParts = new ObservableElement3DCollection();

            Camera = new PerspectiveCamera {
                UpDirection = Vector3.UnitY.ToVector3D(),
            };

            _sunCamera = new OrthographicCamera {
                UpDirection = Vector3.UnitY.ToVector3D(),
                NearPlaneDistance = 1f,
                FarPlaneDistance = 48f,
                Width = 24,
            };

            _lightCamera = new PerspectiveCamera { 
                Position = new Point3D(8f, 8f, 8f),
                LookDirection = new Vector3D(-1f, -1f, -1f),
                UpDirection = new Vector3D(0f, 1f, 0f),
                FarPlaneDistance = 32f,
                NearPlaneDistance = 1f,
                FieldOfView = 45f,
            };
        }

        public void ApplyMaterial(MaterialProperties material)
        {
            var blend = material?.BlendMode;
            if (material != null && blend == null) {
                if (MCPath.IsEntityPath(material.LocalPath)) {
                    var textureData = Minecraft.Java.FindEntityTexturesById<JavaEntityTexture, JavaEntityTextureVersion>(material.Name).FirstOrDefault();
                    blend = textureData != null
                        ? BlendModes.ToString(textureData.BlendMode)
                        : BlendModes.CutoutText;
                }
                else if (MCPath.IsItemPath(material.LocalPath)) {
                    blend = BlendModes.CutoutText;
                }
                else {
                    var textureData = Minecraft.Java.FindBlockTexturesById<JavaBlockTexture, JavaBlockTextureVersion>(material.Name).FirstOrDefault();
                    blend = textureData != null
                        ? BlendModes.ToString(textureData.BlendMode)
                        : BlendModes.OpaqueText;
                }
            }

            MeshBlendMode = blend ?? BlendModes.OpaqueText;
            MeshTintColor = material?.TintColor;
        }

        protected virtual void OnRenderModeChanged()
        {
            RenderModeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRenderModelChanged()
        {
            RenderModelChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
