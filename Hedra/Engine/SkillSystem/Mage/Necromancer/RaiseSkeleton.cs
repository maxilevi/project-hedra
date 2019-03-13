using System;
using System.Drawing;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class RaiseSkeleton : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RaiseSkeletons.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerRaiseSkeleton.dae");
        protected override bool CanMoveWhileCasting => false;
        private int _currentMinions;
        
        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            SpawnMinion();
            InvokeStateUpdated();
        }

        private void SpawnMinion()
        {
            var skeleton = World.WorldBuilding.SpawnHumanoid(HumanType.Skeleton, Player.Position + Player.Orientation * 16);
            skeleton.AddComponent(new WarriorMinionComponent(skeleton, Player));
            skeleton.SetWeapon(ItemPool.Grab(CommonItems.UncommonSilverSword).Weapon);
            var masterySkill = (SkeletonMastery) Player.Toolbar.Skills.First(S => S.GetType() == typeof(SkeletonMastery));
            skeleton.AddonHealth = masterySkill.HealthBonus;
            skeleton.AttackPower = masterySkill.AttackPower;
            skeleton.AttackResistance = masterySkill.AttackResistance;
            skeleton.Level = masterySkill.SkeletonLevel;
            skeleton.SearchComponent<DamageComponent>().Ignore(E => E == Player || E.SearchComponent<DamageComponent>().HasIgnoreFor(Player));
            skeleton.RemoveComponent(skeleton.SearchComponent<HealthBarComponent>());
            skeleton.AddComponent(new HealthBarComponent(skeleton, Translations.Get("skeleton_mastery_minion_name"), HealthBarType.Black, Color.FromArgb(255, 40, 40, 40)));
            skeleton.SearchComponent<DamageComponent>().OnDeadEvent += A => _currentMinions--;
            skeleton.AddComponent(new SkeletonEffectComponent(skeleton));
            skeleton.RemoveComponent(skeleton.SearchComponent<DropComponent>());
            _currentMinions++;
        }
        
        private class SkeletonEffectComponent : Component<IHumanoid>
        {
            public SkeletonEffectComponent(IHumanoid Parent) : base(Parent)
            {
            }

            public override void Update()
            {
                SkillUtils.DarkContinuousParticles(Parent);
            }
        }

        private int MaxMinions => 1 + (int) (4 * (Level / (float) MaxLevel));
        public override float IsAffectingModifier => Math.Min(_currentMinions, 1);
        protected override int MaxLevel => 20;
        protected override bool ShouldDisable => _currentMinions >= MaxMinions;
        public override float ManaCost => 140 - 70 * (Level / (float) MaxLevel);
        public override float MaxCooldown => 54 - 30 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("raise_skeleton_desc");
        public override string DisplayName => Translations.Get("raise_skeleton_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("raise_skeleton_max_change", MaxMinions)
        };
    }
}