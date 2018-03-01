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
	public class WarriorAIComponent : CombatAIComponent
    {
        private readonly Timer _attackTimer;
        public override float SearchRadius { get; set; } = 64;
        public override float AttackRadius { get; set; } = 0;
        public override float ForgetRadius { get; set; } = 64;

        public WarriorAIComponent(Entity Parent, bool Friendly) : base(Parent, Friendly)
		{
			this._attackTimer = new Timer(0.75f);
		}
		
		public override void DoUpdate()
		{			
			if( this.MovementTimer.Tick() && !Chasing)
				this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition;		
			else if(Chasing){
				
				if((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > ForgetRadius * ForgetRadius || ChasingTarget.IsDead || ChasingTarget.IsInvisible)
				{
				    base.Reset();
					return;
				}
				
				this.TargetPoint = ChasingTarget.Position;
				if( Parent.InAttackRange(ChasingTarget) && !Parent.Knocked){
					Parent.Model.Idle();
					if(_attackTimer.Tick()){
						if(ChasingTarget is LocalPlayer)
							Parent.Model.Attack(ChasingTarget, 0f);
						else
							Parent.Model.Attack(ChasingTarget, 4.5f);//Do more damage to mobs
					}
				}else
				{
				    base.Roll();
				}
			}
			
		    base.LookTarget();
		}
	}
}
