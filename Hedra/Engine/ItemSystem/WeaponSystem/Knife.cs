/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	/// <summary>
	/// Description of TwoHandedSword.
	/// </summary>
	public class Knife : Weapon
	{
	    public override bool IsMelee { get; protected set; } = true;
        private Vector3 PreviousPosition = Vector3.Zero;
		private ObjectMesh KnifeSheath;
		
		public Knife(VertexData Contents) : base(Contents)
		{
			
			VertexData SheathData = AssetManager.PlyLoader("Assets/Items/KnifeSheath.ply", Vector3.One);
			SheathData.Transform( Matrix4.CreateRotationY(180f * Mathf.Radian) );
			SheathData.Scale( new Vector3(3.75f, 1.8f, 2f));
			KnifeSheath = ObjectMesh.FromVertexData(SheathData);
			
			AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShootStance.dae");

		    PrimaryAnimations = new Animation[2];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.0f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {

		            Owner.Attack(Owner.DamageEquation * 0.7f, delegate(Entity Mob)
		            {
		                if (Utils.Rng.Next(0, 5) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 3f,
		                        Owner.DamageEquation * 2.0f));
		            });
		        };

		        PrimaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Trail.Emit = false;
		        };
            }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorLunge.dae");

		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[i].Loop = false;
		        SecondaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * 0.8f, delegate(Entity Mob)
		            {
		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
		                        Owner.DamageEquation * 2.0f));
		            });
		        };
		    }
		}
		
		public override void Update(Humanoid Human)
		{
			base.Update(Human);
		    base.SetToDefault(MainMesh);

            if (Sheathed){

                Matrix4 Mat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
			
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.TargetRotation = new Vector3(180, 0, 0);
			}
			
			if(base.InAttackStance || Owner.WasAttacking){

                Matrix4 Mat4 = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(270,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.1f - Vector3.UnitZ * .2f;
				
			}
			
			if(PrimaryAttack){

                Matrix4 Mat4 = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
			}
			
			if(SecondaryAttack){

				Matrix4 Mat4 = Owner.Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftHandPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				
				if(PreviousPosition != Owner.Model.Human.BlockPosition && Owner.Model.Human.IsGrounded){
				    Chunk underChunk = World.GetChunkAt(Owner.Model.Position);
                    World.WorldParticles.VariateUniformly = true;
				    World.WorldParticles.Color = World.GetHighestBlockAt( (int) Owner.Model.Human.Position.X, (int) Owner.Model.Human.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.WorldParticles.Position = Owner.Model.Human.Position - Vector3.UnitY;
				    World.WorldParticles.Scale = Vector3.One * .5f;
				    World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.WorldParticles.Direction = (-Owner.Model.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.WorldParticles.ParticleLifetime = 1;
				    World.WorldParticles.GravityEffect = .1f;
				    World.WorldParticles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					if(World.WorldParticles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
					    World.WorldParticles.Color = underChunk.Biome.Colors.GrassColor;
					
					for(int i = 0; i < 4; i++){
					    World.WorldParticles.Emit();
					}
				}
				PreviousPosition = Owner.BlockPosition;
			}
		    base.SetToDefault(KnifeSheath);

            Matrix4 KnifeMat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
			
			this.KnifeSheath.Position = Owner.Model.Position;
			this.KnifeSheath.BeforeLocalRotation = -Vector3.UnitX * 1.75f - Vector3.UnitY * 3.0f;
			this.KnifeSheath.TransformationMatrix = KnifeMat4;
		}
		
		public override void Attack1(Humanoid Human){
            if(!this.MeetsRequirements()) return;

			base.Attack1(Human);
		    TaskManager.Delay(250, () => Trail.Emit = true);
        }
	}
}