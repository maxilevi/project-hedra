using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class CollisionGroup : ICollidable
    {
        public CollisionShape[] Colliders { get; }
        public float BroadphaseRadius { get; private set; }
        public Vector3 BroadphaseCenter { get; private set; }
        private HashSet<Vector2> _offsets;
        
        
        public CollisionGroup(params CollisionShape[] Colliders)
        {
            this.Colliders = Colliders;
            Recalculate();
        }

        public bool Contains(Vector2 ChunkOffset)
        {
            return _offsets.Contains(ChunkOffset);
        }

        public void Recalculate()
        {
            CalculateBroadphase();
            CalculateAffectedOffsets();
        }
        
        private void CalculateAffectedOffsets()
        {
            var minPoint = new Vector3(
                Colliders.Min(C => C.SupportPoint(-Vector3.UnitX).X),
                Colliders.Min(C => C.SupportPoint(-Vector3.UnitY).Y),
                Colliders.Min(C => C.SupportPoint(-Vector3.UnitZ).Z)
            );
            var maxPoint = new Vector3(
                Colliders.Max(C => C.SupportPoint(Vector3.UnitX).X),
                Colliders.Max(C => C.SupportPoint(Vector3.UnitY).Y),
                Colliders.Max(C => C.SupportPoint(Vector3.UnitZ).Z)
            );

            _offsets = _offsets ?? (_offsets = new HashSet<Vector2>());
            _offsets.Clear();
            for (var x = (int) Math.Floor(minPoint.X) - Chunk.Width; x <= Math.Ceiling(maxPoint.X) + Chunk.Width; x+=Chunk.Width)
            {
                for (var z = (int) Math.Floor(minPoint.Z) - Chunk.Width; z <= (int) Math.Ceiling(maxPoint.Z) + Chunk.Width; z+=Chunk.Width)
                {
                    var offset = World.ToChunkSpace(new Vector2(x, z));
                    _offsets.Add(offset);
                }
            }
        }
        
        private void CalculateBroadphase()
        {
            var avg = Vector3.Zero;
            for (var i = 0; i < Colliders.Length; i++)
            {
                avg += Colliders[i].BroadphaseCenter;
            }
            avg /= Math.Max(Colliders.Length, 1);
            BroadphaseCenter = avg;

            var dist = 0f;
            for (var k = 0; k < Colliders.Length; k++)
            {
                for (var i = 0; i < Colliders[k].Vertices.Length; i++)
                {
                    var length = (Colliders[k].Vertices[i] - this.BroadphaseCenter).LengthFast;

                    if (length > dist)
                        dist = length;
                }
            }
            this.BroadphaseRadius = dist;
        }

        public CollisionShape AsShape()
        {
            return null;
        }

        public Box AsBox()
        {
            return null;
        }

        public CollisionGroup AsGroup()
        {
            return this;
        }       
                
        public Vector3[] Vertices => throw new NotImplementedException();

    }
}