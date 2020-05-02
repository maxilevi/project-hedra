/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Sword : MeleeWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.SwordPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.SwordSecondaryAttack;
        
        private bool FrontSlash => PrimaryAnimationsIndex == 2;
        private readonly float _swordHeight;
        private readonly TrailRenderer _renderer;
        
        protected override string AttackStanceName => "Assets/Chr/WarriorSmash-Stance.dae";
        protected override float PrimarySpeed => 1.4f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorSlash-Left.dae",
            "Assets/Chr/WarriorSlash-Right.dae",
            "Assets/Chr/WarriorSlash-Front.dae"
        };
        protected override float SecondarySpeed => 2f;

        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorLunge.dae"
        };

        public Sword(VertexData Contents) : base(Contents)
        {
            _renderer = new TrailRenderer(
                () => this.Owner.Position + Owner.Orientation * 8f,
                new Vector4(1, 1, 1, .5f))
            {
                MaxLifetime = 3,
                Thickness = 1
            };
        }

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            Owner.AttackSurroundings(Owner.DamageEquation * (FrontSlash ? 1.25f : 1.0f) * Options.DamageModifier, Options.IgnoreEntities);
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            Owner.AttackSurroundings(Owner.DamageEquation * 1.15f * Options.DamageModifier, delegate(IEntity Mob)
            {
                if (Utils.Rng.Next(0, 5) == 1 && Options.DamageModifier > .5f)
                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);
            });
            if (Type == AttackEventType.End) _renderer.Emit = false;

        }

        public override void Update(IHumanoid Human)
        {
            _renderer.Update();
            base.Update(Human);
        }

        protected override int ParsePrimaryIndex(int AnimationIndex)
        {
            return AnimationIndex == 5 ? 2 : AnimationIndex & 1;
        }
        
        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if(!base.MeetsRequirements()) return;

            if (PrimaryAnimationsIndex == 5)
                PrimaryAnimationsIndex = 0;

            PrimaryAnimationsIndex++;

            base.BasePrimaryAttack(Human, Options);
            Trail.Emit = false;
            TaskScheduler.After(.2f, () => Trail.Emit = true);
        }

        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            Options.IdleMovespeed *= Options.Charge * 2.5f + .25f;
            Options.RunMovespeed = Options.Charge * 2.0f + 0.25f;
            base.Attack2(Human, Options);
            _renderer.Emit = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _renderer.Dispose();
        }
    }
}