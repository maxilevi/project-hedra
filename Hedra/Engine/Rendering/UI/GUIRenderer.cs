using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of GUIRenderer.
    /// </summary>
    public class GUIRenderer : IDisposable
    {
        public static Shader Shader;
        public static uint TransparentTexture { get; private set; }
        public static uint[] InmortalTextures { get; private set; }
        private readonly HashSet<TextureCommand> _renderableUI;
        private readonly HashSet<GUITexture> _textures;
        private static bool _inited;
        private static VAO<Vector2> _vao;
        private static VBO<Vector2> _vbo;

        public int DrawCount { get; private set; }
        public int RenderableCount => _renderableUI.Count;
        public int TextureCount => _textures.Count;

        public GUIRenderer()
        {
            Shader = Shader.Build("Shaders/GUI.vert", "Shaders/GUI.frag");
            _renderableUI = new HashSet<TextureCommand>();
            _textures = new HashSet<GUITexture>();
            _vbo = new VBO<Vector2>(
                new[]
                {
                    new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1),
                    new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, -1)
                }, 6 * Vector2.SizeInBytes, VertexAttribPointerType.Float);

            _vao = new VAO<Vector2>(_vbo);

            var bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, Color.FromArgb(0, 0, 0, 0));
            TransparentTexture = Graphics2D.LoadTexture(bmp);
            InmortalTextures = new[] {TransparentTexture};
        }

        public DrawOrder GetDrawOrder(IRenderable Renderable)
        {
            lock (_renderableUI)
            {
                return _renderableUI.First(T => T.Renderable == Renderable).Order;
            }
        }

        public void SetDrawOrder(IRenderable Renderable, DrawOrder Order)
        {
            lock (_renderableUI)
            {
                var command = _renderableUI.First(T => T.Renderable == Renderable);
                command.Order = Order;
            }
        }

        public void SetupQuad()
        {
            _vao.Bind();
        }

        public void DrawQuad()
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vbo.Count);
        }

        public void RescaleTextures(float NewWidth, float NewHeight)
        {
            foreach (GUITexture texture in _textures)
            {
                bool anchorScaleX = false, anchorScaleY = false, anchorPositionX = false, anchorPositionY = false;
                if (texture.Scale.X == 1) anchorScaleX = true;
                if (texture.Scale.Y == 1) anchorScaleY = true;
                if (texture.Position.X == 1) anchorPositionX = true;
                if (texture.Position.Y == 1) anchorPositionY = true;

                //Textures[i].Position = new Vector2(Textures[i].Position.X * GameSettings.Width / NewWidth, Textures[i].Position.Y * GameSettings.Height / NewHeight);
                texture.Scale = new Vector2(texture.Scale.X * GameSettings.Width / NewWidth, texture.Scale.Y * GameSettings.Height / NewHeight);

                //Textures[i].Position = new Vector2( (AnchorPositionX) ? 1 : Textures[i].Position.X, (AnchorPositionY) ? 1 : Textures[i].Position.Y);
                //_textures[i].Scale = new Vector2( (anchorScaleX) ? 1 : _textures[i].Scale.X, (anchorScaleY) ? 1 : _textures[i].Scale.Y);
            }
        }

        public void Add(GUITexture Texture)
        {
            if(!_textures.Contains(Texture))
                _textures.Add(Texture);
        }

        public void Add(IRenderable Texture)
        {
            lock (_renderableUI)
            {
                _renderableUI.Add(new TextureCommand(Texture, DrawOrder.Before));
            }
        }

        public void Add(IRenderable Texture, DrawOrder Order)
        {
            lock (_renderableUI)
            {
                _renderableUI.Add( new TextureCommand(Texture, Order));
            }
        }

        public void Remove(GUITexture Texture)
        {
            if (_textures.Contains(Texture))
                _textures.Remove(Texture);
        }

        public void Remove(IRenderable Texture)
        {
            lock (_renderableUI)
            {
                _renderableUI.RemoveWhere(Command => Command.Renderable == Texture);
            }
        }

        public void Draw()
        {
            DrawCount = 0;
            HashSet<TextureCommand> tempDic;
            lock (_renderableUI)
                tempDic = new HashSet<TextureCommand>(_renderableUI);
            
            foreach (TextureCommand command in tempDic) if (command.Order == DrawOrder.Before) command.Renderable.Draw();

            SetDraw();

            try
            {
                GUITexture[] texturesArray = _textures.ToArray();
                foreach (GUITexture texture in texturesArray)
                {
                    if (texture == null || !texture.Enabled || texture.Scale == Vector2.Zero) continue;
                    DrawCount++;
                    this.BaseDraw(texture);
                }
            }
            catch (ArgumentException e)
            {
                Log.WriteLine(e);
            }
            UnsetDrawing();

            foreach (TextureCommand command in tempDic) if (command.Order == DrawOrder.After) command.Renderable.Draw();
        }

        private void BaseDraw(GUITexture Texture)
        {
            if(Texture.Scale == Vector2.Zero || !Texture.Enabled) return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture.Id);
            Shader["Texture"] = 0;

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Texture.BackGroundId);
            Shader["Background"] = 1;

            if (Texture.UseMask)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, Texture.MaskId);
                Shader["Mask"] = 2;
            }

            Vector2 scale = Texture.Scale;
            this.SetupQuad();

            Shader["Scale"] = scale;
            Shader["Position"] = Texture.Position;
            Shader["Flipped"] = Texture.IdPointer == null && !Texture.Flipped ? 0 : 1;
            Shader["Opacity"] = Texture.Opacity;
            Shader["Grayscale"] = Texture.Grayscale ? 1 : 0;
            Shader["Tint"] = Texture.Tint;
            Shader["Rotation"] = Texture.Angle == 0 ? Matrix3.Identity : Texture.RotationMatrix;
            Shader["Size"] = new Vector2(1.0f / GameSettings.Width, 1.0f / GameSettings.Height);
            Shader["FXAA"] = Texture.Fxaa ? 1.0f : 0.0f;
            Shader["UseMask"] = Texture.UseMask ? 1 : 0;

            this.DrawQuad();
        }

        private static void SetDraw()
        {
            Shader.Bind();
            GraphicsLayer.Enable(EnableCap.Texture2D);
            GraphicsLayer.Enable(EnableCap.Blend);
            GraphicsLayer.Disable(EnableCap.DepthTest);
        }

        private static void UnsetDrawing()
        {
            GraphicsLayer.Enable(EnableCap.DepthTest);
            GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Disable(EnableCap.Texture2D);
            GraphicsLayer.Enable(EnableCap.CullFace);
            Shader.Unbind();
        }

        public void Draw(GUITexture Texture)
        {
            if (!Texture.Enabled) return;
            
            SetDraw();
            this.BaseDraw(Texture);
            UnsetDrawing();
        }

        public void Dispose()
        {
            GL.DeleteBuffers(1, ref _vbo.ID);
        }
    }
}