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
        private readonly HashSet<TextureCommand> _renderableUISet;
        private readonly List<TextureCommand> _renderableUIList;
        private readonly HashSet<GUITexture> _textures;
        private static bool _inited;
        private static VAO<Vector2> _vao;
        private static VBO<Vector2> _vbo;
        private readonly object _lock;

        public int DrawCount { get; private set; }
        public int RenderableCount => _renderableUISet.Count;
        public int TextureCount => _textures.Count;

        public GUIRenderer()
        {
            Shader = Shader.Build("Shaders/GUI.vert", "Shaders/GUI.frag");
            _renderableUISet = new HashSet<TextureCommand>();
            _renderableUIList = new List<TextureCommand>();
            _textures = new HashSet<GUITexture>();
            _vbo = new VBO<Vector2>(
                new[]
                {
                    new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1),
                    new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, -1)
                }, 6 * Vector2.SizeInBytes, VertexAttribPointerType.Float);

            _vao = new VAO<Vector2>(_vbo);
            _lock = new object();
            var bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, Color.FromArgb(0, 0, 0, 0));
            TransparentTexture = Graphics2D.LoadTexture(bmp);
            InmortalTextures = new[] {TransparentTexture};
        }

        public DrawOrder GetDrawOrder(IRenderable Renderable)
        {
            lock (_lock)
            {
                return _renderableUIList.First(T => T.Renderable == Renderable).Order;
            }
        }

        public void SetDrawOrder(IRenderable Renderable, DrawOrder Order)
        {
            lock (_lock)
            {
                var command = _renderableUIList.First(T => T.Renderable == Renderable);
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
            lock(_lock)
            {
                if(!_textures.Contains(Texture))
                    _textures.Add(Texture);
            }
        }

        public void Add(IRenderable Texture)
        {
            lock (_lock)
            {
                var command = new TextureCommand(Texture, DrawOrder.Before);
                _renderableUIList.Add(command);
                _renderableUISet.Add(command);
            }
        }

        public void Add(IRenderable Texture, DrawOrder Order)
        {
            lock (_lock)
            {
                var command = new TextureCommand(Texture, Order);
                _renderableUIList.Add(command);
                _renderableUISet.Add(command);
            }
        }

        public void Remove(GUITexture Texture)
        {
            lock(_lock)
            {
                if (_textures.Contains(Texture))
                    _textures.Remove(Texture);
            }
        }

        public void Remove(IRenderable Texture)
        {
            lock (_lock)
            {
                _renderableUIList.Remove(_renderableUIList.FirstOrDefault(Command => Command.Renderable == Texture));
                _renderableUISet.RemoveWhere(Command => Command.Renderable == Texture);
            }
        }

        public void Draw()
        {
            DrawCount = 0;
            lock (_lock)
            {
                for(var i = 0; i < _renderableUIList.Count; i++)
                {
                    if (_renderableUIList[i].Order == DrawOrder.Before)
                        _renderableUIList[i].Renderable.Draw();
                }
            }
            SetDraw();
            lock (_lock)
            {
                foreach (GUITexture texture in _textures)
                {
                    if (texture == null || !texture.Enabled || texture.Scale == Vector2.Zero) continue;
                    DrawCount++;
                    this.BaseDraw(texture);
                }
            }
            UnsetDrawing();
            lock (_lock)
            {
                for (var i = 0; i < _renderableUIList.Count; i++)
                {
                    if (_renderableUIList[i].Order == DrawOrder.After)
                        _renderableUIList[i].Renderable.Draw();
                }
            }
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
            Renderer.Enable(EnableCap.Texture2D);
            Renderer.Enable(EnableCap.Blend);
            Renderer.Disable(EnableCap.DepthTest);
        }

        private static void UnsetDrawing()
        {
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);
            Renderer.Disable(EnableCap.Texture2D);
            Renderer.Enable(EnableCap.CullFace);
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
            _vbo.Dispose();
        }
    }
}