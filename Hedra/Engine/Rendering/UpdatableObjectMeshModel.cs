using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public abstract class UpdatableObjectMeshModel : UpdatableModel<ObjectMesh>
    {
        protected UpdatableObjectMeshModel(IEntity Parent) : base(Parent)
        {
        }
        
        public bool Enabled
        {
            get => Model.Enabled;
            set => Model.Enabled = value;
        }
        
        public bool Outline
        {
            get => Model.Outline;
            set => Model.Outline = value;
        }

        public Matrix4 TransformationMatrix
        {
            get => Model.TransformationMatrix;
            set => Model.TransformationMatrix = value;
        }

        public Vector3 RotationPoint
        {
            get => Model.LocalRotationPoint;
            set => Model.LocalRotationPoint = value;
        }

        public Vector3 LocalRotation
        {
            get => Model.Rotation;
            set => Model.Rotation = value;
        }
        
        public Vector3 LocalPosition
        {
            get => Model.LocalPosition;
            set => Model.LocalPosition = value;
        }
        
        public Vector3 BeforeLocalRotation
        {
            get => Model.BeforeRotation;
            set => Model.BeforeRotation = value;
        }

        public Vector3 Position
        {
            get => Model.Position;
            set => Model.Position = value;
        }

        public Vector3 TransformPoint(Vector3 Point)
        {
            return Model.TransformPoint(Point);
        }
    }
}