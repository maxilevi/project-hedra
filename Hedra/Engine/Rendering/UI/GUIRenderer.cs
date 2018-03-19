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
        public static GUIShader Shader = new GUIShader("Shaders/GUI.vert", "Shaders/GUI.frag");
        public static uint TransparentTexture { get; private set; }
        private readonly HashSet<TextureCommand> _renderableUi;
        private readonly HashSet<GUITexture> _textures;
        private static bool _inited;
        private static VAO<Vector2> _vao;
        private static VBO<Vector2> _vbo;

        public int DrawCount { get; private set; }
        public int RenderableCount => _renderableUi.Count;
        public int TextureCount => _textures.Count;

        public GUIRenderer()
        {
            _renderableUi = new HashSet<TextureCommand>();
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

            DisposeManager.Add(this);
        }

        public void SetupQuad()
        {
            _vao.Bind();
            GL.EnableVertexAttribArray(0);
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

                //Textures[i].Position = new Vector2(Textures[i].Position.X * Constants.WIDTH / NewWidth, Textures[i].Position.Y * Constants.HEIGHT / NewHeight);
                texture.Scale = new Vector2(texture.Scale.X * Constants.WIDTH / NewWidth, texture.Scale.Y * Constants.HEIGHT / NewHeight);

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
            lock (_renderableUi)
            {
                _renderableUi.Add(new TextureCommand(Texture, DrawOrder.Before));
            }
        }

        public void Add(IRenderable Texture, DrawOrder Order)
        {
            lock (_renderableUi)
            {
                _renderableUi.Add( new TextureCommand(Texture, Order));
            }
        }

        public void Remove(GUITexture Texture)
        {
            if (_textures.Contains(Texture))
                _textures.Remove(Texture);
        }

        public void Remove(IRenderable Texture)
        {
            lock (_renderableUi)
            {
                _renderableUi.RemoveWhere(Command => Command.Renderable == Texture);
            }
        }

        public void Draw()
        {
            DrawCount = 0;
            HashSet<TextureCommand> tempDic;
            lock (_renderableUi)
                tempDic = new HashSet<TextureCommand>(_renderableUi);
            
            foreach (TextureCommand command in tempDic) if (command.Order == DrawOrder.Before) command.Renderable.Draw();

            SetDraw();

            GUITexture[] texturesArray = _textures.ToArray();
            foreach (GUITexture texture in texturesArray)
            {
                if (texture == null || !texture.IsEnabled) continue;
                DrawCount++;
                this.BaseDraw(texture);
            }
            UnsetDrawing();

            foreach (TextureCommand command in tempDic) if (command.Order == DrawOrder.After) command.Renderable.Draw();
        }

        private void BaseDraw(GUITexture Texture)
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Texture.BackGroundId);
            GL.Uniform1(Shader.BackGroundUniform, 1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture.Id);
            GL.Uniform1(Shader.GUIUniform, 0);

            Vector2 scale = Texture.Scale;
            this.SetupQuad();

            GL.Uniform2(Shader.ScaleUniform, scale);
            GL.Uniform2(Shader.SizeUniform, new Vector2(1.0f / Constants.WIDTH, 1.0f / Constants.HEIGHT));
            GL.Uniform1(Shader.FxaaUniform, Texture.Fxaa ? 1.0f : 0.0f);
            GL.Uniform2(Shader.PositionUniform, Texture.Position);
            GL.Uniform4(Shader.ColorUniform, Texture.Color);
            GL.Uniform1(Shader.FlippedUniform, Texture.IdPointer == null && !Texture.Flipped ? 0 : 1);
            GL.Uniform1(Shader.OpacityUniform, Texture.Opacity);
            GL.Uniform1(Shader.GrayscaleUniform, Texture.Grayscale ? 1 : 0);
            GL.Uniform4(Shader.TintUniform, Texture.Tint);

            Matrix3 rot = Texture.Angle == 0 ? Matrix3.Identity : Texture.RotationMatrix;
            GL.UniformMatrix3(Shader.RotationUniform, false, ref rot);

            this.DrawQuad();
        }

        private static void SetDraw()
        {
            Shader.Bind();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
        }

        private static void UnsetDrawing()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            Shader.UnBind();
        }

        public void Draw(GUITexture Texture)
        {
            if (!Texture.IsEnabled) return;
            
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