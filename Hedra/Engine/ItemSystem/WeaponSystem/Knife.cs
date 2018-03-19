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
		private EntityMesh KnifeSheath;
		
		public Knife(VertexData Contents) : base(Contents)
		{
			
			VertexData SheathData = AssetManager.PlyLoader("Assets/Items/KnifeSheath.ply", Vector3.One);
			SheathData.Transform( Matrix4.CreateRotationY(180f * Mathf.Radian) );
			SheathData.Scale( new Vector3(3.75f, 1.8f, 2f));
			KnifeSheath = EntityMesh.FromVertexData(SheathData);
			
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

		            Model.Human.Attack(Model.Human.DamageEquation * 0.7f, delegate(Entity Mob)
		            {
		                if (Utils.Rng.Next(0, 5) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Model.Human, 3f,
		                        Model.Human.DamageEquation * 2.0f));
		            });
		        };

		        PrimaryAnimations[i].OnAnimationEnd += delegate
		        {
		            _trail.Emit = false;
		        };
            }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorLunge.dae");

		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[i].Loop = false;
		        SecondaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Model.Human.Attack(Model.Human.DamageEquation * 0.8f, delegate(Entity Mob)
		            {
		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Model.Human, 4f,
		                        Model.Human.DamageEquation * 2.0f));
		            });
		        };
		    }
		}
		
		public override void Update(HumanModel Model)
		{
			base.Update(Model);
		    base.SetToDefault(Mesh);

            if (Sheathed){

                Matrix4 Mat4 = Model.Model.MatrixFromJoint(Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.ChestPosition);
			
				this.Mesh.Position = Model.Position;
				this.Mesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
				this.Mesh.TransformationMatrix = Mat4;
				this.Mesh.TargetRotation = new Vector3(180, 0, 0);
			}
			
			if(base.InAttackStance || Model.Human.WasAttacking){

                Matrix4 Mat4 = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);
				
				this.Mesh.TransformationMatrix = Mat4;
				this.Mesh.Position = Model.Position;
				this.Mesh.TargetRotation = new Vector3(270,0,0);
				this.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.1f - Vector3.UnitZ * .2f;
				
			}
			
			if(PrimaryAttack){

                Matrix4 Mat4 = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);
				
				this.Mesh.TransformationMatrix = Mat4;
				this.Mesh.Position = Model.Position;
				this.Mesh.TargetRotation = new Vector3(180,0,0);
				this.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
			}
			
			if(SecondaryAttack){

				Matrix4 Mat4 = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);
				
				this.Mesh.TransformationMatrix = Mat4;
				this.Mesh.Position = Model.Position;
				this.Mesh.TargetRotation = new Vector3(180,0,0);
				this.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				
				if(PreviousPosition != Model.Human.BlockPosition && Model.Human.IsGrounded){
				    Chunk underChunk = World.GetChunkAt(Model.Position);
                    World.WorldParticles.VariateUniformly = true;
				    World.WorldParticles.Color = World.GetHighestBlockAt( (int) Model.Human.Position.X, (int) Model.Human.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.WorldParticles.Position = Model.Human.Position - Vector3.UnitY;
				    World.WorldParticles.Scale = Vector3.One * .5f;
				    World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.WorldParticles.Direction = (-Model.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.WorldParticles.ParticleLifetime = 1;
				    World.WorldParticles.GravityEffect = .1f;
				    World.WorldParticles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					if(World.WorldParticles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
					    World.WorldParticles.Color = underChunk.Biome.Colors.GrassColor;
					
					for(int i = 0; i < 4; i++){
					    World.WorldParticles.Emit();
					}
				}
				PreviousPosition = Model.Human.BlockPosition;
			}
		    base.SetToDefault(KnifeSheath);

            Matrix4 KnifeMat4 = Model.Model.MatrixFromJoint(Model.Chest).ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.ChestPosition);
			
			this.KnifeSheath.Position = Model.Position;
			this.KnifeSheath.BeforeLocalRotation = -Vector3.UnitX * 1.75f - Vector3.UnitY * 3.0f;
			this.KnifeSheath.TransformationMatrix = KnifeMat4;
		}
		
		public override void Attack1(HumanModel Model){
            if(!this.MeetsRequirements()) return;

			base.Attack1(Model);
		    TaskManager.Delay(250, () => _trail.Emit = true);
        }
	}
}