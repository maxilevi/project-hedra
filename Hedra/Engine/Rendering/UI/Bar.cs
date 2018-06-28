/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/06/2016
 * Time: 07:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.UI
{
    public class Bar : IRenderable, UIElement, IDisposable
    {
        public static Vector4 Low = new Vector4(0.878f, 0.196f, 0.235f, 1);
        public static Vector4 Middle = new Vector4(1, 0.839f, 0.149f, 1);
        public static Vector4 Full = new Vector4(0.4f, 0.6627451f, 0.4f, 1);
        public static Vector4 Blue = new Vector4(0.200f, 0.514f, 0.714f, 1.000f);
        public static Vector4 Violet = new Vector4(0.941f, 0.008f, 0.608f, 1.000f);
        public static Vector4 Poison = new Vector4(0.282f, 0.725f, 0.373f, 1.000f);

        private static uint BarBlueprint = Graphics2D.LoadFromAssets("Assets/Bar.png");
        private static uint RectangleBlueprint = Graphics2D.ColorTexture(Colors.FromArgb(255, 29, 29, 29));


        public static Shader Shader = Shader.Build("Shaders/Bar.vert", "Shaders/Bar.frag");
        private float _barSize;
        private bool _enabled;
        private Panel _inPanel;
        private Func<float> _max;

        private Vector2 _position;
        private bool _showText = true;

        private Func<float> _value;
        public Vector4 BackgroundColor = new Vector4(0.1529f, 0.1529f, 0.1529f, 1);
        public bool CurvedBorders;
        public DrawOrder Order;
        public bool ShowBar = true;
        public Vector2 TargetResolution = new Vector2(1024, 578);
        public RenderableText Text;
        public Vector4 UniformColor;

        public bool UpdateTextRatio = true;

        public Bar(Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel,
            DrawOrder Order = DrawOrder.Before, bool CurvedBorders = true)
        {
            this.Initialize(Position, Scale, Value, Max, InPanel, null, Order, CurvedBorders);
        }

        public Bar(Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Vector4 Color, Panel InPanel,
            DrawOrder Order = DrawOrder.Before, bool CurvedBorders = true)
        {
            UniformColor = Color;
            this.Initialize(Position, Scale, Value, Max, InPanel, null, Order, CurvedBorders);
        }

        public Bar(Vector2 Position, Vector2 Scale, string Text, Func<float> Value, Func<float> Max, Panel InPanel,
            DrawOrder Order = DrawOrder.Before)
        {
            this.Initialize(Position, Scale, Value, Max, InPanel, Text, Order);
        }

        public bool ShowText
        {
            get { return _showText; }
            set
            {
                _showText = value;
                if (value)
                    Text.Enable();
                else
                    Text.Disable();
            }
        }

        public void Dispose()
        {
            Text.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }

        public void Draw()
        {
            if (!_enabled)
                return;

            _barSize = Mathf.Clamp(Mathf.Lerp(_barSize, _value() / _max(), Time.unScaledDeltaTime * 8f), 0, 1);

            if (UpdateTextRatio)
                Text.Text = (int) _value() + " / " + (int) _max();
            Shader.Bind();          
            GraphicsLayer.Disable(EnableCap.DepthTest);
            GraphicsLayer.Enable(EnableCap.Blend);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, CurvedBorders ? BarBlueprint : RectangleBlueprint);

            Shader["Scale"] =
                Mathf.DivideVector(TargetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) +
                Mathf.DivideVector(TargetResolution * new Vector2(0.015f, 0.015f),
                    new Vector2(GameSettings.Width, GameSettings.Height));
            Shader["Position"] = Position;
            Shader["Color"] = BackgroundColor;

            DrawManager.UIRenderer.SetupQuad();
            DrawManager.UIRenderer.DrawQuad();

            Shader["Scale"] = ShowBar
                    ? Mathf.DivideVector(TargetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) *
                      new Vector2(_barSize, 1)
                    : new Vector2(0, 0);
            Shader["Position"] = Position;
            Shader["Color"] =  UniformColor != Vector4.Zero ? UniformColor : _barSize > 0.6f ? Full : _barSize < 2.5f ? Low : Middle;

            DrawManager.UIRenderer.DrawQuad();

            GraphicsLayer.Enable(EnableCap.CullFace);
            GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Enable(EnableCap.DepthTest);
            GraphicsLayer.Enable(EnableCap.CullFace);
            Shader.Unbind();
        }

        public Vector2 Scale { get; set; }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (Text != null)
                    Text.Position = _position;
            }
        }

        public void Enable()
        {
            _enabled = true;
            Text.Enable();
        }

        public void Disable()
        {
            _enabled = false;
            Text.Disable();
        }

        private void Initialize(Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel,
            string OptionalText, DrawOrder Order = DrawOrder.Before, bool CurvedBorders = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Order = Order;
            this.CurvedBorders = CurvedBorders;
            _value = Value;
            _max = Max;
            _inPanel = InPanel;

            DrawManager.UIRenderer.Add(this, Order);
            if (OptionalText == null)
            {
                Text = new RenderableText(Value() + " / " + Max(), Position, Color.White,
                    FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
            }
            else
            {
                Text = new RenderableText(OptionalText, Position, Color.White,
                    FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
                UpdateTextRatio = false;
            }
            DrawManager.UIRenderer.Add(Text, this.Order);
            InPanel.AddElement(Text);
        }
    }
}