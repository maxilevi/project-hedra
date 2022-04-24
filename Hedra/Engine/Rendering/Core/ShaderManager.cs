/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/05/2016
 * Time: 12:28 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Core
{
    public static class ShaderManager
    {
        private const string FogUniform = "FogSettings";
        private const string LightsUniform = "LightSettings";
        private const string MatricesUniform = "Matrices";
        private const string HighlightUniform = "HighlightSettings";
        public const string ModelViewMatrixName = "_modelViewMatrix";
        public const string ModelViewProjectionName = "_modelViewProjectionMatrix";
        public const int LightDistance = 384;
        public const int MaxLights = 32;
        public const int ReservedLights = 1;
        private static readonly Vector3 _lightPosition;
        private static Vector3 _lightColor;
        private static float _clipPlaneY;
        private static readonly List<Shader> _shaders;
        private static readonly List<PointLight> PointLights;

        static ShaderManager()
        {
            _shaders = new List<Shader>();
            PointLights = new List<PointLight>(MaxLights);
            _lightPosition = new Vector3(-500f, 800f, 0f);
            FogUBO = new UBO<FogData>(FogUniform);
            LightsUBO = new UBO<LightSettings>(LightsUniform);
        }

        public static UBO<FogData> FogUBO { get; }
        public static UBO<LightSettings> LightsUBO { get; }
        public static PointLight[] Lights => PointLights.ToArray();

        public static Vector3 LightColor
        {
            get => _lightColor;
            set
            {
                if (_lightColor == value) return;
                _lightColor = value;
                UpdateLights();
            }
        }

        public static int UsedLights => PointLights.Count;

        public static Shader GetById(uint Id)
        {
            for (var i = 0; i < _shaders.Count; ++i)
                if (_shaders[i].ShaderId == Id)
                    return _shaders[i];
            throw new ArgumentOutOfRangeException();
        }

        public static void ReloadShaders()
        {
#if DEBUG
            AssetManager.GrabShaders();
#endif
            AssetManager.ReloadShaderSources();
            var currentShaders = _shaders.ToArray();
            currentShaders.ToList().ForEach(S => S.Reload());
        }

        public static void RegisterShader(Shader Entry)
        {
            _shaders.Add(Entry);
            FogUBO.RegisterShader(Entry);
            LightsUBO.RegisterShader(Entry);
        }

        public static void UnregisterShader(Shader Entry)
        {
            _shaders.Remove(Entry);
        }

        private static void UpdateLights()
        {
            void DoUpdate()
            {
                var lights = new AlignedPointLight[MaxLights];
                var actualLights = Lights;
                for (var i = 0; i < actualLights.Length; ++i) lights[i] = new AlignedPointLight(actualLights[i]);
                LightsUBO.Update(new LightSettings(lights, _lightColor, _lightPosition, UsedLights));
            }

            if (Thread.CurrentThread.ManagedThreadId == Loader.Hedra.RenderingThreadId)
                DoUpdate();
            else
                Executer.ExecuteOnMainThread(DoUpdate);
        }

        public static PointLight GetAvailableLight(bool UseReserved = false)
        {
            if (UsedLights >= MaxLights - (UseReserved ? 0 : ReservedLights)) return null;
            var light = new PointLight
            {
                Radius = PointLight.DefaultRadius,
                Color = Vector3.Zero,
                Position = Vector3.Zero,
                Locked = true
            };
            PointLights.Add(light);
            return light;
        }

        public static void UpdateLight(PointLight Light)
        {
            if (!Light.Locked) PointLights.Remove(Light);
            UpdateLights();
        }

        public static void SetLightColorInTheSameThread(Vector3 Color)
        {
            if (_lightColor == Color) return;
            _lightColor = Color;
            UpdateLights();
        }
    }
}