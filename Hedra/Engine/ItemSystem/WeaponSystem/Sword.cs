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
		            Model.Human.Attack(Model.Human.DamageEquation * (FrontSlash ? 1.25f : 1.0f) );
                };

                PrimaryAnimations[i].OnAnimationEnd += delegate
		        {	            
		            _trail.Emit = false;
                };
		    }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorLunge.dae");

		    for (var k = 0; k < SecondaryAnimations.Length; k++)
		    {
		        SecondaryAnimations[k].Loop = false;
		        SecondaryAnimations[k].OnAnimationEnd += delegate
		        {
		            Model.Human.Attack(Model.Human.DamageEquation * 1.10f, delegate(Entity Mob)
		            {

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Model.Human, 4f,
		                        Model.Human.DamageEquation * 2f));

		            });
		        };
		    }
		}
		
		public override void Update(HumanModel Model)
		{
			base.Update(Model);
                
			if(SecondaryAttack){
				base.SetToMainHand(Mesh);
				
				if(_previousPosition != Model.Human.BlockPosition && Model.Human.IsGrounded)
				{
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
					
					World.WorldParticles.Emit();
				}
				_previousPosition = Model.Human.BlockPosition;
			}
			
		}

	    public override int ParsePrimaryIndex(int AnimationIndex)
	    {
	        return AnimationIndex == 5 ? 2 : AnimationIndex & 1;
	    }
		
		public override void Attack1(HumanModel Model){
			if(!base.MeetsRequirements()) return;

		    if (PrimaryAnimationsIndex == 5)
		        PrimaryAnimationsIndex = 0;

		    PrimaryAnimationsIndex++;

            base.BasePrimaryAttack(Model);

		    TaskManager.Delay(200, () => _trail.Emit = true);
		}
	}
}