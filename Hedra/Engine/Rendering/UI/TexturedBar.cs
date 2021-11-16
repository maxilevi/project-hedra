/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/06/2016
 * Time: 07:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.UI
{
    public class TexturedBar : DrawableTexture, IRenderable, UIElement, IAdjustable, ISimpleTexture
    {
        private static readonly Shader Shader;
        private readonly Func<float> _max;
        private readonly Func<float> _value;
        private float _barSize;
        private bool _enabled;
        private Vector2 _position;

        static TexturedBar()
        {
            Shader = Shader.Build("Shaders/Bar.vert", "Shaders/Bar.frag");
        }

        public TexturedBar(uint TextureId, Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max,
            DrawOrder Order = DrawOrder.After)
        {
            this.Scale = Scale;
            _value = Value;
            _max = Max;
            this.TextureId = TextureId;

            DrawManager.UIRenderer.Add(this, Order);
            this.Position = Position;
        }

        public bool ShowBar { get; set; } = true;
        public Vector2 AdjustedPosition { get; private set; }

        public bool Centered { get; set; }

        public void Adjust()
        {
            AdjustedPosition = GUITexture.Adjust(Position);
        }

        public void Draw()
        {
            if (!_enabled || Scale == Vector2.Zero)
                return;

            _barSize = Mathf.Clamp(Mathf.Lerp(_barSize, _value() / _max(), Time.DeltaTime * 8f), 0, 1);

            Shader.Bind();
            Renderer.Disable(EnableCap.CullFace);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TextureId);

            Shader["Scale"] = new Vector2(Scale.X * _barSize, Scale.Y);
            Shader["Position"] =
                AdjustedPosition - (!Centered ? new Vector2(Scale.X * (1f - _barSize), 0f) : Vector2.Zero);
            Shader["Color"] = -Vector4.One;

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
            Shader.Unbind();
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Vector2 Scale { get; set; }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                Adjust();
            }
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        public void Dispose()
        {
            DrawManager.UIRenderer.Remove(this);
        }
    }
}