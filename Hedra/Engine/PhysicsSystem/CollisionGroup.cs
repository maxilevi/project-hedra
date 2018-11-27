using System;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class CollisionGroup : ICollidable
    {
        public ICollidable[] Colliders { get; }
        public float BroadphaseRadius { get; private set; }
        public Vector3 BroadphaseCenter { get; private set; }
        
        public CollisionGroup(ICollidable[] Colliders)
        {
            this.Colliders = Colliders;
            CalculateBroadphase();
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