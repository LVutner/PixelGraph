﻿using PixelGraph.UI.Internal;
using System;

#if !RELEASENORENDER
using PixelGraph.Common.Extensions;
using PixelGraph.Rendering;
using SharpDX;
#endif

namespace PixelGraph.UI.Models.Scene
{
    public class ScenePropertiesModel : ModelBase
    {
        public event EventHandler SunChanged;

        private bool _sunEnabled;
        private int _timeOfDay;
        private int _sunTilt;
        private int _sunAzimuth;


        public bool SunEnabled {
            get => _sunEnabled;
            set {
                _sunEnabled = value;
                OnPropertyChanged();
                OnSunChanged();
            }
        }

        public int TimeOfDay {
            get => _timeOfDay;
            set {
                _timeOfDay = value;
                OnPropertyChanged();
                OnSunChanged();
            }
        }

        public int SunTilt {
            get => _sunTilt;
            set {
                _sunTilt = value;
                OnPropertyChanged();
                OnSunChanged();
            }
        }

        public int SunAzimuth {
            get => _sunAzimuth;
            set {
                _sunAzimuth = value;
                OnPropertyChanged();
                OnSunChanged();
            }
        }


        public ScenePropertiesModel()
        {
            _sunEnabled = true;
            _timeOfDay = 6_000;
        }

#if !RELEASENORENDER

        public void GetSunAngle(out Vector3 sunAngle, out float strength)
        {
            const float sun_overlap = 0.0f;
            const float sun_power = 0.9f;

            var time = GetLinearTimeOfDay();
            MinecraftTime.GetSunAngle(_sunAzimuth, _sunTilt, time, out sunAngle);
            strength = MinecraftTime.GetSunStrength(time, sun_overlap, sun_power);
        }

        public float GetLinearTimeOfDay()
        {
            var t = _timeOfDay / 24_000f;
            MathEx.Wrap(ref t, 0f, 1f);
            return t;
        }

        public void SetTimeOfDay(float value)
        {
            TimeOfDay = (int)(value * 24_000f);
        }

#endif

        protected void OnSunChanged()
        {
            SunChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
