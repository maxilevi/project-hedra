﻿/*
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
	internal class MovementManager
	{
	    private const float NormalSpeed = 2.25f;
	    private const float AttackingSpeed = 0.75f;
        private readonly List<MoveOrder> _order;
	    private float _speed;
		public bool CaptureMovement { get; set; } = true;
	    public float JumpingDistance => Human.IsMoving ? 6f : 3f;
		public bool IsFloating { get; set; }
        public Vector3 RollDirection { get; set; }
	    public bool IsJumping { get; private set; }
        protected readonly Humanoid Human;

	    public MovementManager(Humanoid Human){
	        this._order = new List<MoveOrder>();
	        this.Human = Human;
        }

	    public Vector3 MoveFormula(Vector3 Direction)
	    {
	        float movementSpeed = (Human.IsUnderwater && !Human.IsGrounded ? 1.25f : 1.0f) * Human.Speed;
	        return Direction * 5f * 1.75f * movementSpeed * (Human.IsJumping ? 1.75f : 1f) * _speed;
	    }

        public void MoveInWater(bool Up)
        {
		    if(Human.IsRolling || Human.IsDead || !Human.CanInteract || !Human.IsUnderwater) return;

		    Human.IsGrounded = false;
		    Human.Physics.Velocity = Vector3.Zero;
		    Human.Model.Rotation = new Vector3(0, Human.Model.Rotation.Y, 0);
		    if(Up) Human.Physics.TargetPosition += Vector3.UnitY * 12.5f * (float) Time.DeltaTime;
		    else Human.Physics.TargetPosition -= Vector3.UnitY * 12.5f * (float) Time.DeltaTime;

		    Human.Physics.TargetPosition = new Vector3(
		        Human.Physics.TargetPosition.X,
		        Math.Max(Physics.HeightAtPosition(Human.Physics.TargetPosition)+2, Human.Physics.TargetPosition.Y),
		        Human.Physics.TargetPosition.Z);

		    IsFloating = true;
		}
		
		public void Jump()
        {
			if(IsJumping || Human.Knocked || Human.IsCasting || Human.IsRiding ||
                Human.IsRolling || Human.IsDead || !Human.IsGrounded || !Human.CanInteract ||
                Math.Abs(Human.Physics.TargetPosition.Y - Human.Position.Y) > 2.0f || !this.CaptureMovement)
				return;

		    Human.IsSitting = false;
		    Human.IsGrounded = false;
            CoroutineManager.StartCoroutine(this.JumpCoroutine);
        }

		public IEnumerator JumpCoroutine()
        {
			IsJumping = true;
			var startingY = Human.Physics.TargetPosition.Y;
			Human.Physics.GravityDirection = Vector3.Zero;
		    Human.IsGrounded = false;
		    var targetPush = 60f;
		    var push = 0f;
            var stoppedJump = false;
		    while (Human.Model.Position.Y < startingY + JumpingDistance && (push > 0.05f || targetPush > 0))
			{
			    bool shouldPlayJumpAnimation = Human.IsMoving;
                push = Mathf.Lerp(push, targetPush, Time.DeltaTime * 8f);
                float prevTarget = Human.Physics.TargetPosition.Y;
			    var command = new MoveCommand
			    {
			        Delta = Vector3.UnitY * push * Time.DeltaTime,
			        Parent = Human
                };
			    Human.Physics.ProccessCommand( command );
			    if (Math.Abs(prevTarget - Human.Physics.TargetPosition.Y) < .01f)
			    {
			        stoppedJump = true;
                    IsJumping = false;
			        Human.Model.Pause = false;
                    break;
			    }
			    if (shouldPlayJumpAnimation) Human.Model.Pause = true;
                yield return null;
			}
            if (!stoppedJump)
            {
                TaskManager.After(50, delegate
                {
                    IsJumping = false;
                });
                TaskManager.When(() => Human.IsGrounded || Human.IsUnderwater || Human.IsGliding,
                    () => Human.Model.Pause = false);
            }
            Human.Physics.GravityDirection = -Vector3.UnitY;
		}

	    protected virtual void DoUpdate() { }

	    public void Orientate()
	    {
	        Human.Model.TargetRotation = new Vector3(Human.Model.TargetRotation.X, Human.FacingDirection.Y, Human.Model.TargetRotation.Z);
	        var realAngle = (-Human.FacingDirection.Y+90) / Mathf.Degree;
            Human.Orientation = new Vector3((float) Math.Cos(realAngle), 0, (float) Math.Sin(realAngle));
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

		public void Update(){
		    _speed = Mathf.Lerp(_speed, Human.IsAttacking ? AttackingSpeed : NormalSpeed, (float) Time.DeltaTime * 2f);
            this.DoUpdate();
		    this.ManageMoveOrders();
		    this.ManageSwimming();
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

        private void ManageSwimming()
	    {
	        if (!Human.IsGrounded && !Human.IsDead && Human.CanInteract && !Human.IsRiding && Human.IsUnderwater && !GameSettings.Paused)
	        {
	            if (Human.IsMoving) Human.Model.Swim();     
	            else Human.Model.IdleSwim();  
	        }
        }

	    internal class MoveOrder
	    {
	        public Vector3 Position { get; set; }
            public float Seconds { get; set; }
            public float Progress { get; set; }
            public bool Orientate { get; set; }
	    }
	}
}
