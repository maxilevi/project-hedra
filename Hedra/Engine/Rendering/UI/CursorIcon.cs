/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 04:12 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine.Input;
using OpenToolkit.Mathematics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Input;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of Cursor.
    /// </summary>
    public class CursorIcon : IRenderable
    {    
        public uint TextureId { get; set; }
        public Vector2 Position {get; set;}
        public Vector2 Scale { get; set; }
        private static Shader _shader;

        static CursorIcon()
        {
            _shader = Shader.Build("Shaders/Cursor.vert", "Shaders/Cursor.frag");
        }

        public CursorIcon(uint TextureId)
        {
            this.TextureId = TextureId;
            Scale = Mathf.ScaleGui(new Vector2(1024, 576), new Vector2(0.05f * 0.5f, 0.08f * 0.5f));
            DrawManager.Add(this);
        }
        
        public void Draw()
        {
            if(this.Position.X >= .98f || this.Position.Y >= .98f || this.Position.X <= -.98f || this.Position.Y <= -.98f){
                Program.GameWindow.CursorVisible = true;
                return;
            }
            
            if(!Cursor.Show)
                return;
        }

        public void Dispose()
        {
            DrawManager.Remove(this);
        }
    }
}
