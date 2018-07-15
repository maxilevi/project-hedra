/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.QuestSystem
{
    /// <summary>
    /// Description of WarriorAI.
    /// </summary>
    internal class VillagerAIComponent : HumanoidAIComponent
    {
        private readonly bool _move;
        private Vector3 _targetPoint;
        private readonly Timer _movementTimer;
	    private bool _shouldSit;
        public override bool ShouldSleep => true;

        public VillagerAIComponent(Entity Parent, bool Move) : base(Parent)
        {
            this._move = Move;
            this._movementTimer = new Timer(6.0f);//Alert every 6.0f seconds
            this._targetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition;
        }

        public override void Update()
        {
            this.ManageSleeping();
            if (IsSleeping) return;

        	if( (LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < 24*24)
	        {
        		Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
	            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
	            Parent.Model.Idle();
        		return;
        	}       	
        	if(_move)
	        {
	            if(_movementTimer.Tick())
	            {
		            _shouldSit = Utils.Rng.Next(0, 3) == 1;
		            if(!_shouldSit)
			            _targetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.Physics.TargetPosition;
	            }

		        if (_shouldSit) base.Sit();
		        else base.Move(_targetPoint);	
        	}
        }
    }
}
