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
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.UI
{
    public class Bar : IRenderable, UIElement, IAdjustable
    {
        private static uint _barBlueprint;
        private static uint _rectangleBlueprint;

        private static readonly Shader Shader = Shader.Build("Shaders/Bar.vert", "Shaders/Bar.frag");
        private float _barSize;
        private Panel _inPanel;
        private Func<float> _max;

        private Vector2 _position;
        private bool _showText = true;
        private bool _builded;
        private string _optionalText;
        private Func<float> _value;
        public Vector4 BackgroundColor = new Vector4(0.1529f, 0.1529f, 0.1529f, 1);
        public bool CurvedBorders;
        private DrawOrder _order;
        public bool ShowBar = true;
        private readonly Vector2 _targetResolution = new Vector2(1024, 578);
        private RenderableText _barText;
        private readonly Vector4 _uniformColor;
        public bool AlignLeft { get; set; }
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
            if(Color != default(Vector4)) _uniformColor = Color;
            this.Initialize(Position, Scale, Value, Max, InPanel, null, Order, CurvedBorders);
        }

        public Bar(Vector2 Position, Vector2 Scale, string Text, Func<float> Value, Func<float> Max, Vector4 Color, Panel InPanel,
            DrawOrder Order = DrawOrder.Before)
        {
            _uniformColor = Color;
            this.Initialize(Position, Scale, Value, Max, InPanel, Text, Order);
        }
        
        public Bar(Vector2 Position, Vector2 Scale, string Text, Func<float> Value, Func<float> Max, Panel InPanel,
            DrawOrder Order = DrawOrder.Before)
        {
            this.Initialize(Position, Scale, Value, Max, InPanel, Text, Order);
        }

        public void Dispose()
        {
            _barText.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }

        public void Draw()
        {
            if (!Enabled)
                return;

            _barSize = Mathf.Clamp(Mathf.Lerp(_barSize, _value() / _max(), Time.IndependentDeltaTime * 8f), 0, 1);
            if(AlignLeft)
                _barText.Position = Position + _barText.Scale.X * Vector2.UnitX - Scale.X * Vector2.UnitX * .5f;

            if (UpdateTextRatio)
                _barText.Text = (int) _value() + " / " + (int) _max();
            Shader.Bind();          
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, CurvedBorders ? _barBlueprint : _rectangleBlueprint);

            Shader["Scale"] =
                Mathf.DivideVector(_targetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) +
                Mathf.DivideVector(_targetResolution * new Vector2(0.015f, 0.015f), new Vector2(GameSettings.Width, GameSettings.Height));
            Shader["Position"] = AdjustedPosition;
            Shader["Color"] = BackgroundColor;

            DrawManager.UIRenderer.DrawQuad();

            Shader["Scale"] = ShowBar
                    ? Mathf.DivideVector(_targetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) *
                      new Vector2(_barSize, 1)
                    : new Vector2(0, 0);
            Shader["Position"] = AdjustedPosition;
            Shader["Color"] =  _uniformColor != Vector4.Zero 
                ? _uniformColor : _barSize > 0.6f 
                ? Colors.FullHealthGreen : _barSize < 2.5f 
                ? Colors.LowHealthRed : throw new ArgumentOutOfRangeException("Health is out of range");

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.CullFace);
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
            Shader.Unbind();

            _barText.Draw();
        }

        public Vector2 Scale { get; set; }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                if (_barText != null)
                    _barText.Position = _position;
                this.Adjust();
            }
        }

        public string Text
        {
            get => _barText.Text;
            set => _barText.Text = value;
        }

        public Color TextColor
        {
            get => _barText.Color;
            set => _barText.Color = value;
        }
        
        public Font TextFont
        {
            get => _barText.TextFont;
            set => _barText.TextFont = value;
        }
        
        public void Enable()
        {
            Enabled = true;
            _barText.Enable();
        }

        public void Disable()
        {
            Enabled = false;
            _barText.Disable();
        }

        public bool Enabled { get; private set; }

        public void Adjust()
        {
            AdjustedPosition = GUITexture.Adjust(Position);
        }

        private void Initialize(Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel,
            string OptionalText, DrawOrder Order = DrawOrder.Before, bool CurvedBorders = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this._order = Order;
            this.CurvedBorders = CurvedBorders;
            _value = Value;
            _max = Max;
            _inPanel = InPanel;
            _optionalText = OptionalText;
            this.Build();
        }

        private void Build()
        {
            if (_optionalText == null)
            {
                _barText = new RenderableText(_value() + " / " + _max(), Position, Color.White, FontCache.GetBold(11));
            }
            else
            {
                _barText = new RenderableText(_optionalText, Position, Color.White, FontCache.GetBold(11));
                UpdateTextRatio = false;
            }
            DrawManager.UIRenderer.Add(this, this._order);
            _inPanel?.AddElement(_barText);
            
            if (_barBlueprint == 0)
            {
                _barBlueprint = uint.MaxValue;
                Executer.ExecuteOnMainThread(delegate
                {
                    _barBlueprint = Graphics2D.LoadFromAssets("Assets/Bar.png"); 
                    TextureRegistry.MarkStatic(_barBlueprint);
                });
            }
            if (_rectangleBlueprint == 0)
            {
                _rectangleBlueprint = uint.MaxValue;
                Executer.ExecuteOnMainThread(delegate
                {
                    _rectangleBlueprint = Graphics2D.ColorTexture(Colors.FromArgb(255, 29, 29, 29));
                    TextureRegistry.MarkStatic(_rectangleBlueprint);
                });
            }
        }
    }
}