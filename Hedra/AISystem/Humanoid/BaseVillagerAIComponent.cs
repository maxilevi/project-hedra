/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    ///     Description of WarriorAI.
    /// </summary>
    public abstract class BaseVillagerAIComponent : TraverseHumanoidAIComponent
    {
        private readonly bool _move;
        private Vector3 _targetPoint;

        protected BaseVillagerAIComponent(IHumanoid Parent, bool Move) : base(Parent)
        {
            _move = Move;
            IsSitting = true;
            MovementTimer = new Timer(WaitTime);
            if (Utils.Rng.NextBool())
                MovementTimer.MarkReady();
        }

        protected Timer MovementTimer { get; }
        protected bool IsSitting { get; set; }
        protected override bool ShouldSleep => true;

        protected virtual float WaitTime => 8.0f;

        protected virtual Vector3 NewPoint =>
            new Vector3(Utils.Rng.NextFloat() * 18 - 9f, 0, Utils.Rng.NextFloat() * 18 - 9f) * Chunk.BlockSize +
            Parent.Position;

        protected virtual bool ShouldSit { get; } = true;

        public override void Update()
        {
            base.Update();
            if (!CanUpdate) return;
            if (Parent.IsNear(GameManager.Player, 16) && !IsMoving)
            {
                Parent.Orientation = (GameManager.Player.Position - Parent.Position).Xz().NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
            }
            else if (_move)
            {
                if (MovementTimer.Tick())
                {
                    IsSitting = Utils.Rng.Next(0, 4) == 1 && !IsMoving && ShouldSit;
                    if (!IsSitting)
                    {
                        _targetPoint = NewPoint;
                        MoveTo(_targetPoint);
                    }
                }

                if (IsSitting) Sit();
                if (Parent.IsUnderwater)
                    Parent.Movement.MoveInWater(true);
            }
        }
    }
}