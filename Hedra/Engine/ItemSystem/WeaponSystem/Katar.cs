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
	public class Katar : MeleeWeapon
    {
	    protected override string AttackStanceName => "Assets/Chr/RogueBlade-Stance.dae";
	    protected override string[] PrimaryAnimationsNames => new []
	    {
		    "Assets/Chr/RogueBladeLeftAttack.dae", "Assets/Chr/RogueBladeRightAttack.dae"
	    };
	    protected override string[] SecondaryAnimationsNames => new [] { "Assets/Chr/RogueBladeDoubleAttack.dae" };
	    protected override float PrimarySpeed => 1.25f;
	    protected override float SecondarySpeed => 2.0f;
	    
        private readonly ObjectMesh _secondBlade;
		
		public Katar(VertexData Contents) : base(Contents)
		{
			var baseMesh = Contents.Clone();
		    baseMesh.Scale(Vector3.One * 1.75f);
            this._secondBlade = ObjectMesh.FromVertexData(baseMesh);
			this.RegisterWeapon(_secondBlade, baseMesh);
		}
	    
	    protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    if(AttackEventType.Mid != Type) return;
		    Owner.Attack(Owner.DamageEquation * .85f);
	    }
		
	    protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    if(Type != AttackEventType.End) return;
		    Owner.Attack(Owner.DamageEquation * 1.10f, delegate(Entity Mob)
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

		    base.SetToDefault(this.MainMesh);
		    base.SetToDefault(this._secondBlade);

            if (Sheathed){
                Matrix4 Mat4 = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
			
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.TargetRotation = new Vector3(55 + 180, 0, 0);
				
				this._secondBlade.Position = Owner.Model.Position;
				this._secondBlade.BeforeLocalRotation = Vector3.UnitX * 1.0f - Vector3.UnitY * 2f;
				this._secondBlade.TransformationMatrix = Mat4;
				this._secondBlade.TargetRotation = new Vector3(-55 + 180,180,0);

			}
			
			if(base.InAttackStance || Owner.WasAttacking)
            {

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);
				
				this.MainMesh.TransformationMatrix = Mat4L;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180, 180, 0);
				this.MainMesh.BeforeLocalRotation = Vector3.Zero;
				
				Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Position + Owner.Model.RightWeaponPosition);
				
				this._secondBlade.TransformationMatrix = Mat4R;
				this._secondBlade.Position = Owner.Model.Position;
				this._secondBlade.TargetRotation = new Vector3(180,0,0);
				this._secondBlade.BeforeLocalRotation = Vector3.Zero;
			}
			
			if(PrimaryAttack || SecondaryAttack){

                Matrix4 Mat4L = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
				
				this.MainMesh.TransformationMatrix = Mat4L;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180, 180, 0f);
				this.MainMesh.BeforeLocalRotation = Vector3.Zero;
				
				Matrix4 Mat4R = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);
				
				this._secondBlade.TransformationMatrix = Mat4R;
				this._secondBlade.Position = Owner.Model.Position;
				this._secondBlade.TargetRotation = new Vector3(180,0,0);
				this._secondBlade.BeforeLocalRotation = Vector3.Zero;
			}
		    base.ApplyEffects(_secondBlade);
        }
    }
}