using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SpikeTrap : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SpikeTrap.png");
        protected override int MaxLevel => 15;
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherLayTrap.dae");

        protected override void OnAnimationMid()
        {
            var trap = new BearTrap(Player, Player.Position + Player.Orientation * 2, Duration, Damage, Stun);
            World.AddWorldObject(trap);
        }

        private float Duration => 120; // 2 mins
        private float Damage => 50;
        private bool Stun => Utils.Rng.Next(0, 10) == 1;
        public override float MaxCooldown => 4;//40;
        public override string Description => Translations.Get("spike_trap_desc");
        public override string DisplayName => Translations.Get("spike_trap");
        public override string[] Attributes => new[]
        {
            "",
            "",
            ""
        };
    }
}