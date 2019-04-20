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
using Hedra.Engine.Management;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering.Core
{    
    public static class ShaderManager
    {
        public const string ModelViewMatrixName = "_modelViewMatrix";
        public const string ModelViewProjectionName = "_modelViewProjectionMatrix";
        public const int LightDistance = 384;
        public const int MaxLights = 32;
        private static Vector3 _lightPosition;
        private static Vector3 _lightColor;
        private static float _clipPlaneY;
        private static readonly List<Shader> _shaders;
        private static readonly List<PointLight> PointLights;
        public static PointLight[] Lights => PointLights.ToArray();

        static ShaderManager()
        {
            _shaders = new List<Shader>();
            PointLights = new List<PointLight>(MaxLights);
            _lightPosition = new Vector3(0, 1000, 0);
        }

        public static Shader GetById(uint Id)
        {
            return _shaders.First(S => S.ShaderId == Id);
        }
        
        public static void ReloadShaders()
        {
#if DEBUG
            AssetManager.GrabShaders();
#endif
            AssetManager.ReloadShaderSources();
            var currentShaders = _shaders.ToArray();
            currentShaders.ToList().ForEach( S => S.Reload() );
        }
        
        public static void RegisterShader(Shader Entry)
        {
            _shaders.Add(Entry);
            Entry.LightCountLocation = Renderer.GetUniformLocation(Entry.ShaderId, "LightCount");
            Entry.LightColorLocation = Renderer.GetUniformLocation(Entry.ShaderId, "LightColor");
            Entry.LightPositionLocation = Renderer.GetUniformLocation(Entry.ShaderId, "LightPosition");
            Entry.PointLightsColorUniform = new int[MaxLights];
            Entry.PointLightsPositionUniform = new int[MaxLights];
            Entry.PointLightsRadiusUniform = new int[MaxLights];
            
            for(var i = 0; i < Entry.PointLightsColorUniform.Length; i++)
            {
                Entry.PointLightsColorUniform[i] = Renderer.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Color");
            }
            for(var i = 0; i < Entry.PointLightsPositionUniform.Length; i++)
            {
                Entry.PointLightsPositionUniform[i] = Renderer.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Position");
            }
            for(var i = 0; i < Entry.PointLightsRadiusUniform.Length; i++)
            {
                Entry.PointLightsRadiusUniform[i] = Renderer.GetUniformLocation(Entry.ShaderId, "Lights["+i+"].Radius");
            }
        }

        public static void UnregisterShader(Shader Entry)
        {
            _shaders.Remove(Entry);
        }
        
        private static void Do(Func<Shader, bool> Condition, Action<Shader> Action, bool InSameThread = false)
        {
            var previousProgram = Renderer.ShaderBound;
            Renderer.BindShader(previousProgram);
            for (var i = 0; i < _shaders.Count; i++)
            {
                int k = i;
                if (Condition(_shaders[k]))
                {
                    void Do()
                    {
                        _shaders[k].Bind();
                        Action(_shaders[k]);
                    }

                    if (InSameThread) Do();
                    else Executer.ExecuteOnMainThread(Do);
                }
            }

            void Cleanup()
            {
                Renderer.BindShader(previousProgram);
            }
            if (InSameThread) Cleanup();
            else Executer.ExecuteOnMainThread(Cleanup);
        }

        public static PointLight GetAvailableLight()
        {
            if (UsedLights == MaxLights) return null;
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
            if (!Light.Locked)
            {
                PointLights.Remove(Light);
                Do(S => S.LightCountLocation != -1, delegate(Shader Shader)
                {
                    for (var i = 0; i < PointLights.Count; i++)
                    {
                        Renderer.Uniform3(Shader.PointLightsColorUniform[i], PointLights[i].Color);
                        Renderer.Uniform3(Shader.PointLightsPositionUniform[i], PointLights[i].Position);
                        Renderer.Uniform1(Shader.PointLightsRadiusUniform[i], PointLights[i].Radius);
                        Renderer.Uniform1(Shader.LightCountLocation, PointLights.Count);
                    }
                });
            }
            else
            {
                var lightIndex = PointLights.IndexOf(Light);
                Do(S => S.PointLightsColorUniform[lightIndex] != -1, delegate(Shader Shader)
                {
                    Renderer.Uniform3(Shader.PointLightsColorUniform[lightIndex], Light.Color);
                    Renderer.Uniform3(Shader.PointLightsPositionUniform[lightIndex], Light.Position);
                    Renderer.Uniform1(Shader.PointLightsRadiusUniform[lightIndex], Light.Radius);
                    Renderer.Uniform1(Shader.LightCountLocation, PointLights.Count);
                });
            }
        }
        
        public static Vector3 LightColor
        {
            get => _lightColor;
            set
            {
                _lightColor = value;
                Do(S => S.LightColorLocation != -1, delegate (Shader Shader)
                {
                    Shader["LightColor"] = _lightColor;
                });
            }
        }

        public static void SetLightColorInTheSameThread(Vector3 Color)
        {
            _lightColor = Color;
            ShaderManager.Do(S => S.LightColorLocation != -1, delegate (Shader Shader)
            {
                Shader["LightColor"] = _lightColor;
            }, true);
        }

        public static Vector3 LightPosition
        {
            get => _lightPosition;
            set
            {
                if(_lightPosition == value) return;
                
                _lightPosition = value;
                ShaderManager.Do(S => S.LightPositionLocation != -1, delegate(Shader Shader)
                {
                    Shader["LightPosition"] = _lightPosition;
                });
            }
        }

        public static int UsedLights => PointLights.Count;
    }
}
