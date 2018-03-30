using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Player.Inventory;
using OpenTK;

namespace Hedra.Engine.Player.AbilityBar
{
    public class AbilityBarInventoryInterface : InventoryArrayInterface
    {
        public AbilityBarInventoryInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing, CustomIcons)
        {
        }
    }
}
