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
    public class DoubleBlades : RogueWeapon
    {
        public DoubleBlades(VertexData Contents) : base(Contents)
        {
        }

        public override uint PrimaryAttackIcon => WeaponIcons.DoubleBladesPrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.DoubleBladesSecondaryAttack;

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
            MainMesh.BeforeRotation = new Vector3(.6f, 0.75f, -1.2f);
            MainMesh.LocalRotation = new Vector3(0, 90, 135);

            SecondBlade.BeforeRotation = new Vector3(-0.8f, 0.75f, -1.2f);
            SecondBlade.LocalRotation = new Vector3(0, 270, 225);
        }

        protected override void OnAttackStance()
        {
            base.OnAttackStance();
            MainMesh.LocalRotation = new Vector3(180, 0, -45);
            MainMesh.BeforeRotation = Vector3.UnitY * -0.35f;

            SecondBlade.LocalRotation = new Vector3(180, 90, 45);
            SecondBlade.BeforeRotation = Vector3.UnitY * -0.35f;
        }

        protected override void OnAttack()
        {
            base.OnAttack();
            MainMesh.LocalRotation = PrimaryAttack ? new Vector3(180, 0, 0) : new Vector3(180, 0, 0f);
            MainMesh.BeforeRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;

            SecondBlade.LocalRotation = PrimaryAttack ? new Vector3(180, 180, 0) : new Vector3(180, 180, 0f);
            SecondBlade.BeforeRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
        }
    }
}