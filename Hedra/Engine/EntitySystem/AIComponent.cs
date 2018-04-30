/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/05/2016
 * Time: 10:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    ///     Description of AIComponent.
    /// </summary>
    public class AIComponent : EntityComponent
    {
        private readonly Random _rng = new Random();
        private Action _callback;
        private bool _callbacked;
        private bool _ended;
        private bool _floating;
        private Vector3 _lastPos;
        private Vector3 _newPos;
        private Vector3 _point;
        private float _timeLeft;
        private QuadrupedModel _model;

        public Action AILogic;
        public Entity AttackTarget;
        public bool DoLogic = true;
        public bool Enabled = true;
        public Timer FollowTimer = new Timer(16f);
        public Action OldLogic;
        public Vector3 TargetPosition;
        private AIType _type;

        public AIComponent(Entity Parent, AIType Type) : base(Parent)
        {
            _model = Parent.Model as QuadrupedModel;
            if(_model == null) throw new ArgumentException("AI Component only supports Quadruped models.");
            switch (Type)
            {
                case AIType.Neutral:
                    AILogic = this.Neutral;
                    break;

                case AIType.Hostile:
                    AILogic = this.Hostile;
                    break;

                case AIType.Friendly:
                    AILogic = this.Friendly;
                    break;

                default:
                    AILogic = this.Neutral;
                    break;
            }
            _type = Type;
        }

        public override void Update()
        {
            if (!DoLogic) return;

            if (!GameSettings.Paused && !Parent.IsDead && !Parent.Knocked && Enabled)
            {
                AILogic();
            }
            this.UpdateMovement();
        }

        public void Attack(Entity Target, bool Force = false)
        {
            if (Target == null)
                return;

            Physics.LookAt(Parent, Target);
            AttackTarget = Target;

            if (((Target.Position - Parent.Position).LengthSquared > 64 * 64 || Target.IsInvisible) && !Force || Target.IsDead)
            {
                if (Target.IsDead && Target is LocalPlayer)
                    Parent.Health = Parent.MaxHealth;
                AILogic = _type == AIType.Neutral ? this.Neutral : _type == AIType.Hostile ? this.Hostile : OldLogic;
                return;
            }
            if (FollowTimer.Tick())
            {
                AILogic = this.Neutral;
                TaskManager.Delay(8000, delegate { AILogic = OldLogic; });
                return;
            }
            
            if (Parent.InAttackRange(Target) && !Target.IsDead && !Target.IsInvisible)
            {
                Parent.Model.Attack(Target, Parent.AttackDamage + _rng.NextFloat() * 3 * 2f - 3f);
                FollowTimer.Reset();
            }
            else
            {
                float dist = (Target.Position.Xz - Parent.Position.Xz).LengthFast;
                this.MoveToPoint(
                    Target.Position.Xz.ToVector3() -
                    (Target.Position - Parent.Position).NormalizedFast() * .5f * dist, delegate{});
            }
        }

        private void Friendly() { }

        private void Hostile()
        {
            LocalPlayer player = GameManager.Player;
            if (player == null) return;

            this.Neutral();

            if (!((Parent.Position - player.Position).LengthSquared < 24 * 24) || player.IsInvisible || player.IsDead) return;

            AILogic = () => this.Attack(player);
            OldLogic = this.Hostile;
            FollowTimer.Reset();
        }

        private void Neutral()
        {
            if (_timeLeft <= 0)
            {
                _newPos = new Vector3(_rng.Next(-12, 12), 0, _rng.Next(-12, 12)) * Chunk.BlockSize + Parent.BlockPosition;
                _ended = false;
                _timeLeft = 10f + (float) _rng.NextDouble() * 10f - 5f;
            }

            if (_ended)
                _timeLeft -= Time.FrameTimeSeconds;

            this.MoveToPoint(_newPos, delegate { _ended = true; });
        }

        public void MoveToPoint(Vector3 Point, Action Callback)
        {
            _point = Point;
            TargetPosition = Point;
            _callbacked = false;
            _callback = Callback;
        }

        private void UpdateMovement()
        {
            if (Parent.Position.Y < -3 || _floating)
            {
                Parent.Physics.Velocity = Vector3.Zero;
                Parent.IsGrounded = false;
                Parent.Physics.GravityDirection = Vector3.Zero;
                Parent.Physics.TargetPosition += Vector3.UnitY * 3 * (float) Time.deltaTime;
                _floating = true;
            }
            if (Parent.Position.Y > 1.5f && _floating)
                _floating = false;

            if (Parent.Position.Xz == _lastPos.Xz)
            {
                //Parent.Model.Idle();
                _lastPos = Vector3.Zero;
                if (_callback != null && !_callbacked)
                    _callback();
                return;
            }

            _lastPos = Parent.Position;
            if ((_point.Xz - Parent.Position.Xz).LengthSquared < 6 * 6 || _point.Xz == Vector2.Zero)
            {
                if (AttackTarget == null)
                {
                    Parent.Orientation = (_point - Parent.Position).Xz.NormalizedFast().ToVector3();
                    Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                }

                Parent.Model.Idle();
                if (_callback != null && !_callbacked)
                    _callback();
            }
            else if(!_model.IsAttacking)
            {
                Parent.Orientation = (_point - Parent.Position).Xz.NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);

                Parent.Physics.Move(Parent.Orientation * 5 * Parent.Speed * (float) Time.deltaTime);
                Parent.Model.Run();
            }
        }
    }

    public enum AIType
    {
        Neutral,
        Friendly,
        Hostile
    }
}