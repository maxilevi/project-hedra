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
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.PhysicsSystem
{
    public static class Physics
    {
        public const float Gravity = -9.81f;
        public const float Timestep = 1.0f / 60.0f;

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


        public static void LookAt(IEntity Parent, IEntity Target){
            Parent.Orientation = (Target.Model.Position-Parent.Model.Position).Xz.NormalizedFast().ToVector3();
            Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
        }
        
        public static Vector3 DirectionToEuler(Vector3 Direction)
        {
            Matrix4 mv = Mathf.RotationAlign(Direction.Z < 0 ? -Vector3.UnitZ : Vector3.UnitZ, Direction);
            var modifier = new Vector3(0, Direction.Z < 0 ? 180 : 0, 0);
            var multiplier = new Vector3(Direction.Z < 0 ? -1f : 1, 1, 1);
            mv.ExtractRotation().ToAxisAngle(out Vector3 axis, out float angle);
            if(float.IsNaN(angle)) return Vector3.Zero;
            return axis * angle * Mathf.Degree * multiplier + modifier;
        }
            
        public static float HeightAtPosition(float X, float Z, int Lod = -1)
        {
             return HeightAtPosition(new Vector3(X,0,Z), Lod);
        }
        
        public static float HeightAtPosition(Vector3 BlockPosition, int Lod = -1)
        {
            
            if(World.GetHighestBlockAt( (int)BlockPosition.X, (int)BlockPosition.Z).Noise3D)
            {
                return HeightAtBlock( new Vector3(BlockPosition.X, World.GetHighestY( (int) BlockPosition.X, (int) BlockPosition.Z), BlockPosition.Z) );
            }

            if (Lod == -1)
            {
                var underChunk = World.GetChunkAt(BlockPosition);
                if (underChunk != null && underChunk.Landscape.GeneratedLod > 1)
                {
                    Lod = underChunk.Landscape.GeneratedLod;
                    var chunkOffset = World.ToChunkSpace(BlockPosition);
                    var bSpace = World.ToBlockSpace(BlockPosition);
                    BlockPosition =
                        new Vector3(Lod * (float) Math.Round(bSpace.X / Lod), 0, Lod * (float) Math.Round(bSpace.Z / Lod)) *
                        Chunk.BlockSize + chunkOffset.ToVector3();
                }
                else
                {
                    Lod = 1;
                }
            }        
            
            var yx = GetHighest( (int) BlockPosition.X + (int) Chunk.BlockSize * Lod, (int) BlockPosition.Z);
            var yz = GetHighest( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize* Lod);
            var yxz = GetHighest( (int) BlockPosition.X + (int) Chunk.BlockSize * Lod, (int) BlockPosition.Z + (int) Chunk.BlockSize);
            var yh = GetHighest( (int) BlockPosition.X, (int) BlockPosition.Z);
                
            var blockSpace = World.ToBlockSpace(BlockPosition);
            var coords = new Vector2(Math.Abs(BlockPosition.X) % Chunk.BlockSize , Math.Abs(BlockPosition.Z) % Chunk.BlockSize) / Chunk.BlockSize;
            
            var bottom = new Vector3(blockSpace.X, yh, blockSpace.Z);
            var right = new Vector3(blockSpace.X+1, yx, blockSpace.Z);
            var top = new Vector3(blockSpace.X+1, yxz, blockSpace.Z+1);
            var front = new Vector3(blockSpace.X, yz, blockSpace.Z+1);

            float height1 = coords.X < 1 - coords.Y
                ? Mathf.BarryCentric(new Vector3(0, bottom.Y, 0), new Vector3(1, right.Y, 0), new Vector3(0, front.Y, 1), coords)
                : Mathf.BarryCentric(new Vector3(1, right.Y, 0), new Vector3(1, top.Y, 1), new Vector3(0, front.Y, 1), coords);

            coords = new Vector2(coords.X < 0 ? -coords.X : coords.X, coords.Y < 0 ? -coords.Y : coords.Y);

            float height0 = coords.X < 1 - coords.Y
                ? Mathf.BarryCentric(new Vector3(0, bottom.Y, 0), new Vector3(1, right.Y, 0), new Vector3(0, front.Y, 1), coords)
                : Mathf.BarryCentric(new Vector3(1, right.Y, 0), new Vector3(1, top.Y, 1), new Vector3(0, front.Y, 1), coords);

            return (height0 + height1) * .5f * Chunk.BlockSize;
        }

        public static float WaterLevelAtPosition(Vector3 Position)
        {
            return WaterHeight(Position) - HeightAtPosition(Position);
        }
        
        public static int WaterBlock(Chunk UnderChunk, Vector3 Position)
        {
            int nearestWaterBlockY = 0;
            var blockSpace = World.ToBlockSpace(Position);
            for (var y = UnderChunk.BoundsY - 1; y > -1; y--)
            {
                var block = UnderChunk.GetBlockAt((int)blockSpace.X, y, (int)blockSpace.Z);
                if (block.Type == BlockType.Water)
                {
                    nearestWaterBlockY = y;
                    break;
                }
            }

            return nearestWaterBlockY;
        }

        public static float WaterHeight(Vector3 Position)
        {
            return (WaterDensityAndHeight(Position)-1) * Chunk.BlockSize;
        }

        public static bool IsWaterBlock(Vector3 Position)
        {
            return Math.Abs(WaterDensityAndHeight(Position)) > 0.005f;
        }

        public static float WaterDensityAndHeight(Vector3 Position)
        {
            var chunk = World.GetChunkAt(Position);
            var blockSpace = World.ToBlockSpace(Position);
            if (chunk == null) return 0;
            return chunk.GetWaterDensity(new Vector3(blockSpace.X, WaterBlock(chunk, Position), blockSpace.Z));
        }

        public static Vector3 WaterNormalAtPosition(Vector3 Position)
        {
            var heightX = WaterDensityAndHeight(Position + Vector3.UnitX * Chunk.BlockSize);
            var heightZ = WaterDensityAndHeight(Position + Vector3.UnitZ * Chunk.BlockSize);
            var heightXz = WaterDensityAndHeight(Position + Vector3.UnitX * Chunk.BlockSize +  Vector3.UnitZ * Chunk.BlockSize);
            var height = WaterDensityAndHeight(Position);
            
            
            var blockSpace = World.ToBlockSpace(Position);
            var coords = new Vector2(Math.Abs(Position.X) % Chunk.BlockSize , Math.Abs(Position.Z) % Chunk.BlockSize) / Chunk.BlockSize;
            
            var bottom = new Vector3(blockSpace.X, height, blockSpace.Z);
            var right = new Vector3(blockSpace.X+1, heightX, blockSpace.Z);
            var top = new Vector3(blockSpace.X+1, heightXz, blockSpace.Z+1);
            var front = new Vector3(blockSpace.X, heightZ, blockSpace.Z+1);

            return -(coords.X < 1-coords.Y ? Mathf.CalculateNormal(bottom, right, front) : Mathf.CalculateNormal(right, top, front));
        }
        
        public static float HeightAtBlock(Vector3 BlockPosition)
        {        
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
        
        public static Vector3 NormalAtPosition(Vector3 Position, int Lod = -1)
        {
            var heightX = GetHighest(Position.X + Chunk.BlockSize, Position.Z);
            var heightZ = GetHighest(Position.X, Position.Z + Chunk.BlockSize);
            var heightXz = GetHighest(Position.X + Chunk.BlockSize, Position.Z + Chunk.BlockSize);
            var height = GetHighest(Position.X, Position.Z);
                        
            var blockSpace = World.ToBlockSpace(Position);
            var coords = new Vector2(Math.Abs(Position.X) % Chunk.BlockSize , Math.Abs(Position.Z) % Chunk.BlockSize) / Chunk.BlockSize;
            
            var bottom = new Vector3(blockSpace.X, height, blockSpace.Z);
            var right = new Vector3(blockSpace.X+1, heightX, blockSpace.Z);
            var top = new Vector3(blockSpace.X+1, heightXz, blockSpace.Z+1);
            var front = new Vector3(blockSpace.X, heightZ, blockSpace.Z+1);

            return -(coords.X < 1-coords.Y ? Mathf.CalculateNormal(bottom, right, front) : Mathf.CalculateNormal(right, top, front));
        }

        private static float GetHighest(float X, float Z)
        {
            return World.GetHighest((int)X, (int)Z);
        }    
        
        public static bool Collides(ICollidable Obj1, ICollidable Obj2)
        {
            if (!GJKCollision.IsInsideBroadphase(Obj1, Obj2)) return false;
            var obj1Box = Obj1.AsBox();
            var obj2Box = Obj2.AsBox();

            if (obj1Box != null && obj2Box != null)
                return AABBvsAABB(obj1Box, obj2Box);
            
            var obj1Group = Obj1.AsGroup();
            var obj2Group = Obj2.AsGroup();

            if (obj1Group != null || obj2Group != null)
            {
                if(obj1Group != null && obj2Group != null)
                    throw new NotSupportedException("Collision between 2 collision groups is unsupported.");
                return GroupVsShape(obj1Group ?? obj2Group, obj1Group == null ? Obj1.AsShape() : Obj2.AsShape());
            }

            return GJKCollision.Collides(Obj1.AsShape(), Obj2.AsShape());
        }

        private static bool GroupVsShape(CollisionGroup Group, CollisionShape Shape)
        {
            for (var i = 0; i < Group.Colliders.Length; i++)
            {
                if (!GJKCollision.IsInsideBroadphase(Group.Colliders[i], Shape)) continue;
                if (GJKCollision.Collides(Group.Colliders[i].AsShape(), Shape)) return true;
            }
            return false;
        }

        private static bool AABBvsAABB(Box A, Box B)
        {
            return A.Min.X  <= B.Max.X && A.Max.X >= B.Min.X &&
                A.Min.Y  <= B.Max.Y && A.Max.Y >= B.Min.Y &&
                A.Min.Z  <= B.Max.Z && A.Max.Z  >= B.Min.Z;
        }
        
        public static bool AABBvsPoint(Box A, Vector3 P)
        {
          return P.X >= A.Min.X && P.X <= A.Max.X &&
                P.Y >= A.Min.Y && P.Y <= A.Max.Y &&
                P.Z >= A.Min.Y && P.Z <= A.Max.Z;
        }
    }        
}
