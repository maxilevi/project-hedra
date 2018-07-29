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
using Hedra.Engine.Generation.ChunkSystem;
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
	internal class Knife : MeleeWeapon
	{
	    private static readonly VertexData SheathData;
        private readonly ObjectMesh KnifeSheath;
	    private Vector3 _previousPosition;

        static Knife()
        {
            SheathData = AssetManager.PLYLoader("Assets/Items/KnifeSheath.ply", Vector3.One);
            SheathData.Transform(Matrix4.CreateRotationY(180f * Mathf.Radian));
            SheathData.Scale(new Vector3(3.75f, 1.8f, 2f));
        }

        public Knife(VertexData Contents) : base(Contents)
		{
			KnifeSheath = ObjectMesh.FromVertexData(SheathData);
			
			AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherShootStance.dae");

		    PrimaryAnimations = new Animation[2];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.25f;
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
		        PrimaryAnimations[i].Speed = 1.5f;
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

                Matrix4 Mat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.ChestJoint).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
			
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.BeforeLocalRotation = -Vector3.UnitX * 1.6f - Vector3.UnitY * 2f;
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.TargetRotation = new Vector3(180, 0, 0);
			}
			
			if(base.InAttackStance || Owner.WasAttacking){

                Matrix4 Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(270,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.1f - Vector3.UnitZ * .2f;
				
			}
			
			if(PrimaryAttack){

                Matrix4 Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
			}
			
			if(SecondaryAttack){

				Matrix4 Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
				
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(180,0,0);
				this.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				
				if(_previousPosition != Owner.Model.Human.BlockPosition && Owner.Model.Human.IsGrounded){
				    Chunk underChunk = World.GetChunkAt(Owner.Model.Position);
                    World.Particles.VariateUniformly = true;
				    World.Particles.Color = World.GetHighestBlockAt( (int) Owner.Model.Human.Position.X, (int) Owner.Model.Human.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.Particles.Position = Owner.Model.Human.Position - Vector3.UnitY;
				    World.Particles.Scale = Vector3.One * .5f;
				    World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.Particles.Direction = (-Owner.Model.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.Particles.ParticleLifetime = 1;
				    World.Particles.GravityEffect = .1f;
				    World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					if(World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
					    World.Particles.Color = underChunk.Biome.Colors.GrassColor;
					
					for(int i = 0; i < 4; i++){
					    World.Particles.Emit();
					}
				}
				_previousPosition = Owner.BlockPosition;
			}
		    base.SetToDefault(KnifeSheath);

            Matrix4 KnifeMat4 = Owner.Model.Model.MatrixFromJoint(Owner.Model.ChestJoint).ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
			
			this.KnifeSheath.Position = Owner.Model.Position;
			this.KnifeSheath.BeforeLocalRotation = -Vector3.UnitX * 1.75f - Vector3.UnitY * 3.0f;
			this.KnifeSheath.TransformationMatrix = KnifeMat4;
		}
		
		public override void Attack1(Humanoid Human){
            if(!this.MeetsRequirements()) return;

			base.Attack1(Human);
		    TaskManager.After(250, () => Trail.Emit = true);
        }
	}
}