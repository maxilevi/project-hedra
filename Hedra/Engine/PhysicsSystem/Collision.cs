using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Game;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public static class Collision
    {
        public static void Update(Vector3 Position, List<ICollidable> ChunkCollisions, List<ICollidable> StructureCollisions,
            ref  Vector2 LastChunkCollisionPosition, ref Vector2 LastStructureCollisionPosition)
        {
            var chunkSpace = World.ToChunkSpace(Position.Xz);
            if (chunkSpace != LastChunkCollisionPosition)
            {
                LastChunkCollisionPosition = chunkSpace;
                ChunkCollisions.Clear();
                var underChunk = World.GetChunkAt(Position);
                var underChunkR = World.GetChunkAt(Position + new Vector3(Chunk.Width, 0, 0));
                var underChunkL = World.GetChunkAt(Position - new Vector3(Chunk.Width, 0, 0));
                var underChunkF = World.GetChunkAt(Position + new Vector3(0, 0, Chunk.Width));
                var underChunkB = World.GetChunkAt(Position - new Vector3(0, 0, Chunk.Width));
                if(underChunk != null) ChunkCollisions.AddRange(underChunk.CollisionShapes);
                if(underChunkL != null) ChunkCollisions.AddRange(underChunkL.CollisionShapes);
                if(underChunkR != null) ChunkCollisions.AddRange(underChunkR.CollisionShapes);
                if(underChunkF != null) ChunkCollisions.AddRange(underChunkF.CollisionShapes);
                if(underChunkB != null) ChunkCollisions.AddRange(underChunkB.CollisionShapes);
            }
            if ((Position.Xz - LastStructureCollisionPosition).LengthSquared > 1)
            {
                LastStructureCollisionPosition = Position.Xz;
                StructureCollisions.Clear();
                var nearCollisions = GameManager.Player.NearCollisions;
                for (var i = 0; i < nearCollisions.Length; i++)
                {
                    if (nearCollisions[i].Contains(chunkSpace))
                        StructureCollisions.Add(nearCollisions[i]);
                }
            }
        }
    }
}