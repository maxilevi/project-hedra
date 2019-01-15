using System;
using System.Drawing;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class TextBillboard : BaseBillboard
    {
        private uint _id;
        private Vector2 _scale;
        private readonly Color _color;
        private readonly Font _font;
        
        public TextBillboard(float Lifetime, string Text, Color TextColor, Font TextFont, Vector3 Position) 
            : this(Lifetime, Text, TextColor, TextFont, () => Position)
        {
        }
      
        public TextBillboard(float Lifetime, string Text, Color TextColor, Font TextFont, Func<Vector3> Follow) 
            : base(Lifetime, Follow)
        {
            _color = TextColor;
            _font = TextFont;
            UpdateText(Text);
        }
        
        public void UpdateText(string Text)
        {
            _id = GUIText.BuildText(Text, _color, _font, out var measurements);
            _scale = measurements.ToRelativeSize();
        }

        protected override Vector2 Measurements => _scale;
        
        protected override uint Id => _id;
        
        /* We don't dispose fonts since they are from FontCache.cs */
        public override void Dispose()
        {
            base.Dispose();
            Renderer.DeleteTexture(Id);
        }
    }
}