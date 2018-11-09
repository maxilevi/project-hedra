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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{    
    public static class ShaderManager
    {
        public const string ModelViewMatrixName = "_modelViewMatrix";
        public const string ModelViewProjectionName = "_modelViewProjectionMatrix";
        public const int LightDistance = 256;
        public const int MaxLights = 12;
        private static readonly List<Shader> Shaders;
        private static readonly PointLight[] PointLights;
        private static Vector3 _lightPosition;
        private static Vector3 _lightColor;
        private static float _clipPlaneY;

        static ShaderManager()
        {
            Shaders = new List<Shader>();
            PointLights = new PointLight[MaxLights];
            _lightPosition = new Vector3(0, 1000, 0);
            for (var i = 0; i < PointLights.Length; i++)
            {
                PointLights[i] = new PointLight();
            }
        }

        public static void ReloadShaders()
        {
#if DEBUG
            AssetManager.GrabShaders();
#endif
            AssetManager.ReloadShaderSources();
            var currentShaders = Shaders.ToArray();
            currentShaders.ToList().ForEach( S => S.Reload() );
        }
        
        public static void RegisterShader(Shader Entry)
        {
            Shaders.Add(Entry);
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

        private static void Do(Func<Shader, bool> Condition, Action<Shader> Action, bool InSameThread = false)
        {
            var previousProgram = Renderer.ShaderBound;
            Renderer.BindShader(previousProgram);
            for (var i = 0; i < Shaders.Count; i++)
            {
                int k = i;
                if (Condition(Shaders[k]))
                {
                    void Do()
                    {
                        Shaders[k].Bind();
                        Action(Shaders[k]);
                    }

                    if (InSameThread) Do();
                    else Executer.ExecuteOnMainThread(Do);
                }
            }

            void Cleanup()
            {
                Renderer.ShaderBound = previousProgram;
                Renderer.BindShader(Renderer.ShaderBound);
            }
            if (InSameThread) Cleanup();
            else Executer.ExecuteOnMainThread(Cleanup);
        }

        public static PointLight GetAvailableLight()
        {
            for(var i = 0; i < PointLights.Length; i++)
            {
                if (PointLights[i].Locked) continue;
                PointLights[i].Radius = PointLight.DefaultRadius;
                PointLights[i].Color = Vector3.Zero;
                PointLights[i].Position = Vector3.Zero;
                PointLights[i].Locked = true;
                return PointLights[i];
            }
            return null;
        }

        public static void UpdateLight(PointLight Light)
        {
            if (!Light.Locked)
            {
                Light.Radius = PointLight.DefaultRadius;
                Light.Color = Vector3.Zero;
            }
            var lightIndex = Array.IndexOf(PointLights, Light);
            if(lightIndex == -1) return;

            ShaderManager.Do(S => S.PointLightsColorUniform[lightIndex] != -1, delegate (Shader Shader)
            {
                Renderer.Uniform3(Shader.PointLightsColorUniform[lightIndex], Light.Color);
                Renderer.Uniform3(Shader.PointLightsPositionUniform[lightIndex], Light.Position);
                Renderer.Uniform1(Shader.PointLightsRadiusUniform[lightIndex], Light.Radius);
            });
        }
        
        public static Vector3 LightColor
        {
            get => _lightColor;
            set
            {
                _lightColor = value;
                ShaderManager.Do(S => S.LightColorLocation != -1, delegate (Shader Shader)
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

        public static int UsedLights
        {
            get
            {
                var usedLights = 0;
                for (var i = 0; i < PointLights.Length; i++)
                {
                    if (PointLights[i].Locked) usedLights++;
                }
                return usedLights;
            }
        }
    }
}
