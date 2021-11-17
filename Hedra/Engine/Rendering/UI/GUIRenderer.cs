using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of GUIRenderer.
    /// </summary>
    public class GUIRenderer : IDisposable
    {
        public static Shader Shader;
        private static bool _inited;
        private static VAO<Vector2> _vao;
        private static VBO<Vector2> _vbo;
        private readonly object _lock;
        private readonly List<TextureCommand> _renderableUIList;
        private readonly HashSet<TextureCommand> _renderableUISet;
        private readonly List<GUITexture> _textures;

        public GUIRenderer()
        {
            Shader = Shader.Build("Shaders/GUI.vert", "Shaders/GUI.frag");
            _renderableUISet = new HashSet<TextureCommand>();
            _renderableUIList = new List<TextureCommand>();
            _textures = new List<GUITexture>();
            _vbo = new VBO<Vector2>(
                new[]
                {
                    new Vector2(1, 1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                    new Vector2(1, -1),
                    new Vector2(1, 1),
                    new Vector2(-1, -1)
                }, 6 * HedraSize.Vector2, VertexAttribPointerType.Float);

            _vao = new VAO<Vector2>(_vbo);
            _lock = new object();
            var bmp = new Image<Rgba32>(1, 1);
            bmp[0, 0] = Color.Transparent;

        TransparentTexture = Graphics2D.LoadTexture(new BitmapObject
            {
                Bitmap = bmp,
                Path = "UI:TransparentTexture"
            });
            ImmortalTextures = new[] { TransparentTexture };
        }

        public static uint TransparentTexture { get; private set; }
        public static uint[] ImmortalTextures { get; private set; }

        public int DrawCount { get; private set; }
        public int RenderableCount => _renderableUISet.Count;
        public int TextureCount => _textures.Count;

        public void Dispose()
        {
            _vbo.Dispose();
        }

        public void DrawQuad()
        {
            _vao.Bind();
            Renderer.DrawArrays(PrimitiveType.Triangles, 0, _vbo.Count);
            _vao.Unbind();
        }

        public void Add(GUITexture Texture)
        {
            lock (_lock)
            {
                if (!_textures.Contains(Texture))
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
            lock (_lock)
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

        public void Adjust()
        {
            lock (_lock)
            {
                foreach (var texture in _textures) texture.Adjust();

                foreach (var item in _renderableUISet)
                    if (item.Renderable is IAdjustable adjustable)
                        adjustable.Adjust();
            }
        }

        public void SendBack(GUITexture Texture)
        {
            _textures.Remove(Texture);
            _textures.Insert(0, Texture);
        }

        public void Draw()
        {
            DrawCount = 0;
            lock (_lock)
            {
                for (var i = 0; i < _renderableUIList.Count; i++)
                    if (_renderableUIList[i].Order == DrawOrder.Before)
                        _renderableUIList[i].Renderable.Draw();
            }

            SetDraw();
            lock (_lock)
            {
                foreach (var texture in _textures)
                {
                    if (texture == null || !texture.Enabled || texture.Scale == Vector2.Zero) continue;
                    var id = texture.Id;
                    if (IsValidId(id)) BaseDraw(texture, id);
                }
            }

            UnsetDrawing();
            lock (_lock)
            {
                for (var i = 0; i < _renderableUIList.Count; i++)
                    if (_renderableUIList[i].Order == DrawOrder.After)
                        _renderableUIList[i].Renderable.Draw();
            }
        }

        private void BaseDraw(GUITexture Texture, uint Id)
        {
            if (Texture.Scale == Vector2.Zero || !Texture.Enabled) return;

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, Id);
            Shader["Texture"] = 0;

            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, Texture.BackGroundId);
            Shader["Background"] = 1;

            if (Texture.UseMask)
            {
                Renderer.ActiveTexture(TextureUnit.Texture2);
                Renderer.BindTexture(TextureTarget.Texture2D, Texture.MaskId);
                Shader["Mask"] = 2;
            }

            Shader["Scale"] = Texture.Scale;
            Shader["Position"] = Texture.AdjustedPosition;
            Shader["Flipped"] = Texture.IdPointer == null && !Texture.Flipped ? 0 : 1;
            Shader["Opacity"] = Texture.Opacity;
            Shader["Grayscale"] = Texture.Grayscale ? 1 : 0;
            Shader["Tint"] = Texture.Tint;
            Shader["Rotation"] = Math.Abs(Texture.Angle) < .05f ? Matrix4x4.Identity : Texture.RotationMatrix;
            Shader["UseMask"] = Texture.UseMask ? 1 : 0;

            DrawQuad();
            DrawCount++;
        }

        public static void SetDraw(Shader CustomProgram = null)
        {
            (CustomProgram ?? Shader).Bind();
            Renderer.Enable(EnableCap.Blend);
            Renderer.Disable(EnableCap.DepthTest);
        }

        public static void UnsetDrawing(Shader CustomProgram = null)
        {
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.CullFace);
            (CustomProgram ?? Shader).Unbind();
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture2);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void SetTexture(uint Position, uint Id)
        {
            Renderer.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + Position));
            Renderer.BindTexture(TextureTarget.Texture2D, Id);
        }

        public void Draw(GUITexture Texture, Shader CustomProgram = null)
        {
            if (!Texture.Enabled) return;
            var id = Texture.Id;
            if (IsValidId(id))
            {
                SetDraw(CustomProgram);
                BaseDraw(Texture, id);
                UnsetDrawing(CustomProgram);
            }
        }

        private static bool IsValidId(uint Id)
        {
            return Id != TransparentTexture && Id != 0;
        }
    }
}