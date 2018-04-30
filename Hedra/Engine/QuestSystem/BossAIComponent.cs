/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/07/2016
 * Time: 02:38 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of BossAIComponent.
	/// </summary>
	public class BossAIComponent : EntityComponent
	{
		protected LocalPlayer Player;
		public float AttackDamage = 0;
		public float Radius = 6;
		public float VisiblityRange = 96*96;
		public Action AILogic;
		public Func<Vector3> Protect;
		
		public BossAIComponent(Entity Parent) : base(Parent){
			Player = GameManager.Player;
		}
		
		public virtual void LateUpdate(){}
		
		public override void Update(){
			
			if(!GameSettings.Paused && !Parent.IsDead && !Parent.Knocked){
				
				Chunk UnderChunk = World.GetChunkAt(Parent.BlockPosition);
				if(UnderChunk == null) return;
				
				if(AILogic != null)
					AILogic();
				//UpdateMovement();
				LateUpdate();
			}
		}
		
		
		protected virtual void NoAbility(){
			
			if( (Player.Position - base.Parent.Position).LengthSquared < VisiblityRange ){
				
				if((Player.Position-Parent.Position).LengthSquared < Radius && !Player.IsDead){
					QuadrupedModel Model = Parent.Model as QuadrupedModel;
					if(Model != null){
						if(Model.AttackAnimation != Model.Model.Animator.AnimationPlaying){
							Parent.Model.Attack(Player, AttackDamage);
							return;
						}else{
							Parent.Model.Run();
							Physics.LookAt(Parent, Player);
							return;
						}
					}
			            
				}
				Parent.Model.Run();
				Parent.Physics.Move( (Player.Position - Parent.Position).NormalizedFast().Xz.ToVector3() * Parent.Speed * 4 * 2 * (float)Time.deltaTime);
				Physics.LookAt(Parent, Player);
			}else
				Parent.Model.Idle();
		}
	}
}
