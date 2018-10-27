using System.Drawing;
using Hedra.Engine.Player.Inventory;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityInventoryBackground : InventoryBackground
    {
        public AbilityInventoryBackground(Vector2 Position) : base(Position)
        {
            TopLeftText.TextColor = Color.IndianRed;
            TopRightText.TextColor = Color.CornflowerBlue;
            BottomLeftText.TextColor = Color.Yellow;
            BottomRightText.TextColor = Color.LightBlue;
        }

        public override void UpdateView(IHumanoid Human)
        {
            base.UpdateView(Human);
            TopLeftText.Text = $"{Human.HealthRegen:0.00} HP/s";
            BottomLeftText.Text = $"{Human.AttackSpeed:0.00} AS";
            TopRightText.Text = $"{Human.ManaRegen:0.00} MP/s";
            BottomRightText.Text = $"{Human.Speed:0.00} S";
        }
    }
}
