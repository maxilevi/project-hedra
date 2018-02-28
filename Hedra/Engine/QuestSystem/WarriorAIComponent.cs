 /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of WarriorAI.
	/// </summary>
	public class WarriorAIComponent : EntityComponent
	{
		public bool Friendly {get; set;}
		private bool Chasing;
		private Entity ChasingTarget;
		private Vector3 TargetPoint;
		private Timer MovementTimer, AttackTimer, RollTimer;
		private Vector3 OriginalPosition;

		public WarriorAIComponent(Entity Parent, bool Friendly) : base(Parent)
		{
			this.Friendly = Friendly;
			this.MovementTimer = new Timer(6.0f);//Alert every 6.0f seconds
			this.AttackTimer = new Timer(0.75f);
            this.RollTimer = new Timer(4.0f);
			this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition;
			this.OriginalPosition = Parent.BlockPosition;
		}
		
		public override void Update()
		{
			if(Parent.Knocked) return;
			
			if( this.MovementTimer.Tick() && !Chasing){
				this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition;
			}
			else if(Chasing){
				
				if((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > 64*64 || ChasingTarget.IsDead || ChasingTarget.IsInvisible){
					
					//Restore 33% of the killer health
					if(ChasingTarget.IsDead)
						Parent.Health += ChasingTarget.MaxHealth * .33f;
					
					ChasingTarget = null;
					Chasing = false;
					this.TargetPoint = this.OriginalPosition;
					return;
				}
				
				this.TargetPoint = ChasingTarget.Position;
				if( Parent.InAttackRange(ChasingTarget) && !Parent.Knocked){
					Parent.Model.Idle();
					if(AttackTimer.Tick()){
						if(ChasingTarget is LocalPlayer)
							Parent.Model.Attack(ChasingTarget, 0f);
						else
							Parent.Model.Attack(ChasingTarget, 4.5f);//Do more damage to mobs
					}
				}else{
					
					var Human = Parent as Humanoid;//Only if the warrior is a human
					if(RollTimer.Tick() && Human != null && (TargetPoint.Xz - Parent.Position.Xz).LengthSquared > 16*16)
						Human.Roll();
					
					Parent.Model.Run();
					Parent.Physics.Move( Parent.Orientation * Parent.Speed * 4 * 2);
				}
			}
			
			Parent.Orientation = (TargetPoint - Parent.Position).Xz.NormalizedFast().ToVector3();
			Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
			
			if( (TargetPoint.Xz - Parent.Position.Xz).LengthSquared > 3*3 && !Chasing){
				Parent.Physics.Move( Parent.Orientation * Parent.Speed * 4 * 2);
				Parent.Model.Run();
			}else if(!Chasing){
				Parent.Model.Idle();
			}
			
			if(Friendly && !Chasing){
				for(int i = World.Entities.Count-1; i>-1; i--){
					if( World.Entities[i] != this.Parent &&
					   (World.Entities[i].Position.Xz - Parent.Position.Xz).LengthSquared < 32*32){
						
						if(World.Entities[i].IsStatic || World.Entities[i] is LocalPlayer 
						   || World.Entities[i].IsImmune || World.Entities[i].IsFriendly || World.Entities[i].IsInvisible) continue;
						
						this.Chasing = true;
						this.ChasingTarget = World.Entities[i];
						break;
					}
				}
			}else if(!Chasing){
				Humanoid Player = Scenes.SceneManager.Game.LPlayer;
				if( (Player.Position.Xz - Parent.Position.Xz).LengthSquared < 64*64 ){
					this.Chasing = true;
					this.ChasingTarget = Player;
				}
			}
		}
	}
}
