using System;
using System.Collections.Generic;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using SixLabors.ImageSharp;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class RaiseSkeleton : SingleAnimationSkill<ISkilledAnimableEntity>
    {
        private readonly List<IEntity> _skeletons = new List<IEntity>();
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RaiseSkeletons.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/NecromancerRaiseSkeleton.dae");

        protected override float AnimationSpeed => 1.5f;
        protected override bool CanMoveWhileCasting => false;

        private int MaxMinions => 1 + (int)(4 * (Level / (float)MaxLevel));
        public override float IsAffectingModifier => Math.Min(_skeletons.Count, 1);
        protected override int MaxLevel => 20;
        public override float ManaCost => 140 - 70 * (Level / (float)MaxLevel);
        public override float MaxCooldown => 54 - 30 * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("raise_skeleton_desc");
        public override string DisplayName => Translations.Get("raise_skeleton_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("raise_skeleton_max_change", MaxMinions)
        };

        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            Spawn();
            InvokeStateUpdated();
        }

        private static IHumanoid SpawnMinion(IEntity Owner, IMinionMastery MasterySkill)
        {
            MasterySkill = MasterySkill ?? new DefaultMastery();
            var targetPosition = Owner.Position + Owner.Orientation * 16;
            Owner.Physics.StaticRaycast(targetPosition, out var hitPosition);
            var skeleton = NPCCreator.SpawnHumanoid(HumanType.Skeleton, hitPosition);
            skeleton.Position = hitPosition;
            skeleton.AddComponent(new MeleeMinionComponent(skeleton, Owner));
            skeleton.SetWeapon(ItemPool.Grab(CommonItems.UncommonSilverSword).Weapon);
            skeleton.BonusHealth = MasterySkill.HealthBonus;
            skeleton.AttackPower = MasterySkill.AttackPower;
            skeleton.AttackResistance = MasterySkill.AttackResistance;
            skeleton.Level = MasterySkill.SkeletonLevel;
            skeleton.SearchComponent<DamageComponent>()
                .Ignore(E => E == Owner || E.SearchComponent<DamageComponent>().HasIgnoreFor(Owner));
            skeleton.RemoveComponent(skeleton.SearchComponent<HealthBarComponent>());
            skeleton.AddComponent(new HealthBarComponent(skeleton, Translations.Get("skeleton_mastery_minion_name"),
                HealthBarType.Black, Color.FromRgb(40, 40, 40)));
            skeleton.AddComponent(new SkeletonEffectComponent(skeleton));
            skeleton.Physics.CollidesWithEntities = false;
            skeleton.RemoveComponentsOfType<DropComponent>();
            skeleton.AddComponent(new SelfDestructComponent(skeleton, () => Owner.IsDead));
            Owner.SearchComponent<DamageComponent>().Ignore(E => E == skeleton);
            return skeleton;
        }

        private void Spawn()
        {
            if (_skeletons.Count == MaxMinions) _skeletons[0].Damage(_skeletons[0].Health, null, out _, false);
            var skeleton = SpawnMinion(User, User.SearchSkill<SkeletonMastery>());
            skeleton.SearchComponent<DamageComponent>().OnDeadEvent += A => _skeletons.Remove(skeleton);
            _skeletons.Add(skeleton);
        }

        public class SkeletonEffectComponent : Component<IEntity>
        {
            public SkeletonEffectComponent(IEntity Parent) : base(Parent)
            {
            }

            public override void Update()
            {
                SkillUtils.DarkContinuousParticles(Parent);
            }
        }

        private class DefaultMastery : IMinionMastery
        {
            public float HealthBonus => 0;
            public int SkeletonLevel => 1;
            public float AttackResistance => .5f;
            public float AttackPower => .3f;
        }
    }
}