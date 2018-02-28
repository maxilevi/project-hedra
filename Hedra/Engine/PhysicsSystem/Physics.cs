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

namespace Hedra.Engine.PhysicsSystem
{
	public static class Physics
	{
		public static float Gravity = -9.81f;
		public static PhysicsThreadManager Manager = new PhysicsThreadManager();
		
		public static Vector3 LookAt(Entity Parent, Entity Target){
			Vector3 Dir = (Target.BlockPosition-Parent.BlockPosition).Normalized();
			Parent.Orientation = Dir.Xz.Normalized().ToVector3();
			 
			Matrix4 MV = Mathf.RotationAlign(Vector3.UnitZ, Parent.Orientation);
			Vector3 Axis;
			float Angle;
			MV.ExtractRotation().ToAxisAngle(out Axis, out Angle);	
			return new Vector3(Parent.Model.TargetRotation.X, Axis.Y * Angle * Mathf.Degree, Parent.Model.TargetRotation.Z);
		}
		
		public static Vector3 DirectionToEuler( Vector3 Direction){
			Matrix4 MV = Mathf.RotationAlign(Vector3.UnitZ, Direction);
			Vector3 Axis;
			float Angle;
			MV.ExtractRotation().ToAxisAngle(out Axis, out Angle);	
            if(float.IsNaN(Angle)) return Vector3.Zero;

			return Axis * Angle * Mathf.Degree * Vector3.UnitY;
		}
		
		public static float HeightAtPosition(float X, float Z){
			 return HeightAtPosition(new Vector3(X,0,Z));
		}
		
		public static float HeightAtPosition(Vector3 BlockPosition){
			
			Chunk UnderChunk = World.GetChunkAt(BlockPosition);
			
			if(World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Noise3D){
				return HeightAtBlock( new Vector3(BlockPosition.X, World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z), BlockPosition.Z) );
			}
			
			float DensityX = World.GetHighestBlockAt(  (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z ).Density;
			float DensityZ = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			float DensityXZ = World.GetHighestBlockAt( (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z + (int) Chunk.BlockSize ).Density;
			float Density = World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Density;
			
			float YX = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
			float YZ = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YXZ = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YH = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z);
			
			
			Vector3 BlockSpace = World.ToBlockSpace(BlockPosition);
			Vector2 Coords = (new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize , Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize);
			
			Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
			Vector3 Right = new Vector3(BlockSpace.X+1, YX + DensityX, BlockSpace.Z);
			Vector3 Top = new Vector3(BlockSpace.X+1, YXZ + DensityXZ, BlockSpace.Z+1);
			Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z+1);
			
			float Height = 0;
			if(Coords.X < (1-Coords.Y))
				Height = Mathf.BarryCentric(new Vector3(0,Bottom.Y,0), new Vector3(1,Right.Y,0), new Vector3(0,Front.Y,1), Coords);
			else
				Height = Mathf.BarryCentric(new Vector3(1,Right.Y,0), new Vector3(1,Top.Y,1), new Vector3(0,Front.Y,1), Coords);
			
			return Height * Chunk.BlockSize;
		}
		
		public static float HeightAtBlock(Vector3 BlockPosition){
			
			Chunk UnderChunk = World.GetChunkAt(BlockPosition);  
			
			/*if( World.GetNearestBlockAt( (int)BlockPosition.X, (int) BlockPosition.Y+1, (int)BlockPosition.Z).Noise3D ){
				float Nearest = UnderChunk.NearestVertex( BlockPosition * new Vector3(1,Chunk.BlockSize,1) + Vector3.UnitY * Chunk.BlockSize).Y;
				return Nearest;
			}*/


		    float DensityX = World.GetNearestBlockAt((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Y, (int)BlockPosition.Z).Density;
		    float DensityZ = World.GetNearestBlockAt((int)BlockPosition.X, (int)BlockPosition.Y, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
		    float DensityXZ = World.GetNearestBlockAt((int)BlockPosition.X + (int)Chunk.BlockSize, (int)BlockPosition.Y, (int)BlockPosition.Z + (int)Chunk.BlockSize).Density;
		    float Density = World.GetNearestBlockAt((int)BlockPosition.X, (int)BlockPosition.Y, (int)BlockPosition.Z).Density;

		    float YX = (int) BlockPosition.Y;
            float YZ = (int)BlockPosition.Y;
            float YXZ = (int)BlockPosition.Y;
            float YH = (int)BlockPosition.Y;


            Vector3 BlockSpace = World.ToBlockSpace(BlockPosition);
		    Vector2 Coords = (new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize, Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize);

		    Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
		    Vector3 Right = new Vector3(BlockSpace.X + 1, YX + DensityX, BlockSpace.Z);
		    Vector3 Top = new Vector3(BlockSpace.X + 1, YXZ + DensityXZ, BlockSpace.Z + 1);
		    Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z + 1);

		    float Height = 0;
		    if (Coords.X < (1 - Coords.Y))
		        Height = Mathf.BarryCentric(new Vector3(0, Bottom.Y, 0), new Vector3(1, Right.Y, 0), new Vector3(0, Front.Y, 1), Coords);
		    else
		        Height = Mathf.BarryCentric(new Vector3(1, Right.Y, 0), new Vector3(1, Top.Y, 1), new Vector3(0, Front.Y, 1), Coords);

		    return Height * Chunk.BlockSize;
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
            Vector2 Coords = (new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize, Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize);

            Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
            Vector3 Right = new Vector3(BlockSpace.X + 1, YX + DensityX, BlockSpace.Z);
            Vector3 Top = new Vector3(BlockSpace.X + 1, YXZ + DensityXZ, BlockSpace.Z + 1);
            Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z + 1);

            float Height = 0;
            if (Coords.X < (1 - Coords.Y))
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
			
			float DensityX = (World.GetHighestBlockAt(  (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z )).Density;
			float DensityZ = (World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z + (int) Chunk.BlockSize )).Density;
			float DensityXZ = (World.GetHighestBlockAt( (int)BlockPosition.X + (int) Chunk.BlockSize, (int)BlockPosition.Z + (int) Chunk.BlockSize )).Density;
			float Density = (World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z)).Density;
			
