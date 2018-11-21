/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class DoubleBlades : RogueWeapon
    {     
        public DoubleBlades(VertexData Contents) : base(Contents)
        {
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.5f * Options.DamageModifier, delegate (Entity Mob)
            {

                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .75f)
                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .5f)
                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
                        Owner.DamageEquation * 2f));
            });
        }
        
        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            if (Sheathed)
            {
                this.MainMesh.BeforeLocalRotation = new Vector3(.4f, 0.75f, -1.2f);
                this.MainMesh.TargetRotation = new Vector3(0, 90, 135);
                
                this.SecondBlade.BeforeLocalRotation = new Vector3(-1.0f, 0.75f, -1.2f);
                this.SecondBlade.TargetRotation = new Vector3(0, 270, 225);
            }

            if (base.InAttackStance || Owner.WasAttacking)
            {
                this.MainMesh.TargetRotation = new Vector3(180, 0, -45);
                this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.35f;

                this.SecondBlade.TargetRotation = new Vector3(180, 90, 45);
                this.SecondBlade.BeforeLocalRotation = Vector3.UnitY * -0.35f;
            }

            if (PrimaryAttack || SecondaryAttack)
            {
                this.MainMesh.TargetRotation = PrimaryAttack ? new Vector3(180, 0, 0) : new Vector3(180, 0, 0f);
                this.MainMesh.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;

                this.SecondBlade.TargetRotation = PrimaryAttack ? new Vector3(180, 180, 0) : new Vector3(180, 180, 0f);
                this.SecondBlade.BeforeLocalRotation = PrimaryAttack ? -Vector3.UnitY * .35f : Vector3.UnitY * -0.7f;
            }
        }
    }
}