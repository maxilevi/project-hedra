/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Katar : RogueWeapon
    {

        public Katar(VertexData Contents) : base(Contents)
        {
        }
        

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.75f, delegate(Entity Mob)
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
                this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
                this.MainMesh.TargetRotation = new Vector3(55 + 180, 0, 0);

                this.SecondBlade.BeforeLocalRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
                this.SecondBlade.TargetRotation = new Vector3(-55 + 180,180,0);
            }
            
            if(base.InAttackStance || Owner.WasAttacking)
            {
                this.MainMesh.TargetRotation = new Vector3(180, 180, 0);
                this.MainMesh.BeforeLocalRotation = Vector3.Zero;

                this.SecondBlade.TargetRotation = new Vector3(180,0,0);
                this.SecondBlade.BeforeLocalRotation = Vector3.Zero;
            }
            
            if(PrimaryAttack || SecondaryAttack)
            {
                this.MainMesh.TargetRotation = new Vector3(180, 180, 0f);
                this.MainMesh.BeforeLocalRotation = Vector3.Zero;

                this.SecondBlade.TargetRotation = new Vector3(180,0,0);
                this.SecondBlade.BeforeLocalRotation = Vector3.Zero;
            }
        }
    }
}