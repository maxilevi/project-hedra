/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Components.Effects;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    /// <summary>
    ///     Description of TwoHandedSword.
    /// </summary>
    public class Claw : RogueWeapon
    {
        public Claw(VertexData Contents) : base(Contents)
        {
        }

        public override uint PrimaryAttackIcon => WeaponIcons.ClawPrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.ClawSecondaryAttack;

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation * Options.DamageModifier);
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (Type != AttackEventType.Mid) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 2.0f * Options.DamageModifier, Options.IgnoreEntities,
                delegate(IEntity Mob)
                {
                    if (Utils.Rng.Next(0, 2) == 1 && Options.Charge > .5f)
                        Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                    if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .25f)
                        Mob.AddComponent(new BleedingComponent(Mob, Owner, 4f,
                            Owner.DamageEquation * 2f));
                });
        }

        protected override void OnSheathed()
        {
            base.OnSheathed();
            MainMesh.BeforeRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
            MainMesh.LocalRotation = new Vector3(55 + 180, 0, 0);

            SecondBlade.BeforeRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
            SecondBlade.LocalRotation = new Vector3(55 + 180, 180, 0);
        }

        protected override void OnAttackStance()
        {
            base.OnAttackStance();
            MainMesh.LocalRotation = new Vector3(180, 180, 0);
            MainMesh.BeforeRotation = Vector3.UnitY * -0.35f;

            SecondBlade.LocalRotation = new Vector3(180, 0, 0);
            SecondBlade.BeforeRotation = Vector3.UnitY * -0.35f;
        }

        protected override void OnAttack()
        {
            base.OnAttack();
            MainMesh.LocalRotation = new Vector3(180, 180, 0f);
            MainMesh.BeforeRotation = Vector3.UnitY * -0.7f;

            SecondBlade.LocalRotation = new Vector3(180, 0, 0);
            SecondBlade.BeforeRotation = Vector3.UnitY * -0.7f;
        }
    }
}