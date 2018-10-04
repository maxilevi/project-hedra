/*
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
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    public delegate bool OnHitGroundEvent(IEntity Parent, float Falltime);

    public class  PhysicsComponent : EntityComponent
	{
		private readonly Entity _parent;
		public event OnHitGroundEvent OnHitGround;
		public bool UsePhysics { get; set; }
	    public float Falltime { get; private set; }
	    public bool CanBePushed { get; set; } = true;
        public Vector3 GravityDirection = new Vector3(0,-1f,0);
		public float VelocityCap = float.MaxValue;
		public Vector3 Force = Vector3.Zero;
		public Vector3 Velocity = Vector3.Zero;
		public bool HasFallDamage = true;
		public bool UseTimescale { get; set; }
		public bool InFrontOfWall { get; private set; }
        public bool IsDrifting { get; private set; }

	    public PhysicsComponent(Entity Parent) : base(Parent)
	    {
		    _parent = Parent;
		    UsePhysics = true;
		    UseTimescale = true;
	    }

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
            get => Parent.BlockPosition * new Vector3(1,Chunk.BlockSize,1);
	        set => Parent.BlockPosition = new Vector3(value.X, value.Y / Chunk.BlockSize, value.Z);
	    }

	    public override void Update(){
			if(!UsePhysics)
				return;

			_deltaTime = this.Timestep;

	        if (CanCollide)
	        {

	            _underChunk = World.GetChunkAt(Parent.Position);
	            _underChunkR = World.GetChunkAt(Parent.Position + new Vector3(Chunk.Width, 0, 0));
	            _underChunkL = World.GetChunkAt(Parent.Position - new Vector3(Chunk.Width, 0, 0));
	            _underChunkF = World.GetChunkAt(Parent.Position + new Vector3(0, 0, Chunk.Width));
	            _underChunkB = World.GetChunkAt(Parent.Position - new Vector3(0, 0, Chunk.Width));

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

	        var modifier = 40f * (1f / (float) Time.Frametime);
            Velocity += -Physics.Gravity * GravityDirection * _deltaTime * modifier;
	        Velocity = Mathf.Clamp(Velocity, -VelocityCap, VelocityCap);

            var command = new MoveCommand(Parent, Velocity * _deltaTime);
	        this.ProccessCommand(command);

	        if (!Parent.IsGrounded)
	        {
	            if (!Parent.IsUnderwater)
	                Falltime += _deltaTime * 10f / (float)Time.Frametime;
	        }
	        else
	        {
	            if (Falltime > 0)
	            {
	                if (Falltime > 1.75f && HasFallDamage && (OnHitGround?.Invoke(this.Parent, Falltime) ?? true) )
	                {
	                    if (!Parent.SearchComponent<DamageComponent>()?.Immune ?? true)
	                    {
	                        var fallTime = Falltime;
	                        Executer.ExecuteOnMainThread(delegate
	                        {
	                            Parent.Damage(fallTime * 45f, null, out _, true);
	                            Parent.KnockForSeconds(3f);
	                        });
	                    }
	                }
	                Falltime = 0;
	            }
	        }
	        Parent.Model.Position = Mathf.Lerp(Parent.Model.Position, this.TargetPosition, _deltaTime * 8f);
	    }

	    public void ResetVelocity()
	    {
	        Velocity = Vector3.Zero;
	    }

	    public void ExecuteTranslate(MoveCommand Command)
        {
			Physics.Threading.AddCommand(Command);
        }

	    public void Translate(Vector3 Delta)
	    {
	        this.ExecuteTranslate(new MoveCommand(this.Parent, Delta));
	    }

        public void DeltaTranslate(Vector3 Delta)
	    {
	        this.ExecuteTranslate(new MoveCommand(this.Parent, Delta * Time.DeltaTime));
	    }

	    public void DeltaTranslate(MoveCommand Command)
	    {
	        Command.Delta *= this.Timestep;
            this.ExecuteTranslate(Command);
	    }

	    public float Timestep => Time.IndependantDeltaTime * (UseTimescale ? Time.TimeScale : 1);

        public void ProccessCommand(MoveCommand Command)
        {
            if(Command.Delta == Vector3.Zero) return;
            bool onlyY = Command.Delta.Xz == Vector2.Zero;
			Vector3 delta = Command.Delta;
            var parentBox = this.Parent.Model.BroadphaseBox;
			float modifierX = delta.X < 0 ? -1f : 1f;
			float modifierZ = delta.Z < 0 ? -1f : 1f;

            bool blockPx = false, blockNx = false, blockPy = false, blockNy = false, blockPz = false, blockNz = false;
      
            if (!onlyY)
            {
                var nextBlock =
                    World.GetBlockAt(new Vector3(1f * modifierX, 0, 1f * modifierZ) + delta +
                                        this.Parent.BlockPosition);
                bool IsSolid(Block B) => B.Type != BlockType.Air && B.Type != BlockType.Water;
                var calcPosition = new Vector3(1f * modifierX, 2.5f, 1f * modifierZ) + delta +
                                    this.Parent.BlockPosition;
                var nextBlockY = World.GetBlockAt(calcPosition);
                var tNormal = Physics.NormalAtPosition(calcPosition);
                if (!IsDrifting)
                {
                    if (IsSolid(nextBlockY) || (Vector3.Dot(tNormal, Vector3.UnitY) < .35f && IsSolid(nextBlock)))
                    {
                        if (delta.X > 0)
                            blockPx = true;

                        if (delta.X < 0)
                            blockNx = true;

                        if (delta.Z < 0)
                            blockNz = true;

                        if (delta.Z > 0)
                            blockPz = true;

                        if (Parent is Humanoid human && human.IsClimbing)
                        {
                            delta += Vector3.UnitY * Time.DeltaTime * 60f;
                        }
                        InFrontOfWall = true;
                    }
                    else
                    {
                        InFrontOfWall = false;
                    }
                }
            }

            var terrainNormal = Physics.NormalAtPosition(Parent.Position);
            var dot = Vector3.Dot(terrainNormal, Vector3.UnitY);
            if (IsDrifting)
            {
                Parent.BlockPosition += terrainNormal.Xz.ToVector3() * Time.DeltaTime * 8;
                this.ResetFall();
                this.ResetVelocity();
                if (!onlyY) return;
            }

            if (dot < .35 && Parent.IsGrounded) IsDrifting = true;
            else if(dot > .45 || !Parent.IsGrounded) IsDrifting = false;


            if (this.HasCollision && !Command.IsRecursive)
            {
                var entities = World.Entities;
                for (int i = entities.Count - 1; i > -1; i--)
                {
                    if (entities[i] == Parent)
                        continue;

                    if (entities[i].Physics.HasCollision)
                    {
                        if (Physics.Collides(entities[i].Model.BroadphaseBox, this.Parent.Model.BroadphaseBox) 
                            && Physics.Collides(entities[i].Model.BroadphaseCollider, Parent.Model.BroadphaseCollider))
                        {
                            if (!PushAround || !entities[i].Physics.CanBePushed) return;
                            if (entities[i].Model.BroadphaseBox.Size.LengthSquared >
                                this.Parent.Model.BroadphaseBox.Size.LengthSquared * 4f)
                            {
                                if(Vector3.Dot(delta.NormalizedFast(), (entities[i].Position - this.Parent.Position).NormalizedFast()) > .75f) return;
                                else continue;
                            }
                            var increment = -(Parent.Position.Xz - entities[i].Position.Xz).ToVector3().NormalizedFast();
                            for (var j = 0; j < 6; j++)
                            {
                                var command = new MoveCommand(entities[i],
                                    increment * 1f)
                                {
                                    IsRecursive = true
                                };
                                entities[i].Physics.DeltaTranslate(command);
                            }
                        }
                    }
                }
            }
            var overFloor = false;
			lock(_collisions)
			{
			    Vector3 deltaOrientation = delta.NormalizedFast();

				for(int i = _collisions.Count-1; i > -1; i--)
                {
				    Box box = parentBox.Cache;
                    if (onlyY)
                    {
                        /*box = parentBox.Cache;
                        box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) - Vector3.UnitY * .25f;
                        box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + Vector3.UnitY * .25f;
                        if (_collisions[i].Height < Parent.Model.Height * .5)
                        {
                            if (Physics.Collides(box, _collisions[i]))
                            {
                                //Parent.BlockPosition += Vector3.UnitY * Parent.Model.Height * .05f;
                                //Parent.IsGrounded = false;
                            }
                        }*/
                    }

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
				            box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation 
                                * (Parent.Model.BaseBroadphaseBox.Max.Y-1f) -
				                      Vector3.UnitX * .5f - Vector3.UnitZ * .5f;
				            box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation 
                                * Parent.Model.BaseBroadphaseBox.Max.Y +
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
				        /*box.Min = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 1f;
				        box.Max = Parent.BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + deltaOrientation * 2f 
                            + (parentBox.Max.Y - parentBox.Min.Y) * 0.05f * Vector3.UnitY;

				        if (!Physics.Collides(box, _collisions[i]) && !blockPy)
				        {
				            Parent.BlockPosition += Vector3.UnitY * Parent.Model.Height * .05f;
				            if (deltaOrientation.X > 0)
				                blockPx = false;

				            if (deltaOrientation.X < 0)
				                blockNx = false;

				            if (deltaOrientation.Z < 0)
				                blockNz = false;

				            if (deltaOrientation.Z > 0)
				                blockPz = false;

				        }*/
				    }

                    if (Parent is Humanoid human && human.IsTravelling)
                    {
	                    human.IsTravelling = false;
						Parent.KnockForSeconds(3f);
				        Executer.ExecuteOnMainThread(delegate
				        {
				            Parent.Damage(Parent.MaxHealth * .15f, Parent, out float xp);
				        });
				    }
				}
			}
		    if (onlyY)
		    {
		        float heightAtPosition = Physics.HeightAtPosition((int)Parent.BlockPosition.X, (int)Parent.BlockPosition.Z);
		        if (Parent.BlockPosition.Y * Chunk.BlockSize < heightAtPosition)
		        {
		            Parent.BlockPosition = new Vector3(Parent.BlockPosition.X, heightAtPosition / Chunk.BlockSize,
		                Parent.BlockPosition.Z);
		            Parent.IsGrounded = true;
		            Velocity = Vector3.Zero;
		        }
		        else
		        {
		            Parent.IsGrounded = false;
		        }

                if ((!blockNy && delta.Y < 0 || !blockNy && Parent.IsUnderwater))
		        {
		            var underUnderBlock = World.GetBlockAt(Parent.BlockPosition - Vector3.UnitY * 2f);
		            var human = Parent as Humanoid;
                    if (Parent.IsUnderwater || (human?.IsJumping ?? false))
		            {
		                Parent.IsGrounded = false;
                    }
                    /*else if (underUnderBlock.Type != BlockType.Air && underUnderBlock.Type != BlockType.Water 
                        /*&& currentBlock.Type != BlockType.Air && currentBlock.Type != BlockType.Water)
		            {
                        float heightAtPositon = Physics.HeightAtBlock(Parent.BlockPosition - Vector3.UnitY);
		                /*Parent.BlockPosition = new Vector3(Parent.BlockPosition.X,
		                    (heightAtPositon + BaseHeight) / Chunk.BlockSize,
		                    Parent.BlockPosition.Z);
                        
		                Parent.IsGrounded = true;
		                blockNy = true;
		                Velocity = Vector3.Zero;
                    }*/

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
		
		public void ResetFall()
		{
		    Falltime = 0.01f;
		}
		
		public override void Dispose(){
		    lock (_collisions)
                this._collisions.Clear();	
		    this._underChunk = null;
			this._underChunkR = null;
			this._underChunkL = null;
			this._underChunkF = null;
			this._underChunkB = null;
		}
	}
}