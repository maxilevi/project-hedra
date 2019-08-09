using BulletSharp;
using BulletSharp.Math;

namespace Hedra.Engine.BulletPhysics
{
    public delegate void OnMotionStateUpdated();
    public class PhysicsComponentMotionState : MotionState
    {
        public event OnMotionStateUpdated OnUpdated;
        private Matrix _worldTransform;

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

        public OpenTK.Vector3 Position { get; set; }
    }
}