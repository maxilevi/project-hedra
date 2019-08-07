using BulletSharp;

namespace Hedra.Engine.BulletPhysics
{
    public class PhysicsComponentMotionState : MotionState
    {
        private Matrix _worldTransform;
        
        public override Matrix WorldTransform
        {
            get => _worldTransform;
            set
            {
                _worldTransform = value;
                Position = _worldTransform.Origin.Compatible();
            }
        }
        
        public OpenTK.Vector3 Position { get; set; }
    }
}