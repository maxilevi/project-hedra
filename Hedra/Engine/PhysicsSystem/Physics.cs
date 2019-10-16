/*
 * Author: Zaphyk
 * Date: 11/02/2016
 * Time: 06:29 p.m.
 *
 */
using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using System.Collections.Generic;
using System.Linq;
using BulletSharp.SoftBody;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;
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
        
        public static Vector3 DirectionToEuler(Vector3 Direction)
        {
            #if DEBUG
            if(Direction.IsInvalid())
                throw new ArgumentException(nameof(Direction));
            #endif
            if(Direction == Vector3.Zero) return Vector3.Zero;
            if(Direction == new Vector3(0, 1, 0)) return Vector3.UnitX * -90;
            if(Direction == new Vector3(0, -1, 0)) return Vector3.UnitX * 90;
            var newForward = Direction.Xz().ToVector3().NormalizedFast();
            var defaultRot = Vector3.UnitX * CalculateNewX(Direction) + Vector3.UnitZ * GetRotation(Vector3.UnitZ, new Vector3(0, Direction.Y, 1).NormalizedFast(), Vector3.UnitY).Z;
            var xRot = GetRotation(Vector3.UnitZ, newForward, Vector3.UnitY);
            var quat = ((Matrix4x4.CreateRotationZ(defaultRot.Z * Mathf.Radian) * Matrix4x4.CreateRotationX(defaultRot.X * Mathf.Radian)) * Matrix4x4.CreateRotationY(xRot.Y * Mathf.Radian)).ExtractRotation();
            var axisAngle = quat.ToAxisAngle();
            var result = axisAngle.Xyz() * axisAngle.W * Mathf.Degree;
            #if DEBUG
            if (result.IsInvalid())
            {
                int a = 0;
            }
            #endif
            return result;
        }

        private static Vector3 GetRotation(Vector3 source, Vector3 dest, Vector3 up)
        {
            float dot = Vector3.Dot(source, dest);

            if (Math.Abs(dot - (-1.0f)) < 0.01f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return up * 180;
            }
            if (Math.Abs(dot - (1.0f)) < 0.01f)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Vector3.Zero;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(source, dest);
            rotAxis = Vector3.Normalize(rotAxis);
            return rotAxis * rotAngle * Mathf.Degree;
        }
        
        private static float CalculateNewX(Vector3 Direction)
        {
            var output = 0f;
            if (Direction.Y > 0) output = Mathf.Lerp(0, -90, Direction.Y);
            else output = Mathf.Lerp(0, 90, -Direction.Y);
            return output;
        }
        
        public static float HeightAtPosition(float X, float Z)
        {
             return HeightAtPosition(new Vector3(X,0,Z));
        }

        public static float HeightAtPosition(Vector3 BlockPosition)
        {
            var yx = GetHighest( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z);
            var yz = GetHighest( (int) BlockPosition.X, (int) BlockPosition.Z + (int) Chunk.BlockSize);
            var yxz = GetHighest( (int) BlockPosition.X + (int) Chunk.BlockSize, (int) BlockPosition.Z + (int) Chunk.BlockSize);
            var yh = GetHighest( (int) BlockPosition.X, (int) BlockPosition.Z);
                
            var blockSpace = World.ToBlockSpace(BlockPosition);
            var coords = new Vector2(BlockPosition.X % Chunk.BlockSize , BlockPosition.Z % Chunk.BlockSize) / Chunk.BlockSize;
            coords = new Vector2(coords.X < 0 ? 1 + coords.X : coords.X, coords.Y < 0 ? 1 + coords.Y : coords.Y);
            
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

        public static int WaterBlock(Chunk UnderChunk, Vector3 Position)
        {
            if (UnderChunk == null || !UnderChunk.IsGenerated) return 0;
            int nearestWaterBlockY = 0;
            var blockSpace = World.ToBlockSpace(Position);
            for (var y = UnderChunk.MaximumHeight; y > UnderChunk.MinimumHeight; y--)
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
            var underChunk = World.GetChunkAt(Position);
            return (WaterBlock(underChunk, Position)+1) * Chunk.BlockSize;
        }

        public static bool IsWaterBlock(Vector3 Position)
        {
            return World.GetBlockAt(Position).Type == BlockType.Water;
        }

        public static Vector3 WaterNormalAtPosition(Vector3 Position)
        {
            var heightX = WaterHeight(Position + Vector3.UnitX * Chunk.BlockSize);
            var heightZ = WaterHeight(Position + Vector3.UnitZ * Chunk.BlockSize);
            var heightXz = WaterHeight(Position + Vector3.UnitX * Chunk.BlockSize +  Vector3.UnitZ * Chunk.BlockSize);
            var height = WaterHeight(Position);
            
            
            var blockSpace = World.ToBlockSpace(Position);
            var coords = new Vector2(Math.Abs(Position.X) % Chunk.BlockSize , Math.Abs(Position.Z) % Chunk.BlockSize) / Chunk.BlockSize;
            
            var bottom = new Vector3(blockSpace.X, height, blockSpace.Z);
            var right = new Vector3(blockSpace.X+1, heightX, blockSpace.Z);
            var top = new Vector3(blockSpace.X+1, heightXz, blockSpace.Z+1);
            var front = new Vector3(blockSpace.X, heightZ, blockSpace.Z+1);

            return -(coords.X < 1-coords.Y ? Mathf.CalculateNormal(bottom, right, front) : Mathf.CalculateNormal(right, top, front));
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
    }        
}
