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
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of WarriorAI.
	/// </summary>
	public class WarriorAIComponent : CombatAIComponent
    {
        private float _attackTimer;
        public override float SearchRadius { get; set; } = 64;
        public override float AttackRadius { get; set; } = 0;
        public override float ForgetRadius { get; set; } = 64;

        public WarriorAIComponent(Entity Parent, bool Friendly) : base(Parent, Friendly)
		{
			this._attackTimer = 0f;
		}
		
		public override void DoUpdate()
		{
		    if (this.MovementTimer.Tick() && !Chasing)
		    {
		        this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) +
		                           Parent.BlockPosition;
		    }
		    else if (Chasing)
		    {

		        if ((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > ForgetRadius * ForgetRadius ||
		            ChasingTarget.IsDead || ChasingTarget.IsInvisible)
		        {
		            base.Reset();
		            return;
		        }

		        this.TargetPoint = ChasingTarget.Position;
		        this._attackTimer -= Time.FrameTimeSeconds;
		        if (Parent.InAttackRange(ChasingTarget) && !Parent.Knocked)
		        {
		            if (_attackTimer < 0)
		            {
		                Parent.Model.Attack(ChasingTarget);
		                _attackTimer = 1.25f;
		            }
		        }
		        else
		        {
		            base.Roll();
		        }
		    }

		    base.LookTarget();
		}
	}
}