			float YX = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
			float YZ = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YXZ = World.GetHighestY( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z + (int) Chunk.BlockSize);
			float YH = World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z);
			
			
			Vector3 BlockSpace = World.ToBlockSpace(BlockPosition);
			Vector2 Coords = (new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize , Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize);
			
			Vector3 Bottom = new Vector3(BlockSpace.X, YH + Density, BlockSpace.Z);
			Vector3 Right = new Vector3(BlockSpace.X+1, YX + DensityX, BlockSpace.Z);
			Vector3 Top = new Vector3(BlockSpace.X+1, YXZ + DensityXZ, BlockSpace.Z+1);
			Vector3 Front = new Vector3(BlockSpace.X, YZ + DensityZ, BlockSpace.Z+1);
			
			Vector3 Normal = Vector3.Zero;
			if(Coords.X < (1-Coords.Y))
				Normal = Mathf.CalculateNormal(Bottom, Right, Front);
			else
				Normal = Mathf.CalculateNormal(Right, Top, Front);
			
			return -Normal;
		}
		
		public static bool IsColliding(Vector3 Position, Box Hitbox){
			Chunk UnderChunk = World.GetChunkAt(Position);
			Chunk UnderChunkR = World.GetChunkAt(Position + new Vector3(Chunk.ChunkWidth,0, 0));
			Chunk UnderChunkL = World.GetChunkAt(Position - new Vector3(Chunk.ChunkWidth,0, 0));
			Chunk UnderChunkF = World.GetChunkAt(Position + new Vector3(0,0,Chunk.ChunkWidth));
			Chunk UnderChunkB = World.GetChunkAt(Position - new Vector3(0,0,Chunk.ChunkWidth));
			
			List<ICollidable> Collisions = new List<ICollidable>();
			Collisions.AddRange(World.GlobalColliders.ToArray());
			
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
		
		public static bool Collides(ICollidable Obj1, ICollidable Obj2){
			
			if( Obj1 is Box && Obj2 is Box)
				return Physics.AABBvsAABB(Obj1 as Box, Obj2 as Box);
			
			if( Obj1 is CollisionShape && Obj2 is CollisionShape)
				return GJKCollision.Collides(Obj1 as CollisionShape, Obj2 as CollisionShape);
			
			if(Obj1 is CollisionShape && Obj2 is Box)
				return GJKCollision.Collides(Obj1 as CollisionShape, (Obj2 as Box).ToShape() );

		    if (Obj1 is Box && Obj2 is CollisionShape)
		        return GJKCollision.Collides((Obj1 as Box).ToShape(), Obj2 as CollisionShape);
            return false;
		}

		public static bool AABBvsAABB(Box a, Box b) {
			return (a.Min.X  <= b.Max.X && a.Max.X >= b.Min.X) &&
			   (a.Min.Y  <= b.Max.Y && a.Max.Y >= b.Min.Y) &&
	 		   (a.Min.Z  <= b.Max.Z && a.Max.Z  >= b.Min.Z);
		}
		
		public static bool AABBvsPoint(Box a, Vector3 P) {
		  return (P.X >= a.Min.X && P.X <= a.Max.X) &&
		         (P.Y >= a.Min.Y && P.Y <= a.Max.Y) &&
		         (P.Z >= a.Min.Y && P.Z <= a.Max.Z);
		}
	}		
}
