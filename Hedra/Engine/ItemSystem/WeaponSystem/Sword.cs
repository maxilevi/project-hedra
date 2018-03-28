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
	public class Sword : Weapon
	{
	    public override bool IsMelee { get; protected set; } = true;
        private Vector3 _previousPosition;
	    private bool FrontSlash => PrimaryAnimationsIndex == 2;


        public Sword(VertexData Contents) : base(Contents)
		{
			AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Stance.dae");

		    PrimaryAnimations = new Animation[3];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");
		    PrimaryAnimations[2] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Front.dae");

            for (var i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.35f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * (FrontSlash ? 1.25f : 1.0f) );
                };

                PrimaryAnimations[i].OnAnimationEnd += delegate
		        {	            
		            Trail.Emit = false;
                };
		    }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorLunge.dae");

		    for (var k = 0; k < SecondaryAnimations.Length; k++)
		    {
		        SecondaryAnimations[k].Loop = false;
		        SecondaryAnimations[k].OnAnimationEnd += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * 1.10f, delegate(Entity Mob)
		            {

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
		                        Owner.DamageEquation * 2f));

		            });
		        };
		    }
		}
		
		public override void Update(Humanoid Human)
		{
			base.Update(Human);
                
			if(SecondaryAttack){
				base.SetToMainHand(MainMesh);
				
				if(_previousPosition != Human.BlockPosition && Human.IsGrounded)
				{
				    Chunk underChunk = World.GetChunkAt(Human.Position);
				    World.WorldParticles.VariateUniformly = true;
				    World.WorldParticles.Color = World.GetHighestBlockAt( (int) Human.Position.X, (int) Human.Position.Z).GetColor(underChunk.Biome.Colors);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.WorldParticles.Position = Human.Position - Vector3.UnitY;
				    World.WorldParticles.Scale = Vector3.One * .5f;
				    World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.WorldParticles.Direction = (-Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.WorldParticles.ParticleLifetime = 1;
				    World.WorldParticles.GravityEffect = .1f;
				    World.WorldParticles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					if(World.WorldParticles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
						World.WorldParticles.Color = underChunk.Biome.Colors.GrassColor;
					
					World.WorldParticles.Emit();
				}
				_previousPosition = Human.BlockPosition;
			}	
		}

	    public override int ParsePrimaryIndex(int AnimationIndex)
	    {
	        return AnimationIndex == 5 ? 2 : AnimationIndex & 1;
	    }
		
		public override void Attack1(Humanoid Human){
			if(!base.MeetsRequirements()) return;

		    if (PrimaryAnimationsIndex == 5)
		        PrimaryAnimationsIndex = 0;

		    PrimaryAnimationsIndex++;

            base.BasePrimaryAttack(Human);
		    Trail.Emit = false;
		    TaskManager.Delay(200, () => Trail.Emit = true);
		}
	}
}