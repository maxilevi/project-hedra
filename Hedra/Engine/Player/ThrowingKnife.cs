/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Item;
using System.Collections;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Bow.
	/// </summary>
	public class ThrowingKnife : Weapon
	{
	    public override bool IsMelee { get; protected set; } = false;
        public override Vector3 SheathedPosition {get{ return new Vector3(-.6f,0.5f,-0.8f); } }
		public override Vector3 SheathedRotation {get{ return new Vector3(-5,90,-125); } }
		private bool Attack1Mode, Attack2Mode;
		private EntityMesh Quiver, KnifeLeft, KnifeRight;
		private VertexData KnifeData;
		
		public ThrowingKnife(VertexData Contents, InventoryItem Item) : base(Contents){
			base.Mesh = EntityMesh.FromVertexData(new VertexData());
			
			KnifeRight = EntityMesh.FromVertexData(Contents);
			KnifeLeft = EntityMesh.FromVertexData(Contents);

			KnifeRight.TargetRotation = new Vector3(0, 45, 180);//90 
			KnifeLeft.TargetRotation = new Vector3(0, -45, 180);//new Vector3(-90, 0, 180);
				
			VertexData QuiverData = AssetManager.PlyLoader("Assets/Items/KnifeQuiver.ply", Vector3.One * new Vector3(2.4f, 2.6f, 2.4f) );
			QuiverData.Color(AssetManager.ColorCode1, ItemPool.MaterialColor(Item.Info.MaterialType) );
			Quiver = EntityMesh.FromVertexData(QuiverData);
			Quiver.TargetPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
			Quiver.LocalRotationPoint = new Vector3(0,0,Quiver.TargetPosition.Z);
			Quiver.TargetRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);
			KnifeData = Contents;
		}
		
		public void ShootKnife(Humanoid Human, Vector3 Direction, int BleedChance = -1){
			Projectile KnifeProj = new Projectile(KnifeData.Clone(), 
			                                      ((LeftHand) ? (Model.LeftHandPosition) :
			                                       (Model.RightHandPosition) )
			                                      + Model.Human.Orientation * 2 +
			                                      ( (Human is LocalPlayer ) ? Vector3.UnitY * 2f : Vector3.Zero), Direction, Human);
			
			if(LeftHand)
				KnifeProj.Rotation = new Vector3(KnifeProj.Rotation.X, KnifeProj.Rotation.Y, KnifeProj.Rotation.Z + 45*(Direction.Y-.2f)*3);
			else
				KnifeProj.Rotation = new Vector3(KnifeProj.Rotation.X, KnifeProj.Rotation.Y, KnifeProj.Rotation.Z + 45*(Direction.Y-.2f)*3+90);
			KnifeProj.Speed = 8.0f;
			KnifeProj.Lifetime = 5f;
			KnifeProj.RotateOnX = true;
			KnifeProj.HitEventHandler += delegate(Projectile Sender, Entity Hit) { 
				float Exp;
				Hit.Damage(Human.DamageEquation * 0.3f, Human, out Exp, true);
				Human.XP += Exp;
				if(BleedChance != -1){
					if(Utils.Rng.Next(0, BleedChance) == 0)
						Hit.AddComponent(new BleedingComponent(Hit, Human, 4f, 30f));
				}
			};
			Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Human.Position, false, 1f, .8f);
		}
		
		public IEnumerator ShootDoubleKnife( object[] Params ){
			Humanoid Human = Params[0] as Humanoid;
			Vector3 Direction = Vector3.Zero;
			if(Human is LocalPlayer){
				float Distance = (Human as LocalPlayer).View.TargetDistance;
				Direction = (Human as LocalPlayer).View.CrossDirection;
			}else{
				Direction = Human.Orientation;
			}
			this.ShootKnife(Human, Direction, 8);
			for(int i = 0; i < 8; i++)
				yield return null;
			this.ShootKnife(Human, Direction, 8);
			Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Human.Position, false, 1f, .8f);
		}
		
		private bool LeftHand = false;
		public override void Attack1(HumanModel Model)
		{
			if(Model.Human.IsAttacking && !ContinousAttack || Model.Human.Knocked)
				return;
			
			Init(Model);

			Model.LeftWeapon.Mesh.TargetRotation = Vector3.Zero;
			Model.LeftWeapon.Mesh.Rotation = Vector3.Zero;
			Model.LeftWeapon.Mesh.LocalRotation = Vector3.Zero;

			Model.Human.Movement.TargetSpeed = 2.5f;
			if(LeftHand)
				LeftHand = false;
			else
				LeftHand = true;
			Model.Human.IsAttacking = true;
			Model.Human.WasAttacking = true;
			Attack1Mode = true;
		}
		
		public override void Attack2(HumanModel Model)
		{
			if(Model.Human.IsAttacking){
				if(Model.Human.IsAttacking && !Attack1Mode || Model.Human.Knocked)
					return;
			}
			Init(Model);

			Model.LeftWeapon.Mesh.TargetRotation = Vector3.Zero;
			Model.LeftWeapon.Mesh.Rotation = Vector3.Zero;
			Model.LeftWeapon.Mesh.LocalRotation = Vector3.Zero;

			Model.Human.Movement.TargetSpeed = 2.5f;
			//if(!Model.Human.IsMoving){
			//	Model.Human.Movement.MoveFeet = true;
			//	Model.Human.Movement.MoveCount = 1.25f;
			//}
			Model.Human.IsAttacking = true;
			Model.Human.WasAttacking = true;
			Attack2Mode = true;
		}
	}
}
