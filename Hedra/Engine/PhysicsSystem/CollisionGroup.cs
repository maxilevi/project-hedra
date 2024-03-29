using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.PhysicsSystem
{
    public class CollisionGroup
    {
        private HashSet<Vector2> _offsets;


        public CollisionGroup(params CollisionShape[] Colliders)
        {
            this.Colliders = Colliders;
            Recalculate();
        }

        public CollisionShape[] Colliders { get; }
        public float BroadphaseRadius { get; private set; }
        public Vector3 BroadphaseCenter { get; private set; }

        public Vector2[] Offsets => _offsets.ToArray();

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
            _offsets = new HashSet<Vector2>(Colliders.SelectMany(C => C.Vertices).Select(World.ToChunkSpace));
        }

        private void CalculateBroadphase()
        {
            var avg = Vector3.Zero;
            for (var i = 0; i < Colliders.Length; i++) avg += Colliders[i].BroadphaseCenter;
            avg /= Math.Max(Colliders.Length, 1);
            BroadphaseCenter = avg;

            var dist = 0f;
            for (var k = 0; k < Colliders.Length; k++)
            for (var i = 0; i < Colliders[k].Vertices.Length; i++)
            {
                var length = (Colliders[k].Vertices[i] - BroadphaseCenter).LengthFast();

                if (length > dist)
                    dist = length;
            }

            BroadphaseRadius = dist;
        }
    }
}