using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    public abstract class HeavyMeleeWeapon : MeleeWeapon
    {
        protected override string AttackStanceName => "Assets/Chr/WarriorSmash-Stance.dae";
        protected override float PrimarySpeed => 1.15f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorSlash-Right.dae",
            "Assets/Chr/WarriorSlash-Left.dae"
        };
        protected override float SecondarySpeed => 1.25f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorSlash-Front.dae"
        };

        protected HeavyMeleeWeapon(VertexData Contents) : base(Contents)
        {
        }

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation, Options.IgnoreEntities);
        }
        
        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!base.MeetsRequirements()) return;

            base.Attack1(Human, Options);

            TaskScheduler.After(.25f, () => Trail.Emit = true);
        }

        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!base.MeetsRequirements()) return;

            base.Attack2(Human, Options);

            TaskScheduler.After(.2f, () => Trail.Emit = true);
        }
    }
}