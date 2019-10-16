using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class WindmillBlades : BaseStructure, IUpdatable
    {
        private Quaternion _currentRotation;
        private Quaternion _targetRotation;
        private readonly ObjectMesh _mesh;
        private readonly CollisionShape _shape;
        private readonly Vector3 _rotationAxis;
        private float _angle = 0;
        
        public WindmillBlades(VertexData Mesh, CollisionShape Shape, Vector3 RotationAxis, Vector3 Position, CollidableStructure Structure) : base(Position)
        {
            _mesh = ObjectMesh.FromVertexData(Mesh);
            _mesh.ApplyNoiseTexture = true;
            _mesh.Position = Position;
            _shape = Shape;
            _rotationAxis = RotationAxis;
            if (_shape != null) Structure.AddCollisionShape(_shape);
            UpdateManager.Add(this);
        }
        
        public void Update()
        {
            _angle += Time.DeltaTime * 80f;
            var yawPitchRoll = _rotationAxis * _angle * Mathf.Radian;
            _targetRotation = Quaternion.CreateFromYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
            _currentRotation = Quaternion.Slerp(_currentRotation, _targetRotation, Time.DeltaTime * 2f);
            _mesh.LocalRotation = _currentRotation.ToEuler();
            _mesh.Position = Position;
            if(_angle > 360) _angle -= 360;
        }

        public override void Dispose()
        {
            base.Dispose();
            _mesh.Dispose();
            UpdateManager.Remove(this);
        }
    }
}