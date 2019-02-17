using System.Drawing;
using System.Globalization;
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
            TopLeftText.Text = $"{Human.HealthRegen.ToString("0.00", CultureInfo.InvariantCulture)} HP/s";
            BottomLeftText.Text = $"{Human.AttackSpeed.ToString("0.00", CultureInfo.InvariantCulture)} AS";
            TopRightText.Text = $"{Human.ManaRegen.ToString("0.00", CultureInfo.InvariantCulture)} MP/s";
            BottomRightText.Text = $"{Human.Speed.ToString("0.00", CultureInfo.InvariantCulture)} S";
        }
    }
}
