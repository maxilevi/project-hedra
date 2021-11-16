using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.EntitySystem
{
    public class DummyModel : UpdatableObjectMeshModel
    {
        public DummyModel(IEntity Parent, Box Dimensions) : base(Parent)
        {
            Model = new ObjectMesh(VertexData.Empty);
            this.Dimensions = Dimensions;
            //this.BaseBroadphaseBox = new Box(Vector3.Zero, (Dimensions.Max - Dimensions.Min) * .5f);
        }

        public override bool IsStatic => false;
    }
}