using System.Globalization;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BloodExchange : BloodSkill
    {
        private float _fromHealth;
        private float _toHealth;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BloodExchange.png");

        protected override float MaxRadius => 48 + 48 * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 34 - 10 * (Level / (float)MaxLevel);
        public override float ManaCost => 80;
        public override string Description => Translations.Get("blood_exchange_desc");
        public override string DisplayName => Translations.Get("blood_exchange_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("blood_exchange_radius_skill", MaxRadius.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void SpawnParticle(IEntity Victim)
        {
            _toHealth = User.Health;
            _fromHealth = Victim.Health;
            LaunchParticle(User, User, Victim, OnReached);
            LaunchParticle(User, Victim, User, OnReached);
        }

        protected override void OnStart(IEntity Victim)
        {
            Victim.Model.Outline = true;
            Victim.Model.OutlineColor = Colors.Red;
            User.Model.Outline = true;
            User.Model.OutlineColor = Colors.Red;
        }

        private void OnReached(IEntity From, IEntity To)
        {
            To.Model.Outline = false;
            if (User == From) To.Health = _toHealth;
            if (User == To) To.Health = _fromHealth;
        }
    }
}