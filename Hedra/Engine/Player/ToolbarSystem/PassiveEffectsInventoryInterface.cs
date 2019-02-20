using System.Linq;
using Hedra.Engine.Player.Inventory;
using OpenTK;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public sealed class PassiveEffectsInventoryInterface : InventoryArrayInterface
    {
        public PassiveEffectsInventoryInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing, CustomIcons)
        {
            for (var i = 0; i < Buttons.Length; ++i)
            {
                Buttons[i].Dispose();
            }
            for (var i = 0; i < ButtonsText.Length; ++i)
            {
                ButtonsText[i].Dispose();
            }

            for (var i = 0; i < Textures.Length; ++i)
            {
                Textures[i].TextureElement.Opacity = .75f;
                Textures[i].TextureElement.MaskId = InventoryArrayInterface.DefaultId;
            }
        }

        public override void UpdateView()
        {
            var sum = Textures.Sum(T => T.Scale.X);
            Position = Position.Y * Vector2.UnitY - Vector2.UnitX * sum * .25f;
        }
    }
}