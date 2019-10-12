/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/12/2016
 * Time: 03:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using OpenToolkit.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.Frustum
{
    /// <summary>
    /// Description of Occludable.
    /// </summary>
    public abstract class Occludable : IDisposable
    {
        private static readonly Vector3 BoxMargin = Vector3.One * 32;
        private static Shader Passthrough { get; }
        private OcclusionState _state;
        private int _occlusionQueryId;
        private bool _occluded;

        public bool Occluded
        {
            get => _occluded && GameSettings.OcclusionCulling;
            set => _occluded = value;
        }

        static Occludable()
        {
            Passthrough = Shader.Build("Shaders/OccludePassthrough.vert", "Shaders/Passthrough.frag");
        }
        
        protected Occludable()
        {
            Executer.ExecuteOnMainThread( () => _occlusionQueryId = Renderer.GenQuery() );
        }
        
        public void DrawQuery()
        {
            if (IsFrustumInsideBox())
            {
                Occluded = false;
                return;
            }
            if(_occlusionQueryId == 0) return;
            
            var isAvailable = 0;
    
            if(_state == OcclusionState.Waiting) 
                Renderer.GetQueryObject(_occlusionQueryId, GetQueryObjectParam.QueryResultAvailable, out isAvailable);
    
            if(isAvailable != 0)
            {
                Renderer.GetQueryObject(_occlusionQueryId, GetQueryObjectParam.QueryResult, out var passed);
                _state = passed != 0 ? OcclusionState.Visible : OcclusionState.Hidden;
                Occluded = _state == OcclusionState.Hidden;
            }
            
            if (_state != OcclusionState.Waiting && !GameSettings.LockFrustum)
            {
                _state = OcclusionState.Waiting;
                Renderer.BeginQuery(QueryTarget.AnySamplesPassed, _occlusionQueryId);

                Draw();
                
                Renderer.EndQuery(QueryTarget.AnySamplesPassed);
            }
        }

        private void Draw()
        {
            Renderer.Disable(EnableCap.CullFace);
            
            Passthrough["Scale"] = Max - Min;
            Passthrough["Position"] = Min;
            Renderer.DrawElements(PrimitiveType.Triangles, BasicGeometry.CubeIndicesVBO.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);
            
            Renderer.Enable(EnableCap.CullFace);
        }
        
        public static void Bind()
        {
            Renderer.ColorMask(false, false, false, false);
            Renderer.DepthMask(false);
            Renderer.Enable(EnableCap.CullFace);
            Passthrough.Bind();
            BasicGeometry.CubeVAO.Bind();
            BasicGeometry.CubeIndicesVBO.Bind();

        }

        public static void Unbind()
        {
            Renderer.ColorMask(true, true, true, true);
            Renderer.DepthMask(true);
            BasicGeometry.CubeVAO.Unbind();
            BasicGeometry.CubeIndicesVBO.Unbind();
            Passthrough.Unbind();
        }

        private bool IsFrustumInsideBox()
        {
            var frustumPosition = GameManager.Player.View.CameraEyePosition;
            return frustumPosition.X > Min.X && frustumPosition.X < Max.X
                && frustumPosition.Y > Min.Y && frustumPosition.Y < Max.Y
                && frustumPosition.Z > Min.Z && frustumPosition.Z < Max.Z;
        }
        
        public virtual void Dispose()
        {
            Executer.ExecuteOnMainThread( () => Renderer.DeleteQuery( _occlusionQueryId ) );
        }

        private Vector3 Min => OcclusionMin - BoxMargin;
        private Vector3 Max => OcclusionMax + BoxMargin;

        protected abstract Vector3 OcclusionMin { get; }
        protected abstract Vector3 OcclusionMax { get; }
    }
}
