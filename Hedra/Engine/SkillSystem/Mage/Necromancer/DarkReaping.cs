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
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/DarkReaping.png");
        
        protected override void Add()
        {
            User.Kill += OnKill;
        }

        protected override void Remove()
        {
            User.Kill -= OnKill;
        }

        private void OnKill(DeadEventArgs Args)
        {
            User.Mana = Math.Min(User.Mana + ManaPerKill, User.MaxMana);
            User.ShowText($"+{(int) ManaPerKill} MP", Color.CornflowerBlue, 12, 3);
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