using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class AbilityBagInventoryInterface : InventoryArrayInterface
    {
        private readonly Panel _panel;

        public AbilityBagInventoryInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing, CustomIcons)
        {
            _panel = new Panel();
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                this.Buttons[i].Texture.IdPointer = null;
                this.Buttons[i].Texture.TextureId = GUIRenderer.TransparentTexture;
                this.Buttons[i].Scale *= 1.25f;

                this.Array[i] = new Item
                {
                    Model = new VertexData(),
                };
                this.Array[i].SetAttribute("ImageId", 0);
                this.Textures[i].Scale = Vector2.Zero;
            }
            _panel.Disable();
        }

        public sealed override void UpdateView()
        {
            var sum = Buttons.Sum(B => B.Texture.Scale.X);
            Position = Position.Y * Vector2.UnitY - Vector2.UnitX * sum * .25f;
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (value) _panel.Enable();
                else _panel.Disable();
            }
        }
    }
}
