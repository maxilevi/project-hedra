using System.Drawing;
using Hedra.Engine.Player.Inventory;
using Hedra.EntitySystem;
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
            Name.Text = Human.Name;
            Level.Text = $"LEVEL {Human.Level}";
            TopLeftText.Text = $"{Human.HealthRegen:0.00} HP/s";
            BottomLeftText.Text = $"{Human.AttackSpeed:0.00} AS";
            TopRightText.Text = $"{Human.ManaRegen:0.00} MP/s";
            BottomRightText.Text = $"{Human.Speed:0.00} S";
        }
    }
}
