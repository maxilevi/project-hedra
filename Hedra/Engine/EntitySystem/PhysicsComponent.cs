﻿/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 09:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Generation;
using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.EntitySystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of PhysicsComponent.
	/// </summary>
	public class  PhysicsComponent : EntityComponent{
	
		public bool UsePhysics{ get; set; }
	    public float FallTime { get; private set; }
        public PhysicsComponent(Entity Parent) : base(Parent){ UsePhysics = true; UseTimescale = true; }
		public Vector3 GravityDirection = new Vector3(0,-1f,0);
		public float VelocityCap = float.MaxValue;
		public float HitboxSize = 12f;
		public Vector3 Force = Vector3.Zero;
		public Vector3 Velocity = Vector3.Zero;
		public bool HasFallDamage = true;
		public bool UseTimescale {get; set;}

        /// <summary>
        /// If collides with structures
        /// </summary>
        public bool CanCollide = false;
		/// <summary>
		/// If it pushes entities when moving
		/// </summary>
		public bool PushAround = true;
		/// <summary>
		/// If collides with other entities
		/// </summary>
		public bool HasCollision = true;
		public float BaseHeight = .0f;
		
		private Chunk _underChunk, _underChunkR, _underChunkL, _underChunkF, _underChunkB;
		private readonly List<ICollidable> _collisions = new List<ICollidable>();
		private float _height;
		private bool _isOverBox, _isInsideHitbox;
		private bool _isOverTerrain;
		private float _deltaTime;
	    private bool _wasBlockPx;
	    private bool _wasBlockNx;
	    private bool _wasBlockPy;
	    private bool _wasBlockNy;
	    private bool _wasBlockPz;
	    private bool _wasBlockNz;

	    public Vector3 TargetPosition
	    {
            get { return Parent.BlockPosition * new Vector3(1,Chunk.BlockSize,1); }
            set { Parent.BlockPosition = new Vector3(value.X, value.Y / Chunk.BlockSize, value.Z); }
	    }

	    public override void Update(){
			if(!UsePhysics)
				return;

			_deltaTime = (float) ( UseTimescale ? Time.deltaTime : Time.unScaledDeltaTime);

	        if (CanCollide)
	        {

	            _underChunk = World.GetChunkAt(Parent.Position);
	            _underChunkR = World.GetChunkAt(Parent.Position + new Vector3(Chunk.ChunkWidth, 0, 0));
	            _underChunkL = World.GetChunkAt(Parent.Position - new Vector3(Chunk.ChunkWidth, 0, 0));
	            _underChunkF = World.GetChunkAt(Parent.Position + new Vector3(0, 0, Chunk.ChunkWidth));
	            _underChunkB = World.GetChunkAt(Parent.Position - new Vector3(0, 0, Chunk.ChunkWidth));

	            lock (_collisions)
	            {
	                _collisions.Clear();
	                _collisions.AddRange(World.GlobalColliders);
	                try
	                {
	                    if (_underChunk != null && _underChunk.Initialized)
	                        _collisions.AddRange(_underChunk.CollisionShapes);


	                    var player = LocalPlayer.Instance;

	                    if (player?.NearCollisions != null)
	                        _collisions.AddRange(player.NearCollisions);

                        if (_underChunkL != null && _underChunkL.Initialized)
	                        _collisions.AddRange(_underChunkL.CollisionShapes);
	                    if (_underChunkR != null && _underChunkR.Initialized)
	                        _collisions.AddRange(_underChunkR.CollisionShapes);
	                    if (_underChunkF != null && _underChunkF.Initialized)
	                        _collisions.AddRange(_underChunkF.CollisionShapes);
	                    if (_underChunkB != null && _underChunkB.Initialized)
	                        _collisions.AddRange(_underChunkB.CollisionShapes);          

	                }
	                catch (Exception e)
	                {
	                    Log.WriteLine("Catched a sync error." + Environment.NewLine + e);
	                }
	            }
	        }

	        Velocity += -Physics.Gravity * GravityDirection * _deltaTime * 6f;
	        Velocity = Mathf.Clamp(Velocity, -VelocityCap, VelocityCap);

            var command = new MoveCommand(Parent, Velocity);
	        this.ProccessCommand(command);

	        if (!Parent.IsGrounded)
	        {
	            if (!Parent.IsUnderwater)
	                FallTime += (float)Time.deltaTime;
	        }
	        else
	        {
	            if (FallTime != 0)
	            {
	                float Exp;
	                if (FallTime > 1.75f && HasFallDamage)
	                {
	                    Parent.Damage(FallTime * 45f, null, out Exp, true);
	                    Parent.KnockForSeconds(3f);
	                }
	                FallTime = 0;
	            }
	        }

            Parent.Model.Position = Mathf.Lerp(Parent.Model.Position, this.TargetPosition, _deltaTime * 8f);
	    }
		
		public override void Draw(){}
		
		public float CollisionSize => HitboxSize * Parent.Model.Scale.Average();

	    public void Move(MoveCommand command){
			Physics.Manager.AddCommand(command);
		}

	    public void Move(Vector3 v)
	    {
	        Physics.Manager.AddCommand(new MoveCommand(this.Parent, v));
	    }

        public void ProccessCommand(MoveCommand Command) {
            if(Command.Delta == Vector3.Zero) return;
			Vector3 v = Command.Delta;
            bool onlyY = Command.Delta.Xz == Vector2.Zero;
			Vector3 delta = v * _deltaTime;
            var parentBox = this.Parent.HitBox;
			float modifierX = delta.X < 0 ? -1f : 1f;
			float modifierZ = delta.Z < 0 ? -1f : 1f;

            bool blockPx = false, blockNx = false, blockPy = false, blockNy = false, blockPz = false, blockNz = false;

            Block nextBlock =
                World.GetBlockAt(new Vector3(1f * modifierX, 0, 1f * modifierZ) + delta +
                                    this.Parent.BlockPosition);

            if (!onlyY)
            {
                if (nextBlock.Type != BlockType.Air && nextBlock.Type != BlockType.Water)
                {

                    Block nextBlockY = World.GetBlockAt(new Vector3(1f * modifierX, 2.75f, 1f * modifierZ) + delta +
                                                        this.Parent.BlockPosition);
                    if (nextBlockY.Type != BlockType.Air && nextBlockY.Type != BlockType.Water)
                    {
                        if (delta.X > 0)
                            blockPx = true;

                        if (delta.X < 0)
                            blockNx = true;

                        if (delta.Z < 0)
                            blockNz = true;

                        if (delta.Z > 0)
                            blockPz = true;
                    }
                }
            }
            
            if (this.HasCollision && !Command.IsRecursive)
            {
                for (int i = World.Entities.Count - 1; i > -1; i--)
                {
                    if (World.Entities[i] == Parent)
                        continue;

                    if (World.Entities[i].Physics.HasCollision &&
                        Physics.Collides(World.Entities[i].HitBox, parentBox))
                    {
                        if (!PushAround) return;
                        
                        Vector3 increment = -(Parent.Position.Xz - World.Entities[i].Position.Xz).ToVector3();
                        increment = increment.Xz.NormalizedFast().ToVector3();
                        var command = new MoveCommand(World.Entities[i], increment * 2f);
                        command.IsRecursive = true;

                        World.Entities[i].Physics.Move(command);
                        return;
                    }
                }
            }
  
			lock(_collisions)
			{
			    Vector3 deltaOrientation = delta.NormalizedFast();

				for(int i = _collisions.Count-1; i > -1; i--){

				    Box box = parentBox.Cache;

				    if (!onlyY)
				    {
				        box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 1f;
				        box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 2f + Vector3.UnitY;
				    }
				    else
				    {
				        if (delta.Y < 0)
				        {
				            box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 1f -
				                      Vector3.UnitX * .5f - Vector3.UnitZ * .5f;
				            box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 2f +
				                      Vector3.UnitX * .5f + Vector3.UnitZ * .5f;
				        }
				        else
				        {
				            box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * (Parent.DefaultBox.Max.Y-1f) -
				                      Vector3.UnitX * .5f - Vector3.UnitZ * .5f;
				            box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * (Parent.DefaultBox.Max.Y) +
				                      Vector3.UnitX * .5f + Vector3.UnitZ * .5f;
                        }
				    }

				    if (!Physics.Collides(box, _collisions[i])) continue;

				    if (deltaOrientation.X > 0)
				        blockPx = true;

				    if (deltaOrientation.X < 0)
				        blockNx = true;

				    if (deltaOrientation.Z < 0)
				        blockNz = true;

				    if (deltaOrientation.Z > 0)
				        blockPz = true;

				    if (deltaOrientation.Y > 0)
				        blockPy = true;

				    if (deltaOrientation.Y < 0)
				        blockNy = true;

				    if (!onlyY)
				    {

				        box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 2f 
                            + (parentBox.Max.Y - parentBox.Min.Y) * .5f * Vector3.UnitY;
				        box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 4f 
                            + (parentBox.Max.Y - parentBox.Min.Y) * 1.0f * Vector3.UnitY;

				        if (!Physics.Collides(box, _collisions[i]) && !blockPy)
				        {
				            Parent.BlockPosition += Vector3.UnitY * (parentBox.Max.Y - parentBox.Min.Y) * .05f;
				            if (deltaOrientation.X > 0)
				                blockPx = false;

				            if (deltaOrientation.X < 0)
				                blockNx = false;

				            if (deltaOrientation.Z < 0)
				                blockNz = false;

				            if (deltaOrientation.Z > 0)
				                blockPz = false;

				        }
				    }

                    var human = Parent as Humanoid;
					if(human != null && human.IsGliding){
						human.IsGliding = false;
						Parent.KnockForSeconds(3f);
					}
				}
			}
		    if (onlyY)
		    {
		        float lowestY = Physics.LowestHeight((int)Parent.BlockPosition.X, (int)Parent.BlockPosition.Z);
		        if (Parent.BlockPosition.Y * Chunk.BlockSize < lowestY)
		        {
		            Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, (lowestY + Parent.Model.Height + BaseHeight) / Chunk.BlockSize, Parent.BlockPosition.Z);
		        }

                if (!blockNy && delta.Y < 0 || !blockNy && Parent.IsUnderwater)
		        {
		            Block underBlock = World.GetBlockAt(Parent.BlockPosition - Vector3.UnitY);
		            Block underUnderBlock = World.GetBlockAt(Parent.BlockPosition - Vector3.UnitY * 2f);
		            if (Parent.IsUnderwater)
		            {
		                Parent.IsGrounded = false;
                    }
                    else if (underBlock.Type != BlockType.Air && underBlock.Type != BlockType.Water && underUnderBlock.Type != BlockType.Air && underUnderBlock.Type != BlockType.Water)
		            {

                        float heightAtPositon = Physics.HeightAtBlock(Parent.BlockPosition - Vector3.UnitY);
		                Parent.BlockPosition = new Vector3(Parent.BlockPosition.X,
		                    (heightAtPositon + Parent.Model.Height + BaseHeight) / Chunk.BlockSize,
		                    Parent.BlockPosition.Z);	                

		                Parent.IsGrounded = true;
		                blockNy = true;
		                Velocity = Vector3.Zero;
                    }
		            else
		            {
		                
                        Parent.IsGrounded = false;
		            }

		        }
		        else if(blockNy)
		        {
                    Parent.IsGrounded = true;
		            Velocity = Vector3.Zero;
		        }
		    }
		    

			Vector3 prevPosition = Parent.BlockPosition;
			Parent.BlockPosition += delta * new Vector3(1,1/Chunk.BlockSize,1);
				
			
			if( (blockNz || _wasBlockNz) && (Parent.BlockPosition.Z - prevPosition.Z) < 0)
				Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, Parent.BlockPosition.Y, prevPosition.Z);
			
			if( (blockPz || _wasBlockPz) && (Parent.BlockPosition.Z - prevPosition.Z) > 0)
				Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, Parent.BlockPosition.Y, prevPosition.Z);
			
			if( (blockNx || _wasBlockNx) && (Parent.BlockPosition.X - prevPosition.X) < 0)
				Parent.BlockPosition = new Vector3(prevPosition.X, Parent.BlockPosition.Y, Parent.BlockPosition.Z);
			
			if( (blockPx || _wasBlockPx) && (Parent.BlockPosition.X - prevPosition.X) > 0)
				Parent.BlockPosition = new Vector3(prevPosition.X, Parent.BlockPosition.Y, Parent.BlockPosition.Z);
			
			if( (blockNy || _wasBlockNy) && (Parent.BlockPosition.Y - prevPosition.Y) < 0)
			    Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, prevPosition.Y, Parent.BlockPosition.Z);

            if ((blockPy || _wasBlockPy) && (Parent.BlockPosition.Y - prevPosition.Y) > 0)
                Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, prevPosition.Y, Parent.BlockPosition.Z);
                
            

            _wasBlockNz = blockNz;
            _wasBlockNx = blockNx;
            _wasBlockNy = blockNy;
            _wasBlockPz = blockPz;
            _wasBlockPx = blockPx;
            _wasBlockPy = blockPy;
        }
		
		public void ResetFall(){
		    FallTime = 0.01f;
		}
		
		public override void Dispose(){
		    lock (_collisions)
                this._collisions.Clear();	
		    this._underChunk = null;
			this._underChunkR = null;
			this._underChunkL = null;
			this._underChunkF = null;
			this._underChunkB = null;
			
			this.Disposed = true;
		}
	}
}