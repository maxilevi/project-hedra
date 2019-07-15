using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Cutthroat : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Cutthroat.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueCutthroat.dae");
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => 1.25f;
        private readonly Timer _updateStatusTimer = new Timer(.1f);
        private bool _canDo;

        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
            var isBehind = SkillUtils.IsBehindAny(User, out var entity);
            if (isBehind && entity is IHumanoid)
            {
                entity.Damage(entity.MaxHealth * DamagePercentage, User, out var xp);
                User.XP += xp;
                if (entity.IsDead)
                    User.Toolbar.ResetCooldowns();
            }
        }

        public override void Update()
        {
            base.Update();
            if (Level > 0 && _updateStatusTimer.Tick())
            {
                var isBehind = SkillUtils.IsBehindAny(User, out var entity);
                _canDo = isBehind && entity is IHumanoid;
            }
        }

        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && SkillUtils.IsBehindAny(User, out var entity) && entity is IHumanoid;
        }

        protected override bool ShouldDisable => !_canDo;
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 55 - 10f * (Level / (float)MaxLevel);
        public override float ManaCost => 40;
        private float DamagePercentage => .1f + .15f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("cutthroat_desc");
        public override string DisplayName => Translations.Get("cutthroat_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("cutthroat_damage_change", (int)(DamagePercentage * 100)),
            Translations.Get("cutthroat_bonus")
        };
    }
}