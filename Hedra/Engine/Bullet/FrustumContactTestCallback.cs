using System.Collections.Generic;
using BulletSharp;

namespace Hedra.Engine.Bullet
{
    public class FrustumContactTestCallback : ContactResultCallback
    {
        public HashSet<CollisionObject> Objects { get; } = new HashSet<CollisionObject>();
        public bool HasHit => Objects.Count > 0;
        
        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            if (cp.Distance < 0)
                Objects.Add(colObj1Wrap.CollisionObject);
            return 0;
        }

        public void Reset()
        {
            Objects.Clear();
        }
    }
}