/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:12 a.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Engine.Player
{
    public class MovementManager
    {
        private readonly List<MoveOrder> _order;
        protected readonly IHumanoid Human;
        private bool _appliedDownwardImpulse;
        private float _speed;
        protected Vector3 JumpPropulsion;

        public MovementManager(IHumanoid Human)
        {
            _order = new List<MoveOrder>();
            this.Human = Human;
        }

        public bool CaptureMovement { get; set; } = true;
        public Vector3 RollDirection { get; set; }
        public virtual bool IsMovingForward { get; protected set; }
        public virtual bool IsMovingBackwards { get; protected set; }
        public Vector3 LastOrientation { get; private set; }
        public float RollFacing { get; set; }
        public bool IsJumping { get; private set; }
        protected Vector3 AccumulatedMovement { get; set; }

        protected void ClampSwimming(IHumanoid Player)
        {
        }

        public void MoveInWater(bool Up)
        {
            if (Human.IsRolling || Human.IsDead || !Human.CanInteract || !Human.IsUnderwater) return;
            Human.IsGrounded = false;
            Human.Physics.ResetVelocity();
            Human.Model.LocalRotation = new Vector3(0, Human.Model.LocalRotation.Y, 0);
            if (Up) Human.Position += Vector3.UnitY * 12.5f * Time.DeltaTime;
            else Human.Position -= Vector3.UnitY * 12.5f * Time.DeltaTime;
        }

        protected void Jump()
        {
            var canJump = Human.IsGrounded;
            if (IsJumping || Human.IsKnocked || Human.IsCasting || Human.IsRiding ||
                Human.IsRolling || Human.IsDead || !canJump || !Human.CanInteract ||
                Math.Abs(Human.Position.Y - Human.Position.Y) > 2.0f || !CaptureMovement)
                return;

            ForceJump();
        }

        public void ForceJump(float Propulsion = 70)
        {
            Human.IsSitting = false;
            Human.IsGrounded = false;
            IsJumping = true;
            _appliedDownwardImpulse = false;
            Human.Physics.ResetFall();
            Human.Physics.GravityDirection = -Vector3.UnitY;
            Human.Physics.ApplyImpulse(Vector3.UnitY * Propulsion);
        }

        protected virtual void DoUpdate()
        {
        }

        public void ProcessMovement(float CharacterRotation, Vector3 MoveSpace, bool Orientate = true)
        {
            Human.Physics.MoveTowards(MoveSpace);
            if (Orientate)
                ProcessOrientation(MoveSpace, CharacterRotation);
        }

        public void ProcessTranslation(float CharacterRotation, Vector3 MoveSpace, bool Orientate)
        {
            Human.Physics.DeltaTranslate(MoveSpace);
            if (Orientate)
                ProcessOrientation(MoveSpace, CharacterRotation);
        }

        private void ProcessOrientation(Vector3 Towards, float CharacterRotation)
        {
            LastOrientation = new Vector3(Towards.X, 0, Towards.Z).NormalizedFast();
            if (!Human.WasAttacking && !Human.IsAttacking)
            {
                Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, CharacterRotation,
                    Human.Model.TargetRotation.Z);
                Human.Orientation = LastOrientation;
            }
        }


        public void OrientateTowards(float Facing)
        {
            Human.Model.TargetRotation =
                new Vector3(Human.Model.TargetRotation.X, Facing, Human.Model.TargetRotation.Z);
            var inRadians = Human.Model.LocalRotation.Y * Mathf.Radian;
            // There seems to be a bug in how we store the rotations so be switch the sines
            //if(Human is LocalPlayer player)
            //    Human.Orientation = player.View.LookingDirection;
            //else
            Human.Orientation = new Vector3((float)Math.Sin(inRadians), 0, (float)Math.Cos(inRadians));
        }

        public void Orientate()
        {
            if (Human is LocalPlayer)
                OrientateTowards(Human.FacingDirection);
        }

        public void Move(Vector3 Position, float Seconds, bool Orientate = true)
        {
            var order = new MoveOrder
            {
                Position = Position,
                Seconds = Seconds,
                Orientate = Orientate
            };
            _order.Add(order);
        }

        public void Update()
        {
            Human.IsSwimming = Human.IsMoving && Human.IsUnderwater;
            DoUpdate();
            ManageMoveOrders();
            HandleJumping();
        }

        private void HandleJumping()
        {
            if (!IsJumping) return;
            if (Human.IsGrounded && Human.Physics.Impulse.LengthSquared() < 30 || Human.IsUnderwater) CancelJump();

            if (Human.Physics.Impulse.LengthSquared() < 10f && !_appliedDownwardImpulse)
            {
                Human.Physics.ApplyImpulse(Vector3.UnitY * -4f);
                _appliedDownwardImpulse = true;
            }
        }

        public void CancelJump()
        {
            IsJumping = false;
        }

        private void ManageMoveOrders()
        {
            for (var i = _order.Count - 1; i > -1; i--)
                if (ExecuteMoveOrder(_order[i]))
                    _order.RemoveAt(i);
        }

        private bool ExecuteMoveOrder(MoveOrder Order)
        {
            Human.Physics.MoveTowards(Order.Position / Order.Seconds);
            Order.Progress += Time.DeltaTime;
            if (Order.Orientate) Orientate();
            return Order.Progress >= Order.Seconds;
        }

        private class MoveOrder
        {
            public Vector3 Position { get; set; }
            public float Seconds { get; set; }
            public float Progress { get; set; }
            public bool Orientate { get; set; }
        }
    }
}