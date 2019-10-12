/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Katar : RogueWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.KatarPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.KatarSecondaryAttack;

        public Katar(VertexData Contents) : base(Contents)
        {
        }
        

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.75f, Options.IgnoreEntities, delegate(IEntity Mob)
            {

                if (Utils.Rng.Next(0, 2) == 1 && Options.Charge > .5f)
                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

                if (Utils.Rng.Next(0, 3) == 1 && Options.Charge > .25f)
                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
                        Owner.DamageEquation * 2f));
            });
        }
        
        protected override void OnSheathed()
        {
            base.OnSheathed();
            MainMesh.BeforeRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
            MainMesh.LocalRotation = new Vector3(55 + 180, 0, 0);

            SecondBlade.BeforeRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
            SecondBlade.LocalRotation = new Vector3(55 + 180, 180,0);
        }
            
        protected override void OnAttackStance()
        {
            base.OnAttackStance();
            MainMesh.LocalRotation = new Vector3(180, 180, 0);
            MainMesh.BeforeRotation = Vector3.Zero;

            SecondBlade.LocalRotation = new Vector3(180,0,0);
            SecondBlade.BeforeRotation = Vector3.Zero;
        }
            
        protected override void OnAttack()
        {
            base.OnAttack();
            MainMesh.LocalRotation = new Vector3(180, 180, 0f);
            MainMesh.BeforeRotation = Vector3.Zero;
        
            SecondBlade.LocalRotation = new Vector3(180,0,0);
            SecondBlade.BeforeRotation = Vector3.Zero;
        }
    }
}