/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:12 a.m.
 *
 */
using System;
using OpenTK;
using Hedra.Engine.Management;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public class MovementManager
    {
        private readonly List<MoveOrder> _order;
        private float _speed;
        public bool CaptureMovement { get; set; } = true;
        public Vector3 RollDirection { get; set; }
        public float RollFacing { get; set; }
        public bool IsJumping { get; private set; }
        protected readonly IHumanoid Human;
        private Vector3 _jumpPropulsion;

        public MovementManager(IHumanoid Human)
        {
            this._order = new List<MoveOrder>();
            this.Human = Human;
        }

        protected void ClampSwimming(IHumanoid Player)
        {
            var minHeight = Physics.HeightAtPosition(Player.Position);
            if (Player.Position.Y < minHeight)
            {
                Human.Physics.TargetPosition = new Vector3(
                    Human.Physics.TargetPosition.X,
                    minHeight,
                    Human.Physics.TargetPosition.Z
                    );
            }
        }

        public void MoveInWater(bool Up)
        {
            if(Human.IsRolling || Human.IsDead || !Human.CanInteract || !Human.IsUnderwater) return;
            if(Human.Position.Y + Human.Model.Height + 1 > Physics.WaterHeight(Human.Physics.TargetPosition) && Up) return;
            Human.IsGrounded = false;
            Human.Physics.Velocity = Vector3.Zero;
            Human.Model.Rotation = new Vector3(0, Human.Model.Rotation.Y, 0);
            if(Up) Human.Physics.TargetPosition += Vector3.UnitY * 12.5f * (float) Time.DeltaTime;
            else Human.Physics.TargetPosition -= Vector3.UnitY * 12.5f * (float) Time.DeltaTime;

            Human.Physics.TargetPosition = new Vector3(
                Human.Physics.TargetPosition.X,
                Math.Max(Physics.HeightAtPosition(Human.Physics.TargetPosition)+2, Human.Physics.TargetPosition.Y),
                Human.Physics.TargetPosition.Z);
        }

        protected void Jump()
        {
            var canJump = Human.IsGrounded || Human.Position.Y - Human.Model.Height * .5f < Physics.HeightAtPosition(Human.Position);
            if (IsJumping || Human.IsKnocked || Human.IsCasting || Human.IsRiding ||
                Human.IsRolling || Human.IsDead || !canJump || !Human.CanInteract ||
                Math.Abs(Human.Physics.TargetPosition.Y - Human.Position.Y) > 2.0f || !this.CaptureMovement)
                return;

            Human.IsSitting = false;
            Human.IsGrounded = false;
            IsJumping = true;
            Human.Physics.ResetFall();
            Human.Physics.GravityDirection = -Vector3.UnitY;
            _jumpPropulsion = Vector3.UnitY * 80f;
        }

        protected virtual void DoUpdate() { }

        public void ProcessMovement(float CharacterRotation, Vector3 MoveSpace, bool Orientate = true)
        {
            Human.Physics.DeltaTranslate(MoveSpace);

            if (Orientate)
            {
                if (!Human.WasAttacking && !Human.IsAttacking)
                {
                    Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, CharacterRotation,
                        Human.Model.TargetRotation.Z);
                    Human.Orientation = new Vector3(MoveSpace.X, 0, MoveSpace.Z).NormalizedFast();
                }
            }
        }
        
        public void OrientateTowards(float Facing)
        {
            Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, Facing, Human.Model.TargetRotation.Z);
            var inRadians = Human.Model.Rotation.Y * Mathf.Radian;
            // There seems to be a bug in how we store the rotations so be switch the sines
            Human.Orientation = new Vector3((float) Math.Sin(inRadians), 0, (float) Math.Cos(inRadians));
        }
        
        public void Orientate()
        {
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
            this.DoUpdate();
            this.ManageMoveOrders();
            this.HandleJumping();
        }

        private void HandleJumping()
        {
            if (!IsJumping) return;
            if ((Physics.HeightAtPosition(Human.Position)+2 > Human.Position.Y || Human.IsGrounded) && _jumpPropulsion.LengthFast < 40 || Human.IsUnderwater)
                IsJumping = false;
            Human.Physics.DeltaTranslate(_jumpPropulsion);
            _jumpPropulsion *= (float)Math.Pow(.25f, Time.DeltaTime * 3f);
        }

        private void ManageMoveOrders()
        {
            for (var i = _order.Count-1; i > -1; i--)
            {
                if(this.ExecuteMoveOrder(_order[i]))
                    this._order.RemoveAt(i);
            }
        }

        private bool ExecuteMoveOrder(MoveOrder Order)
        {
            Human.Physics.DeltaTranslate(Order.Position / Order.Seconds);
            Order.Progress += (float) Time.DeltaTime;
            if(Order.Orientate) this.Orientate();
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
