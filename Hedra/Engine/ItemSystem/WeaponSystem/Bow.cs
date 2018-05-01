/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	public delegate void OnModifyArrowEvent(Projectile Arrow);
		
	public class Bow : Weapon
	{	
		public override Vector3 SheathedPosition => new Vector3(1.8f,-1.0f,0.65f);
	    public override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
	    protected override float WeaponCooldown => .15f;
        public override bool IsMelee { get; protected set; } = false;
        private readonly ObjectMesh Quiver;
		private readonly ObjectMesh[] Arrow = new ObjectMesh[1];//hacky stuff! so it's not affected by global enablers
		private readonly VertexData ArrowData;
		public OnModifyArrowEvent BowModifiers;
		
		public Bow(VertexData Contents) : base(Contents){
			Arrow[0] = ObjectMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 1.5f, Vector3.UnitX * .35f, Vector3.Zero));
			Quiver = ObjectMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Quiver.ply", Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f));
			Quiver.TargetPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
			Quiver.LocalRotationPoint = new Vector3(0, 0, Quiver.TargetPosition.Z);
			Quiver.TargetRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);
			ArrowData = AssetManager.PlyLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 2.0f, Vector3.Zero, new Vector3(-90,0,90) * Mathf.Radian);

            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShootStance.dae");

            PrimaryAnimations = new Animation[1];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShoot.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.15f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            var player = Owner as LocalPlayer;
		            Vector3 direction = player?.View.CrossDirection ?? Owner.Orientation;

                    this.ShootArrow(Owner, direction);
		        };
		    }

            SecondaryAnimations = new Animation[1];
		    SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/ArcherTripleShoot.dae");

		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[i].Loop = false;
		        SecondaryAnimations[i].Speed = 1.15f;
                SecondaryAnimations[i].OnAnimationMid += delegate
		        {
		            this.ShootTripleArrow(Owner);
		        };
		    }

		    base.ShouldPlaySound = false;
        }
		
		public override void Update(Humanoid Human)
		{
			base.Update(Human);

            base.SetToDefault(MainMesh);

			if(Sheathed){
                this.MainMesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
                this.MainMesh.Position = Owner.Model.Position;
			    this.MainMesh.LocalRotation = this.SheathedRotation;
                this.MainMesh.BeforeLocalRotation = this.SheathedPosition * this.Scale;
            }

            if (base.InAttackStance || Owner.IsAttacking || Owner.WasAttacking)
            {
				Matrix4 Mat4 = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);
					
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(90,25,180);
				this.MainMesh.BeforeLocalRotation = (Vector3.UnitZ * -0.7f - Vector3.UnitX * -.5f + Vector3.UnitY * .35f);				
			}
			
            base.SetToDefault(this.Quiver);

		    this.Quiver.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
		    this.Quiver.Position = Owner.Model.Position;
		    this.Quiver.BeforeLocalRotation = (-Vector3.UnitY * 1.5f - Vector3.UnitZ * 1.8f) * this.Scale;

            base.SetToDefault(this.Arrow[0]);

            Matrix4 ArrowMat4 = Owner.Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightHandPosition);
			
			this.Arrow[0].TransformationMatrix = ArrowMat4;
			this.Arrow[0].Position = Owner.Model.Position;
			this.Arrow[0].BeforeLocalRotation = Vector3.UnitZ * 0.5f;
			this.Arrow[0].Enabled = (base.InAttackStance || Owner.IsAttacking) && this.Quiver.Enabled;	
			
		}
		
		public Projectile AddModifiers(Projectile ArrowProj){
		    BowModifiers?.Invoke(ArrowProj);
		    return ArrowProj;
		}
		
		public Projectile ShootArrow(Humanoid Human, Vector3 Direction, int KnockChance = -1){
			var arrowProj = new Projectile(ArrowData.Clone(), Owner.Model.LeftHandPosition + Owner.Model.Human.Orientation * 2 +
			                                      ( (Human is LocalPlayer ) ? Vector3.UnitY * 0f : Vector3.Zero), Direction, Human);
			arrowProj.Rotation = new Vector3(arrowProj.Rotation.X, arrowProj.Rotation.Y, arrowProj.Rotation.Z + 45*(Direction.Y-.2f)*3);
			arrowProj.Speed = 6.0f;
			arrowProj.Lifetime = 5f;
			arrowProj.HitEventHandler += delegate(Projectile Sender, Entity Hit) { 
				float Exp;
				Hit.Damage(Human.DamageEquation * 0.75f, Human, out Exp, true);
				Human.XP += Exp;
				if(KnockChance != -1){
					if(Utils.Rng.Next(0, KnockChance) == 0)
						Hit.KnockForSeconds(3);
				}
			};
			SoundManager.PlaySound(SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
			arrowProj = this.AddModifiers(arrowProj);
			return arrowProj;
		}
		
		public void ShootTripleArrow(Humanoid Human){
		    var player = Human as LocalPlayer;
            Vector3 direction = player?.View.CrossDirection ?? Human.Orientation;

			this.ShootArrow(Human, (direction + Vector3.UnitX*.15f).NormalizedFast());
			this.ShootArrow(Human, direction);
			this.ShootArrow(Human, (direction - Vector3.UnitX*.15f).NormalizedFast());
			SoundManager.PlaySound(SoundType.BowSound, Human.Position, false, 1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
		}
	}
}
