/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/09/2017
 * Time: 04:48 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Frustum;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of DropShadowRenderer.
    /// </summary>
    public class DropShadowRenderer
    {
        public static Shader Shader;
        private List<DropShadow> _shouldShadows;
        private readonly List<DropShadow> _shadows;
        private readonly HashSet<Vector3> _shadowPositions;
        public int Count {get; private set;}

        static DropShadowRenderer()
        {
            Shader = Shader.Build("Shaders/DropShadows.vert", "Shaders/DropShadows.frag");
        }

        public DropShadowRenderer()
        {
            _shouldShadows = new List<DropShadow>();
            _shadows = new List<DropShadow>();
            _shadowPositions = new HashSet<Vector3>();
        }
        
        public void Add(DropShadow Shadow){
            lock(_shadows)
                _shadows.Add(Shadow);

            lock (_shadowPositions)
                _shadowPositions.Add(Shadow.Position.Xz.ToVector3());

        }

        public void Remove(DropShadow Shadow)
        {
            lock (_shadows)
                _shadows.Remove(Shadow);

            /*TODO: since shadows can be moved outside of the renderer's scope this might create a small memory leak for dynamic shadows because the position on the hashtable hasnt been updated.*/
            lock (_shadowPositions)
            {
                if (_shadowPositions.Contains(Shadow.Position))
                    _shadowPositions.Remove(Shadow.Position);
            }
    }
        
        public DropShadow Get(Vector3 ShadowPosition){
            return _shadows.Find( Shadow => Shadow.Position == ShadowPosition);
        }
        
        public bool Exists(Vector3 ShadowPosition)
        {
            lock (_shadowPositions)
                return _shadowPositions.Contains(ShadowPosition);
            
        }    

        public void Draw()
        {         
            lock(_shadows)
            {
                _shouldShadows.Clear();
                for(var i = _shadows.Count-1; i > -1; --i)
                {
                    if (_shadows[i].DeleteWhen != null && _shadows[i].DeleteWhen())
                    {
                        this.Remove(_shadows[i]);
                        continue;
                    }

                    if ((!GameSettings.SSAO || _shadows[i].IsCosmeticShadow) && _shadows[i].ShouldDraw && Culling.IsInside(_shadows[i].Position))
                        _shouldShadows.Add(_shadows[i]);
                }
                _shouldShadows = _shouldShadows.OrderBy(S => S.Position.Y).ToList();
            }
            Count = _shouldShadows.Count;
            if(_shouldShadows.Count > 0){

                _shouldShadows = _shouldShadows.OrderBy( Shadow => Shadow.Position.Y).ToList();
                Shader.Bind();
                Renderer.Enable(EnableCap.Blend);
                Renderer.Disable(EnableCap.DepthTest);

                for (int i = 0; i < _shouldShadows.Count; i++)
                {
                    if (_shouldShadows[i].DepthTest)
                        Renderer.Enable(EnableCap.DepthTest);

                    Shader["Rotation"] = _shouldShadows[i].Rotation;
                    Shader["Opacity"] = _shouldShadows[i].Opacity;
                    Shader["PlayerPosition"] = GameManager.Player.Position;
                    Shader["Position"] = _shouldShadows[i].Position;
                    Shader["Scale"] = _shouldShadows[i].Scale;
                    
                    DrawManager.UIRenderer.DrawQuad();
                    
                    if(_shouldShadows[i].DepthTest)
                        Renderer.Disable(EnableCap.DepthTest);
                }
                Renderer.Enable(EnableCap.DepthTest);
                Renderer.Disable(EnableCap.Blend);
                Shader.Unbind();
            }
        }
    }
}