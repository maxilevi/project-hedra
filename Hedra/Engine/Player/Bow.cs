/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;

namespace Hedra.Engine.Player
{
	public delegate void OnModifyArrowEvent(Projectile Arrow);
		
	public class Bow : Weapon
	{	
		public override Vector3 SheathedPosition => new Vector3(1.8f,-1.0f,0.0f);
	    public override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
	    public override bool IsMelee { get; protected set; } = false;
        private EntityMesh Quiver;
		private EntityMesh[] Arrow = new EntityMesh[1];//hacky stuff! so it's not affected by global enablers
		private VertexData ArrowData;
		public OnModifyArrowEvent BowModifiers;
		
		public Bow(VertexData Contents) : base(Contents){
			VertexData BaseMesh = Contents.Clone();
			BaseMesh.Scale(Vector3.One * 1.4f);
			base.Mesh = EntityMesh.FromVertexData(BaseMesh);

			Arrow[0] = EntityMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 1.5f, Vector3.UnitX * .35f, Vector3.Zero));
			Quiver = EntityMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Quiver.ply", Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f));
			Quiver.TargetPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
			Quiver.LocalRotationPoint = new Vector3(0, 0, Quiver.TargetPosition.Z);
			Quiver.TargetRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);
			ArrowData = AssetManager.PlyLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 1.5f, Vector3.Zero, new Vector3(-90,0,90) * Mathf.Radian);

            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShootStance.dae");

            PrimaryAnimations = new Animation[1];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShoot.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            var player = Model.Human as LocalPlayer;
		            Vector3 direction = player?.View.CrossDirection ?? Model.Human.Orientation;

                    this.ShootArrow(Model.Human, direction);
		        };
		    }

            SecondaryAnimations = new Animation[1];
		    SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/ArcherTripleShoot.dae");

		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[0].Loop = false;
		        SecondaryAnimations[0].OnAnimationMid += delegate
		        {
		            this.ShootTripleArrow(Model.Human);
		        };
		    }

		    base.ShouldPlaySound = false;
        }
		
		public override void Update(HumanModel Model)
		{
			base.Update(Model);

            base.SetToDefault(Mesh);

			if(Sheathed){
                this.Mesh.TransformationMatrix = Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.ChestPosition - Vector3.UnitY * .25f);
                this.Mesh.Position = Model.Position;
			    this.Mesh.LocalRotation = this.SheathedRotation;
                this.Mesh.BeforeLocalRotation = this.SheathedPosition * this.Scale;
            }

            if (base.InAttackStance || Model.Human.IsAttacking || Model.Human.WasAttacking)
            {
				Matrix4 Mat4 = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);
					
				this.Mesh.TransformationMatrix = Mat4;
				this.Mesh.Position = Model.Position;
				this.Mesh.TargetRotation = new Vector3(90,25,180);
				this.Mesh.BeforeLocalRotation = (Vector3.UnitZ * -0.7f - Vector3.UnitX * -.5f + Vector3.UnitY * .35f);				
			}
			
            base.SetToDefault(this.Quiver);

		    this.Quiver.TransformationMatrix = Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.ChestPosition);
		    this.Quiver.Position = Model.Position;
		    this.Quiver.BeforeLocalRotation = (-Vector3.UnitY * 1.5f - Vector3.UnitZ * 1.8f) * this.Scale;

            base.SetToDefault(this.Arrow[0]);

            Matrix4 ArrowMat4 = Model.RightHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.RightHandPosition);
			
			this.Arrow[0].TransformationMatrix = ArrowMat4;
			this.Arrow[0].Position = Model.Position;
			this.Arrow[0].BeforeLocalRotation = Vector3.UnitZ * 0.5f;
			this.Arrow[0].Enabled = (base.InAttackStance || Model.Human.IsAttacking) && this.Quiver.Enabled;	
			
		}
		
		public Projectile AddModifiers(Projectile ArrowProj){
		    BowModifiers?.Invoke(ArrowProj);
		    return ArrowProj;
		}
		
		public Projectile ShootArrow(Humanoid Human, Vector3 Direction, int KnockChance = -1){
			Init(Human.Model);
			var arrowProj = new Projectile(ArrowData.Clone(), Model.LeftHandPosition + Model.Human.Orientation * 2 +
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
