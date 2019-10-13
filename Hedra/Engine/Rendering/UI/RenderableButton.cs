/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 09:53 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Drawing;
using OpenToolkit.Mathematics;

using Hedra.Engine.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.Sound;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.UI
{
    
    public class RenderableButton : Button, IRenderable, IAdjustable
    {
        private static readonly uint _marker = Graphics2D.ColorTexture(Colors.Blue);
        public RenderableButton(Vector2 Position, Vector2 Scale, uint Texture) : base(Position, Scale, Texture)
        {
            DrawManager.UIRenderer.Remove(this.Texture);
        }
        
        public void Draw()
        {
            if (!Texture.Enabled || Texture.IdPointer == null && Texture.Id == GUIRenderer.TransparentTexture) return;
            if (this.Texture != null)      
                DrawManager.UIRenderer.Draw(this.Texture);
        }
        
        ~RenderableButton(){
            Executer.ExecuteOnMainThread( ()=> base.Dispose() );
        }

        public void Adjust()
        {
            this.Texture.Adjust();
        }
    }
}
