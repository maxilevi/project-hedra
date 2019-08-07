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
using System.ComponentModel;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player
{
    public class MovementManager
    {
        private readonly List<MoveOrder> _order;
        private float _speed;
        public bool CaptureMovement { get; set; } = true;
        public Vector3 RollDirection { get; set; }
        public virtual bool IsMovingForward { get; protected set; }
        public virtual bool IsMovingBackwards { get; protected set; }
        public Vector3 LastOrientation { get; private set; }
        public float RollFacing { get; set; }
        public bool IsJumping { get; private set; }
        protected readonly IHumanoid Human;
        private Vector3 _jumpPropulsion;
        private float _lastHeight;

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
                Human.Position = new Vector3(
                    Human.Position.X,
                    minHeight,
                    Human.Position.Z
                );
            }
        }

        public void MoveInWater(bool Up)
        {
            if(Human.IsRolling || Human.IsDead || !Human.CanInteract || !Human.IsUnderwater) return;
            if(Human.Position.Y + Human.Model.Height + 1 > Physics.WaterHeight(Human.Position) && Up) return;
            Human.IsGrounded = false;
            Human.Physics.ResetVelocity();
            Human.Model.LocalRotation = new Vector3(0, Human.Model.LocalRotation.Y, 0);
            if(Up) Human.Position += Vector3.UnitY * 12.5f * (float) Time.DeltaTime;
            else Human.Position -= Vector3.UnitY * 12.5f * (float) Time.DeltaTime;

            Human.Position = new Vector3(
                Human.Position.X,
                Math.Max(Physics.HeightAtPosition(Human.Position)+2, Human.Position.Y),
                Human.Position.Z);
        }

        protected void Jump()
        {
            var canJump = Human.IsGrounded || Human.Position.Y - Human.Model.Height * .5f < Physics.HeightAtPosition(Human.Position);
            if (IsJumping || Human.IsKnocked || Human.IsCasting || Human.IsRiding ||
                Human.IsRolling || Human.IsDead || !canJump || !Human.CanInteract ||
                Math.Abs(Human.Position.Y - Human.Position.Y) > 2.0f || !this.CaptureMovement)
                return;

            ForceJump();
        }

        public void ForceJump(float Propulsion = 60)
        {
            Human.IsSitting = false;
            Human.IsGrounded = false;
            IsJumping = true;
            Human.Physics.ResetFall();
            Human.Physics.GravityDirection = -Vector3.UnitY;
            _jumpPropulsion = Vector3.UnitY * Propulsion;
        }

        protected virtual void DoUpdate() { }

        public void ProcessMovement(float CharacterRotation, Vector3 MoveSpace, bool Orientate = true)
        {
            Human.Physics.DeltaTranslate(MoveSpace);
            if (Orientate)
            {
                LastOrientation = new Vector3(MoveSpace.X, 0, MoveSpace.Z).NormalizedFast();
                if (!Human.WasAttacking && !Human.IsAttacking)
                {
                    Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, CharacterRotation,
                        Human.Model.TargetRotation.Z);
                    Human.Orientation = LastOrientation;
                }
            }

            Human.IsSitting = false;
        }
        
        public void OrientateTowards(float Facing)
        {
            Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, Facing, Human.Model.TargetRotation.Z);
            var inRadians = Human.Model.LocalRotation.Y * Mathf.Radian;
            // There seems to be a bug in how we store the rotations so be switch the sines
            //if(Human is LocalPlayer player)
            //    Human.Orientation = player.View.LookingDirection;
            //else
                Human.Orientation = new Vector3((float)Math.Sin(inRadians), 0, (float)Math.Cos(inRadians));
        }
        
        public void Orientate()
        {
            if(Human is LocalPlayer)
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
            if ((Physics.HeightAtPosition(Human.Position) + .5f > Human.Position.Y || Human.IsGrounded) 
                && _jumpPropulsion.LengthFast < 30 || Human.IsUnderwater)
            {
                CancelJump();
            }

            if (!Human.Physics.DeltaTranslate(_jumpPropulsion, true)) CancelJump();
            _jumpPropulsion *= (float)Math.Pow(.25f, Time.DeltaTime * 3f);
        }

        public void CancelJump()
        {
            IsJumping = false;
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
