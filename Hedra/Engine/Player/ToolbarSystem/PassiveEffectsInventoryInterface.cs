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
        }

        public override void UpdateView()
        {
            
        }
    }
}