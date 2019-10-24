using System.Drawing;
using System.Globalization;
using Hedra.Engine.Player.Inventory;
using Hedra.EntitySystem;
using Hedra.Localization;
using System.Numerics;

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
            Level.Text = $"{Translations.Get("level").ToUpperInvariant()} {Human.Level}";
            TopLeftText.Text = $"{Human.HealthRegen.ToString("0.00", CultureInfo.InvariantCulture)} {Translations.Get("hp_per_second")}";
            BottomLeftText.Text = $"{Human.AttackSpeed.ToString("0.00", CultureInfo.InvariantCulture)} {Translations.Get("attack_speed_label")}";
            TopRightText.Text = $"{Human.ManaRegen.ToString("0.00", CultureInfo.InvariantCulture)} {Translations.Get("mp_per_second")}";
            BottomRightText.Text = $"{Human.Speed.ToString("0.00", CultureInfo.InvariantCulture)} {Translations.Get("speed_label")}";
        }
    }
}
