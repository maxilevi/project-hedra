using System;
using System.Drawing;
using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class DarkReaping : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/DarkReaping.png");
        
        protected override void Add()
        {
            Player.Kill += OnKill;
        }

        protected override void Remove()
        {
            Player.Kill -= OnKill;
        }

        private void OnKill(DeadEventArgs Args)
        {
            Player.Mana = Math.Min(Player.Mana + ManaPerKill, Player.MaxMana);
            Player.ShowText($"+{(int) ManaPerKill} MP", Color.CornflowerBlue, 12, 3);
        }

        protected override int MaxLevel => 16;
        private float ManaPerKill => 64 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("dark_reaping_desc");
        public override string DisplayName => Translations.Get("dark_reaping_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("dark_reaping_mana_change", ManaPerKill.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}