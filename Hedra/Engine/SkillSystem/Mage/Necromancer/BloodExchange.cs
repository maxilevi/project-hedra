using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BloodExchange : BloodSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BloodExchange.png");
        private float _fromHealth;
        private float _toHealth;
        
        protected override void SpawnParticle(IEntity Victim)
        {
            _toHealth = Player.Health;
            _fromHealth = Victim.Health;
            LaunchParticle(Player, Victim, OnReached);
            LaunchParticle(Victim, Player, OnReached);
        }

        protected override void OnStart(IEntity Victim)
        {
            Victim.Model.Outline = true;
            Victim.Model.OutlineColor = Colors.Red;
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Colors.Red;
        }

        private void OnReached(IEntity From, IEntity To)
        {
            To.Model.Outline = false;
            if (Player == From) To.Health = _toHealth;
            if (Player == To) To.Health = _fromHealth;
        }

        protected override float MaxRadius => 48 + 48 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 34 - 10 * (Level / (float) MaxLevel);
        public override float ManaCost => 80;
        public override string Description => Translations.Get("blood_exchange_desc");
        public override string DisplayName => Translations.Get("blood_exchange_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("blood_exchange_radius_skill", MaxRadius.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}