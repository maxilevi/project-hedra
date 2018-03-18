/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/01/2017
 * Time: 09:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of MountAIComponent.
	/// </summary>
	public class MountAIComponent : EntityComponent
	{
		private MountAIType Type;
		private Entity Target = null;
		private Entity Owner = null;
		private RideComponent RideComp = null;
		private float WasAttackingTimer = 0f, PreviousHealth;
		public bool DoLogic = true;
		public float Damage {get; set;}
		public MountAIComponent(Entity Parent, Entity Owner, MountAIType Type) : base(Parent)
		{
			this.Type = Type;
			this.Owner = Owner;
			this.Target = Owner;
			this.PreviousHealth = Parent.Health;
			
			//Attack when owner is attacked
			if(Owner.SearchComponent<DamageComponent>() != null){
				Owner.SearchComponent<DamageComponent>().OnDamageEvent += delegate(DamageEventArgs Args) {
					//Target = Args.Damager;
				};
			}
		}
		
		public override void Update()
		{
            /*
			WasAttackingTimer -= Time.ScaledFrameTimeSeconds;
			if(PreviousHealth != Parent.Health){
				WasAttackingTimer = 8f;
				PreviousHealth = Parent.Health;
			}
			
			if(WasAttackingTimer < 0f){
				Parent.Health += 8f * Time.ScaledFrameTimeSeconds;
			}*/
		    //Parent.Health = Parent.MaxHealth;
			
			if(RideComp != null){
				DoLogic = !RideComp.HasRider;
			}else{
				RideComp = Parent.SearchComponent<RideComponent>();
			}
			
			if(!DoLogic) return;
			

			if( Target != null && Target != Owner && (Target.Position - Parent.Position).LengthSquared > 72*72 ){
				Target = Owner;
			}
			
			if( (Parent.Position - Owner.Position).LengthSquared > 128*128){
				Parent.Position = Owner.BlockPosition + OpenTK.Vector3.UnitX * 12f;
			}
			
			float Distance = (Target == Owner) ? 8 : 3;
			if( Target != null && (Target.Position - Parent.Position).LengthSquared > Distance * Distance * Chunk.BlockSize * Chunk.BlockSize ){	
				
				Parent.Model.Run();
			}else if (Target != null && (Target.Position - Parent.Position).LengthSquared < Distance*.75f * Distance*.75f * Chunk.BlockSize * Chunk.BlockSize){
					
				if(Target != null && Target != Owner){
					Parent.Model.Attack(Target, Parent.AttackDamage);
					if(Target.IsDead) Target = null;
				}else{
					Parent.Model.Idle();
				}
			}
			QuadrupedModel QuadModel = (Parent.Model as QuadrupedModel);
			if(QuadModel.WalkAnimation == QuadModel.Model.Animator.AnimationPlaying && Target != null){
				Parent.Orientation = (Target.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
				Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
				Parent.Physics.Move( Parent.Speed * 8 * Parent.Orientation );
			}
			
			if( (Target == Owner || Target == null) ){
				//bool FoundSomething = false;
				/*for(int i = World.Entities.Count-1; i > -1; i--){
					if( World.Entities[i] != Owner 
					   && World.Entities[i] != Parent
					   && !World.Entities[i].IsStatic
					   && (World.Entities[i].Position - Parent.Position).LengthSquared < 72*72){
						
						Target = World.Entities[i];
						FoundSomething = true;
						break;
					}
				}*/
				//if(!FoundSomething){
				//	Target = Owner;
				//}
			}
		}
	}
	
	public enum MountAIType{
		Wolf,
		Horse
	}
}
