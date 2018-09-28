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
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.UI
{
    public class Bar : IRenderable, UIElement, IAdjustable
    {
        private static uint _barBlueprint;
        private static uint _rectangleBlueprint;

        private static readonly Shader Shader = Shader.Build("Shaders/Bar.vert", "Shaders/Bar.frag");
        private float _barSize;
        private bool _enabled;
        private Panel _inPanel;
        private Func<float> _max;

        private Vector2 _position;
        private bool _showText = true;
        private bool _builded;
        private string _optionalText;
        private Func<float> _value;
        public Vector4 BackgroundColor = new Vector4(0.1529f, 0.1529f, 0.1529f, 1);
        public bool CurvedBorders;
        private DrawOrder Order;
        public bool ShowBar = true;
        private Vector2 TargetResolution = new Vector2(1024, 578);
        public RenderableText Text;
        private Vector4 UniformColor;
        public Vector2 AdjustedPosition { get; set; }

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

        public void Dispose()
        {
            Text.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }

        public void Draw()
        {
            if (!_enabled)
                return;

            _barSize = Mathf.Clamp(Mathf.Lerp(_barSize, _value() / _max(), Time.IndependantDeltaTime * 8f), 0, 1);

            if (UpdateTextRatio)
                Text.Text = (int) _value() + " / " + (int) _max();
            Shader.Bind();          
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, CurvedBorders ? _barBlueprint : _rectangleBlueprint);

            Shader["Scale"] =
                Mathf.DivideVector(TargetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) +
                Mathf.DivideVector(TargetResolution * new Vector2(0.015f, 0.015f),
                    new Vector2(GameSettings.Width, GameSettings.Height));
            Shader["Position"] = AdjustedPosition;
            Shader["Color"] = BackgroundColor;

            DrawManager.UIRenderer.SetupQuad();
            DrawManager.UIRenderer.DrawQuad();

            Shader["Scale"] = ShowBar
                    ? Mathf.DivideVector(TargetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) *
                      new Vector2(_barSize, 1)
                    : new Vector2(0, 0);
            Shader["Position"] = AdjustedPosition;
            Shader["Color"] =  UniformColor != Vector4.Zero 
                ? UniformColor : _barSize > 0.6f 
                ? Colors.FullHealthGreen : _barSize < 2.5f 
                ? Colors.LowHealthRed : throw new ArgumentOutOfRangeException("Health is out of range");

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.CullFace);
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
            Shader.Unbind();
        }

        public Vector2 Scale { get; set; }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                if (Text != null)
                    Text.Position = _position;
                this.Adjust();
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

        public void Adjust()
        {
            AdjustedPosition = GUITexture.Adjust(Position);
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
            _optionalText = OptionalText;
            this.Build();
        }

        private void Build()
        {
            DrawManager.UIRenderer.Add(this, Order);
            if (_optionalText == null)
            {
                Text = new RenderableText(_value() + " / " + _max(), Position, Color.White,
                    FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
            }
            else
            {
                Text = new RenderableText(_optionalText, Position, Color.White,
                    FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
                UpdateTextRatio = false;
            }
            DrawManager.UIRenderer.Add(Text, this.Order);
            _inPanel.AddElement(Text);
            
            if (_barBlueprint == 0)
            {
                Executer.ExecuteOnMainThread(delegate
                {
                    _barBlueprint = Graphics2D.LoadFromAssets("Assets/Bar.png"); 
                    
                });
            }
            if (_rectangleBlueprint == -1)
            {
                Executer.ExecuteOnMainThread(delegate
                {
                    _rectangleBlueprint = Graphics2D.ColorTexture(Colors.FromArgb(255, 29, 29, 29));
                });
            }
        }
    }
}