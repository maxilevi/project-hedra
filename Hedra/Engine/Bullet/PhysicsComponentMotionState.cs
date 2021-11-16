using BulletSharp;
using BulletSharp.Math;
using Vector3 = System.Numerics.Vector3;

namespace Hedra.Engine.Bullet
{
    public delegate void OnMotionStateUpdated();

    public class PhysicsComponentMotionState : MotionState
    {
        private Matrix _worldTransform;

        public Vector3 Position { get; set; }
        public event OnMotionStateUpdated OnUpdated;

        public override void GetWorldTransform(out Matrix worldTrans)
        {
            worldTrans = _worldTransform;
        }

        public override void SetWorldTransform(ref Matrix worldTrans)
        {
            _worldTransform = worldTrans;
            Position = _worldTransform.Origin.Compatible();
            OnUpdated?.Invoke();
        }
    }
}