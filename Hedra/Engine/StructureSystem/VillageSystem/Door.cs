using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Door : InteractableStructure
    {
        protected override bool SingleUse => false;
        public override string Message => Translations.Get(!_opened ? "open_door" : "close_door");
        public override int InteractDistance => 16;
        private bool _isMoving;
        private bool _opened;
        private Vector3 _targetRotation;
        private readonly ObjectMesh _mesh;
        private readonly ObjectMeshCollider _collider;
        private readonly Vector3 _rotationPoint;
        private readonly CollisionShape _shape;
        private Vector3 _lastRotation;
        private Vector3 _lastPosition;
        
        public Door(VertexData Mesh, Vector3 RotationPoint, Vector3 Position, CollidableStructure Structure) : base(Position)
        {
            _mesh = ObjectMesh.FromVertexData(Mesh);
            _mesh.ApplyNoiseTexture = true;
            _mesh.Position = Position;
            _rotationPoint = RotationPoint;
            _collider = new ObjectMeshCollider(_mesh, Mesh);
            _shape = _collider.Shape;
            Structure.AddCollisionShape(_shape);
        }
        public override void Update()
        {
            base.Update();
            if (_mesh != null)
            {
                _mesh.LocalRotation = Mathf.Lerp(_mesh.LocalRotation, _targetRotation, Time.DeltaTime * 4f);
                _mesh.LocalRotationPoint = _rotationPoint;
                _mesh.Position = Position;
            }
            if(_collider != null) UpdateBox();
        }

        private void UpdateBox()
        {
            if ((_mesh.LocalRotation - _lastRotation).LengthSquared < 0.005f * 0.005f
                && (_lastPosition - _mesh.Position).LengthSquared < 0.005f * 0.005f) return;
            var collider = _collider.Collider;
            for (var i = 0; i < _shape.Vertices.Length; i++)
            {
                _shape.Vertices[i] = collider.Corners[i];
            }
            _shape.RecalculateBroadphase();
            _lastRotation = _mesh.LocalRotation;
            _lastPosition = _mesh.Position;
        }

        protected override void Interact(IPlayer Interactee)
        {
            if (_isMoving) return;
            _opened = !_opened;
            _targetRotation = _opened ? Vector3.UnitY * -90 : Vector3.Zero;
        }

        public static Vector3 GetRotationPointFromMesh(VertexData Mesh)
        {
            var xSize = Vector3.UnitX * (Mesh.SupportPoint(Vector3.UnitX).X - Mesh.SupportPoint(-Vector3.UnitX).X);
            var zSize = Vector3.UnitZ * (Mesh.SupportPoint(Vector3.UnitZ).Z - Mesh.SupportPoint(-Vector3.UnitZ).Z);
            return (xSize.LengthFast > zSize.LengthFast ? xSize : zSize) * .5f;
        }
    }
}