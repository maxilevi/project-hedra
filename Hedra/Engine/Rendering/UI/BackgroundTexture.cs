/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Management;
using System.IO;
using Hedra.Engine.Game;
using Hedra.Game;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of Texture.
    /// </summary>
    public class BackgroundTexture : UIElement
    {
        public GUITexture TextureElement;
        public bool Enabled { get; private set; }

        public BackgroundTexture(string AssetPath, Vector2 Position, Vector2 Scale)
        {
            this.TextureElement = new GUITexture(Graphics2D.LoadFromAssets(AssetPath),
                Graphics2D.SizeFromAssets(AssetPath) * Scale, Position) {Enabled = true};
            DrawManager.UIRenderer.Add(this.TextureElement);
        }

        public BackgroundTexture(uint TextureId, Vector2 Position, Vector2 Scale)
        {
            this.TextureElement = new GUITexture(TextureId, Scale, Position);
            this.TextureElement.Enabled = true;
            DrawManager.UIRenderer.Add(this.TextureElement);
        }
        public BackgroundTexture(Vector4 TextureColor, Vector2 Position, Vector2 Scale)
        {
            this.TextureElement = new GUITexture(Graphics2D.ColorTexture(TextureColor), Scale, Position);
            this.TextureElement.Enabled = true;
            DrawManager.UIRenderer.Add(this.TextureElement);
        }
        
        public BackgroundTexture(Color GradientColor0, Color GradientColor1, Vector2 Position, Vector2 Scale, GradientType Type)
        {
            this.TextureElement = new GUITexture(CreateGradient(Scale, GradientColor0, GradientColor1, Type), Scale, Position)
            {
                Enabled = true
            };
            DrawManager.UIRenderer.Add(this.TextureElement);
        }

        private uint CreateGradient(Vector2 Scale, Color GradientColor0, Color GradientColor1, GradientType Type)
        {
            var bmp = new Bitmap( (int) (Scale.X * GameSettings.Width+1), (int) (Scale.Y*GameSettings.Height+1));
            bmp = Graphics2D.CreateGradient( GradientColor0, GradientColor1, Type, bmp);
            return Graphics2D.LoadTexture(new BitmapObject
            {
                Bitmap = bmp,
                Path = $"UI:Gradient:{GradientColor0}-{GradientColor1}"
            });
        }

        public void SendBack()
        {
            DrawManager.UIRenderer.SendBack(TextureElement);
        }
        
        public Vector2 Scale
        {
            get => TextureElement.Scale;
            set => TextureElement.Scale = value;
        }
        
        public Vector2 Position
        {
            get => TextureElement.Position;
            set => this.TextureElement.Position = value;
        }

        public void Enable()
        {
            TextureElement.Enabled = true;
            Enabled = true;            
        }

        public void Disable()
        {
            TextureElement.Enabled = false;    
            Enabled = false;
        }
        
        public void Dispose()
        {
            DrawManager.UIRenderer.Remove(this.TextureElement);
        }
    }
}
