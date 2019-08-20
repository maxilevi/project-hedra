using BulletSharp;

namespace Hedra.Engine.Bullet
{
    public class ContactTestResultCallback : ContactResultCallback
    {
        public bool HasHit { get; private set; }
        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            HasHit = cp.Distance < 0 || HasHit;
            return 0;
        }
    }
}