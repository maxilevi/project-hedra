/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 06/05/2016
 * Time: 12:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of Hands.
    /// </summary>
    public class Hands : MeleeWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.DefaultAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.DefaultAttack;
        protected override bool ShouldPlaySound => false;
        protected override string AttackStanceName => "Assets/Chr/WarriorPunch-Stance.dae";
        protected override float PrimarySpeed => 1.35f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorLeftPunch.dae",
            "Assets/Chr/WarriorRightPunch.dae"
        };
        protected override float SecondarySpeed => 1.5f;

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/WarriorHeadbutt.dae"
        };

        public Hands() : base(null)
        {
            MainWeaponSize = new Vector3(MainWeaponSize.X, 1f, MainWeaponSize.Z);
        }

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.5f);
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 3.0f * Options.Charge, delegate(IEntity Mob)
            {
                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .4f)
                    Mob.KnockForSeconds(2.5f + Utils.Rng.NextFloat() * 2f);
            });
        }
    }
}
