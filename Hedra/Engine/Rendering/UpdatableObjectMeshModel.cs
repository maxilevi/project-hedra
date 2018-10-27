using Hedra.Engine.EntitySystem;
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

        public Vector3 AnimationPosition
        {
            get => Model.AnimationPosition;
            set => Model.AnimationPosition = value;
        }

        public Vector3 TargetPosition
        {
            get => Model.TargetPosition;
            set => Model.TargetPosition = value;
        }

        public override Vector3 TargetRotation
        {
            get => Model.TargetRotation;
            set => Model.TargetRotation = value;
        }

        public Vector3 RotationPoint
        {
            get => Model.RotationPoint;
            set => Model.RotationPoint = value;
        }

        public Vector3 LocalRotation
        {
            get => Model.LocalRotation;
            set => Model.LocalRotation = value;
        }
        
        public Vector3 LocalPosition
        {
            get => Model.LocalPosition;
            set => Model.LocalPosition = value;
        }
        
        public Vector3 BeforeLocalRotation
        {
            get => Model.BeforeLocalRotation;
            set => Model.BeforeLocalRotation = value;
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