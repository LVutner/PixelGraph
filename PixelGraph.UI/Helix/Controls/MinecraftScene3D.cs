﻿using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.Minecraft;
using SharpDX;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls
{
    public class MinecraftScene3D : Element3D, IMinecraftScene
    {
        private MinecraftSceneNode MCSceneNode => SceneNode as MinecraftSceneNode;
        public bool IsRenderValid => MCSceneNode.IsRenderValid;

        public float TimeOfDay {
            get => (float)GetValue(TimeOfDayProperty);
            set => SetValue(TimeOfDayProperty, value);
        }

        public Vector3 SunDirection {
            get => (Vector3)GetValue(SunDirectionProperty);
            set => SetValue(SunDirectionProperty, value);
        }

        public float SunStrength {
            get => (float)GetValue(SunStrengthProperty);
            set => SetValue(SunStrengthProperty, value);
        }

        public float Wetness {
            get => (float)GetValue(WetnessProperty);
            set => SetValue(WetnessProperty, value);
        }

        public float ParallaxDepth {
            get => (float)GetValue(ParallaxDepthProperty);
            set => SetValue(ParallaxDepthProperty, value);
        }

        public int ParallaxSamplesMin {
            get => (int)GetValue(ParallaxSamplesMinProperty);
            set => SetValue(ParallaxSamplesMinProperty, value);
        }

        public int ParallaxSamplesMax {
            get => (int)GetValue(ParallaxSamplesMaxProperty);
            set => SetValue(ParallaxSamplesMaxProperty, value);
        }

        public bool EnableLinearSampling {
            get => (bool)GetValue(EnableLinearSamplingProperty);
            set => SetValue(EnableLinearSamplingProperty, value);
        }

        public bool EnableSlopeNormals {
            get => (bool)GetValue(EnableSlopeNormalsProperty);
            set => SetValue(EnableSlopeNormalsProperty, value);
        }

        public bool EnablePuddles {
            get => (bool)GetValue(EnablePuddlesProperty);
            set => SetValue(EnablePuddlesProperty, value);
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            MCSceneNode?.Apply(deviceContext);
        }

        public void ResetValidation()
        {
            MCSceneNode?.ResetValidation();
        }

        protected override SceneNode OnCreateSceneNode()
        {
            return new MinecraftSceneNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            base.AssignDefaultValuesToSceneNode(core);
            if (core is not MinecraftSceneNode n) return;

            n.SunDirection = SunDirection;
            n.SunStrength = SunStrength;
            n.TimeOfDay = TimeOfDay;
            n.Wetness = Wetness;
            n.ParallaxDepth = ParallaxDepth;
            n.ParallaxSamplesMin = ParallaxSamplesMin;
            n.ParallaxSamplesMax = ParallaxSamplesMax;
            n.EnableLinearSampling = EnableLinearSampling;
            n.EnableSlopeNormals = EnableSlopeNormals;
            n.EnablePuddles = EnablePuddles;
        }

        public static readonly DependencyProperty TimeOfDayProperty =
            DependencyProperty.Register(nameof(TimeOfDay), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0.25f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.TimeOfDay = (float) e.NewValue;
            }));

        public static readonly DependencyProperty SunDirectionProperty =
            DependencyProperty.Register(nameof(SunDirection), typeof(Vector3), typeof(MinecraftScene3D), new PropertyMetadata(new Vector3(0, -1, 0), (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftSceneNode sceneNode })
                    sceneNode.SunDirection = (Vector3)e.NewValue;
            }));

        public static readonly DependencyProperty SunStrengthProperty =
            DependencyProperty.Register(nameof(SunStrength), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(1f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.SunStrength = (float) e.NewValue;
            }));

        public static readonly DependencyProperty WetnessProperty =
            DependencyProperty.Register(nameof(Wetness), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.Wetness = (float)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxDepthProperty =
            DependencyProperty.Register(nameof(ParallaxDepth), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxDepth = (float)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxSamplesMinProperty =
            DependencyProperty.Register(nameof(ParallaxSamplesMin), typeof(int), typeof(MinecraftScene3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxSamplesMin = (int)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxSamplesMaxProperty =
            DependencyProperty.Register(nameof(ParallaxSamplesMax), typeof(int), typeof(MinecraftScene3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxSamplesMax = (int)e.NewValue;
            }));

        public static readonly DependencyProperty EnableLinearSamplingProperty =
            DependencyProperty.Register(nameof(EnableLinearSampling), typeof(bool), typeof(MinecraftScene3D), new PropertyMetadata(false, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.EnableLinearSampling = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty EnableSlopeNormalsProperty =
            DependencyProperty.Register(nameof(EnableSlopeNormals), typeof(bool), typeof(MinecraftScene3D), new PropertyMetadata(false, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.EnableSlopeNormals = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty EnablePuddlesProperty =
            DependencyProperty.Register(nameof(EnablePuddles), typeof(bool), typeof(MinecraftScene3D), new PropertyMetadata(false, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.EnablePuddles = (bool)e.NewValue;
            }));
    }
}
