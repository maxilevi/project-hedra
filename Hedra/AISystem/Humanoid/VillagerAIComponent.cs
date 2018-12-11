/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of WarriorAI.
    /// </summary>
    public class VillagerAIComponent : HumanoidAIComponent
    {
        private readonly bool _move;
        private readonly Timer _movementTimer;
        private Vector3 _targetPoint;
        protected bool IsSitting;
        protected override bool ShouldSleep => true;

        public VillagerAIComponent(IHumanoid Parent, bool Move) : base(Parent)
        {
            _move = Move;
            _movementTimer = new Timer(WaitTime);
            IsSitting = true;
        }

        public override void Update()
        {
            base.Update();
            if (!base.CanUpdate) return;

            if((GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 16*16 && !IsMoving)
            {
                Parent.Orientation = (GameManager.Player.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
                return;
            }           
            if(_move)
            {
                if(_movementTimer.Tick())
                {
                    IsSitting = Utils.Rng.Next(0, 4) == 1 && !IsMoving;
                    if(!IsSitting)
                        _targetPoint = NewPoint;
                }

                if (IsSitting) Sit();
                else Move(_targetPoint);    
            }
        }
        
        protected virtual float WaitTime => 8.0f;

        protected virtual Vector3 NewPoint =>
            new Vector3(Utils.Rng.NextFloat() * 18 - 9f, 0, Utils.Rng.NextFloat() * 18 - 9f) * Chunk.BlockSize +
            Parent.Physics.TargetPosition;
    }
}
