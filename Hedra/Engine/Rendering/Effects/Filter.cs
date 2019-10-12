/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using OpenTK.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of IFilter.
    /// </summary>
    public abstract class Filter : IDisposable
    {    

        public abstract void Dispose();
        public abstract void Pass(FBO Src, FBO Dst);
        
        public virtual void DrawQuad(Shader DrawingShader, uint TexID, uint Additive = 0, bool Flipped = false)
        {
            Renderer.Disable(EnableCap.DepthTest);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TexID);

            if (DrawingShader.HasUniform("Scale")) DrawingShader["Scale"] = Vector2.One;
            if (DrawingShader.HasUniform("Position"))  DrawingShader["Position"] = Vector2.Zero;

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
