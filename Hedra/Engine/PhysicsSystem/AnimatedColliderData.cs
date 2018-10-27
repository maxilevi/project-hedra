using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class AnimatedColliderData
    {
        public BoneData[] BonesData { get; set; }
        public BoneBox[] DefaultBoneBoxes { get; set; }
        public CollisionPoint[] DefaultBroadphase { get; set; }
    }

    public class CollisionPoint
    {
        public Vector3 Id { get; set; }
        public Vector3 Vertex { get; set; }
    }
}
