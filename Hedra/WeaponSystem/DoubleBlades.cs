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
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class DoubleBlades : RogueWeapon
    {     
        public override uint PrimaryAttackIcon => WeaponIcons.DoubleBladesPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.DoubleBladesSecondaryAttack;
        
        public DoubleBlades(VertexData Contents) : base(Contents)
        {
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 2.0f * Options.DamageModifier, delegate (IEntity Mob)
            {

                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .75f)
                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .5f)
                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
                        Owner.DamageEquation * 2f));
            });
        }
        
        
        protected override void OnSheathed()
        {
            base.OnSheathed();
            MainMesh.BeforeLocalRotation = new Vector3(.4f, 0.75f, -1.2f);
            MainMesh.TargetRotation = new Vector3(0, 90, 135);
                
            SecondBlade.BeforeLocalRotation = new Vector3(-1.0f, 0.75f, -1.2f);
            SecondBlade.TargetRotation = new Vector3(0, 270, 225);
        }

        protected override void OnAttackStance()
        {
            base.OnAttackStance();
            MainMesh.TargetRotation = new Vector3(180, 0, -45);
            MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

            SecondBlade.TargetRotation = new Vector3(180, 90, 45);
            SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
        }

        protected override void OnAttack()
        {
            base.OnAttack();
            MainMesh.TargetRotation = PrimaryAttack ? new Vector3(180, 0, 0) : new Vector3(180, 0, 0f);
            MainMesh.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
        
            SecondBlade.TargetRotation = PrimaryAttack ? new Vector3(180, 180, 0) : new Vector3(180, 180, 0f);
            SecondBlade.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
        }
    }
}