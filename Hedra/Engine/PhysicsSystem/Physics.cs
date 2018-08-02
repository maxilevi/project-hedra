/*
 * Author: Zaphyk
 * Date: 11/02/2016
 * Time: 06:29 p.m.
 *
 */
using System;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using System.Collections.Generic;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PhysicsSystem
{
	public static class Physics
	{
		public const float Gravity = -9.81f;
	    public const float Timestep = 1.0f / 60.0f;

        public static PhysicsThreadManager Threading;

	    static Physics()
	    {
	        Threading = new PhysicsThreadManager();
        }

        public static void Update()
	    {
	        Threading.Update();
	    }

	    public static Box BuildBroadphaseBox(VertexData Model)
	    {
	        var offset = new Vector3(
	            (Model.SupportPoint(Vector3.UnitX).X + Model.SupportPoint(-Vector3.UnitX).X) * .5f,
	            0,
	            (Model.SupportPoint(Vector3.UnitZ).Z + Model.SupportPoint(-Vector3.UnitZ).Z) * .5f
	        );
	        var minus = Math.Min(Model.SupportPoint(-Vector3.UnitX).X - offset.X, Model.SupportPoint(-Vector3.UnitZ).Z - offset.Z);
	        var plus = Math.Max(Model.SupportPoint(Vector3.UnitX).X - offset.X, Model.SupportPoint(Vector3.UnitZ).Z - offset.Z);
	        return new Box(
	            new Vector3(minus, Model.SupportPoint(-Vector3.UnitY).Y, minus),
	            new Vector3(plus, Model.SupportPoint(Vector3.UnitY).Y, plus)
	        );
        }

	    public static Box BuildDimensionsBox(VertexData Model)
	    {
	        return new Box(
	            new Vector3(Model.SupportPoint(-Vector3.UnitX).X, Model.SupportPoint(-Vector3.UnitY).Y, Model.SupportPoint(-Vector3.UnitZ).Z),
	            new Vector3(Model.SupportPoint(Vector3.UnitX).X, Model.SupportPoint(Vector3.UnitY).Y, Model.SupportPoint(Vector3.UnitZ).Z)
            );
	    }


        public static void LookAt(Entity Parent, Entity Target){
		    Parent.Orientation = (Target.Model.Position-Parent.Model.Position).Xz.NormalizedFast().ToVector3();
            Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
		}
		
		public static Vector3 DirectionToEuler( Vector3 Direction)
        {
            Matrix4 mv = Mathf.RotationAlign(Direction.Z < 0 ? -Vector3.UnitZ : Vector3.UnitZ, Direction);
            var modifier = new Vector3(0, Direction.Z < 0 ? 180 : 0, 0);
            var multiplier = new Vector3(Direction.Z < 0 ? -1f : 1, 1, 1);
            mv.ExtractRotation().ToAxisAngle(out Vector3 axis, out float angle);
            if(float.IsNaN(angle)) return Vector3.Zero;
            return axis * angle * Mathf.Degree * multiplier + modifier;
        }

        public static float HeightAtPosition(float X, float Z){
			 return HeightAtPosition(new Vector3(X,0,Z));
		}
		
		public static float HeightAtPosition(Vector3 BlockPosition){
			
			if(World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Noise3D){
				return HeightAtBlock( new Vector3(BlockPosition.X, World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z), BlockPosition.Z) );
			}
			
			var densityX = World.GetHighestBlockAt(  (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z ).Density;
			var densityZ = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			var densityXz = World.GetHighestBlockAt( (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			var density = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Density;

			var yx = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
			var yz = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			var yxz = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			var yh = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z);
				
			Vector3 blockSpace = World.ToBlockSpace(BlockPosition);
			var coords = new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize , Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize;
			
			var bottom = new Vector3(blockSpace.X, yh + density, blockSpace.Z);
			var right = new Vector3(blockSpace.X+1, yx + densityX, blockSpace.Z);
			var top = new Vector3(blockSpace.X+1, yxz + densityXz, blockSpace.Z+1);
			var front = new Vector3(blockSpace.X, yz + densityZ, blockSpace.Z+1);

		    float height1 = coords.X < 1 - coords.Y
		        ? Mathf.BarryCentric(new Vector3(0, bottom.Y, 0), new Vector3(1, right.Y, 0), new Vector3(0, front.Y, 1), coords)
		        : Mathf.BarryCentric(new Vector3(1, right.Y, 0), new Vector3(1, top.Y, 1), new Vector3(0, front.Y, 1), coords);

		    coords = new Vector2(coords.X < 0 ? -coords.X : coords.X, coords.Y < 0 ? -coords.Y : coords.Y);

		    float height0 = coords.X < 1 - coords.Y
		        ? Mathf.BarryCentric(new Vector3(0, bottom.Y, 0), new Vector3(1, right.Y, 0), new Vector3(0, front.Y, 1), coords)
		        : Mathf.BarryCentric(new Vector3(1, right.Y, 0), new Vector3(1, top.Y, 1), new Vector3(0, front.Y, 1), coords);

		    return (height0 + height1) * .5f * Chunk.BlockSize;
        }
		
		public static float HeightAtBlock(Vector3 BlockPosition){
			
			Chunk UnderChunk = World.GetChunkAt(BlockPosition);  
			
			/*if( World.GetNearestBlockAt( (int)BlockPosition.X, (int) BlockPosition.Y+1, (int)BlockPosition.Z).Noise3D ){
				float Nearest = UnderChunk.NearestVertex( BlockPosition * new Vector3(1,Chunk.BlockSize,1) + Vector3.UnitY * Chunk.BlockSize).Y;
				return Nearest;
			}*/


		    var densityX = World.GetNearestBlockAt((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Y, (int)BlockPosition.Z).Density;
		    var densityZ = World.GetNearestBlockAt((int)BlockPosition.X, (int)BlockPosition.Y, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
		    var densityXz = World.GetNearestBlockAt((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Y, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
		    var density = World.GetNearestBlockAt((int)BlockPosition.X, (int)BlockPosition.Y, (int)BlockPosition.Z).Density;

            var yx = (int) BlockPosition.Y;
            var yz = (int)BlockPosition.Y;
            var yxz = (int)BlockPosition.Y;
            var yh = (int)BlockPosition.Y;

            var blockSpace = World.ToBlockSpace(BlockPosition);
		    var coords = new Vector2(BlockPosition.X % Chunk.BlockSize, BlockPosition.Z % Chunk.BlockSize) / Chunk.BlockSize;

		    var bottom = new Vector3(blockSpace.X, yh + density, blockSpace.Z);
		    var right = new Vector3(blockSpace.X + 1, yx + densityX, blockSpace.Z);
		    var top = new Vector3(blockSpace.X + 1, yxz + densityXz, blockSpace.Z + 1);
		    var front = new Vector3(blockSpace.X, yz + densityZ, blockSpace.Z + 1);

		    coords = new Vector2(coords.X < 0 ? -coords.X : coords.X, coords.Y < 0 ? -coords.Y : coords.Y);

            float height0 = coords.X < 1 - coords.Y
                ? Mathf.BarryCentric(new Vector3(0, bottom.Y, 0), new Vector3(1, right.Y, 0), new Vector3(0, front.Y, 1), coords) 
                : Mathf.BarryCentric(new Vector3(1, right.Y, 0), new Vector3(1, top.Y, 1), new Vector3(0, front.Y, 1), coords);

		    return height0 * Chunk.BlockSize + (BlockPosition.X < 0 || BlockPosition.Z < 0 ? .25f : 0);
        }

	    public static float LowestHeight(float X, float Z)
	    {
	        return LowestHeight(new Vector3(X, 0, Z));
	    }
        public static float LowestHeight(Vector3 BlockPosition)
        {

            Chunk UnderChunk = World.GetChunkAt(BlockPosition);

            if (World.GetNearestBlockAt((int)BlockPosition.X, (int)BlockPosition.Y + 1, (int)BlockPosition.Z).Noise3D)
            {
                float Nearest = UnderChunk.NearestVertex(BlockPosition * new Vector3(1, Chunk.BlockSize, 1) + Vector3.UnitY * Chunk.BlockSize).Y;
                return Nearest;
            }


            float DensityX = World.GetLowestBlock((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Z).Density;
            float DensityZ = World.GetLowestBlock((int)BlockPosition.X, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
            float DensityXZ = World.GetLowestBlock((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
            float Density = World.GetLowestBlock((int)BlockPosition.X, (int)BlockPosition.Z).Density;

            float lowestY = World.GetLowestY((int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
            float YX = lowestY;
            float YZ = lowestY;
            float YXZ = lowestY;
            float YH = lowestY;


            Vector3 BlockSpace = World.ToBlockSpace(BlockPosition);
            Vector2 Coords = new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize, Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize;

            Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
            Vector3 Right = new Vector3(BlockSpace.X + 1, YX + DensityX, BlockSpace.Z);
            Vector3 Top = new Vector3(BlockSpace.X + 1, YXZ + DensityXZ, BlockSpace.Z + 1);
            Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z + 1);

            float Height = 0;
            if (Coords.X < 1 - Coords.Y)
                Height = Mathf.BarryCentric(new Vector3(0, Bottom.Y, 0), new Vector3(1, Right.Y, 0), new Vector3(0, Front.Y, 1), Coords);
            else
                Height = Mathf.BarryCentric(new Vector3(1, Right.Y, 0), new Vector3(1, Top.Y, 1), new Vector3(0, Front.Y, 1), Coords);

            return Height * Chunk.BlockSize;
        }

        public static Vector3 NormalAtPosition(float X, float Z){
			return NormalAtPosition(new Vector3(X,0,Z));
		}
		
		public static Vector3 NormalAtPosition(Vector3 BlockPosition){
			Chunk UnderChunk = World.GetChunkAt(BlockPosition);
			
			float DensityX = World.GetHighestBlockAt(  (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z ).Density;
			float DensityZ = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			float DensityXZ = World.GetHighestBlockAt( (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			float Density = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Density;
			
			float YX = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
			float YZ = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YXZ = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YH = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z);
			
			
			Vector3 BlockSpace = World.ToBlockSpace(BlockPosition);
			Vector2 Coords = new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize , Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize;
			
			Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
			Vector3 Right = new Vector3(BlockSpace.X+1, YX + DensityX, BlockSpace.Z);
			Vector3 Top = new Vector3(BlockSpace.X+1, YXZ + DensityXZ, BlockSpace.Z+1);
			Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z+1);
			
			Vector3 Normal = Vector3.Zero;
			if(Coords.X < 1-Coords.Y)
				Normal = Mathf.CalculateNormal(Bottom, Right, Front);
			else
				Normal = Mathf.CalculateNormal(Right, Top, Front);
			
			return -Normal;
		}
		
		public static bool IsColliding(Vector3 Position, Box Hitbox){
			Chunk UnderChunk = World.GetChunkAt(Position);
			Chunk UnderChunkR = World.GetChunkAt(Position + new Vector3(Chunk.Width,0, 0));
			Chunk UnderChunkL = World.GetChunkAt(Position - new Vector3(Chunk.Width,0, 0));
			Chunk UnderChunkF = World.GetChunkAt(Position + new Vector3(0,0,Chunk.Width));
			Chunk UnderChunkB = World.GetChunkAt(Position - new Vector3(0,0,Chunk.Width));
			
			List<ICollidable> Collisions = new List<ICollidable>();
			Collisions.AddRange(World.GlobalColliders);
			
			try{
				if(UnderChunk != null)
					Collisions.AddRange(UnderChunk.CollisionShapes);
				if(UnderChunkL != null)
					Collisions.AddRange(UnderChunkL.CollisionShapes);
				if(UnderChunkR != null)
					Collisions.AddRange(UnderChunkR.CollisionShapes);
				if(UnderChunkF != null)
					Collisions.AddRange(UnderChunkF.CollisionShapes);
				if(UnderChunkB != null)
					Collisions.AddRange(UnderChunkB.CollisionShapes);
			}catch(IndexOutOfRangeException e){
				Log.WriteLine(e.ToString());
			}
				
			for(int i = 0; i < Collisions.Count; i++){
				if(Physics.Collides(Collisions[i], Hitbox))
					return true;
			}
			return false;
		}
		
		public static bool Collides(ICollidable Obj1, ICollidable Obj2)
		{
		    var obj1Box = Obj1 as Box;
		    var obj2Box = Obj2 as Box;

            if (obj1Box != null && obj2Box != null)
				return Physics.AABBvsAABB(obj1Box, obj2Box);
			
			if(obj1Box == null && obj2Box == null)
				return GJKCollision.Collides(Obj1 as CollisionShape, Obj2 as CollisionShape);
			
			if(obj1Box == null)
				return GJKCollision.Collides(Obj1 as CollisionShape, obj2Box.ToShape() );

		    return GJKCollision.Collides(obj1Box.ToShape(), Obj2 as CollisionShape);
		}

		public static bool AABBvsAABB(Box a, Box b) {
			return a.Min.X  <= b.Max.X && a.Max.X >= b.Min.X &&
                a.Min.Y  <= b.Max.Y && a.Max.Y >= b.Min.Y &&
                a.Min.Z  <= b.Max.Z && a.Max.Z  >= b.Min.Z;
		}
		
		public static bool AABBvsPoint(Box a, Vector3 P) {
		  return P.X >= a.Min.X && P.X <= a.Max.X &&
                P.Y >= a.Min.Y && P.Y <= a.Max.Y &&
                P.Z >= a.Min.Y && P.Z <= a.Max.Z;
		}
	}		
}
