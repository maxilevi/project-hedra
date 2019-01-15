using System;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Door : InteractableStructure
    {
        protected override bool SingleUse => false;
        public override string Message => Translations.Get(!_opened ? "open_door" : "close_door");
        public override int InteractDistance => 12;
        private bool _isMoving;
        private bool _opened;
        private Vector3 _targetRotation;
        private readonly ObjectMesh _mesh;
        private readonly ObjectMeshCollider _collider;
        private readonly Vector3 _rotationPoint;
        private readonly CollisionShape _shape;
        private readonly CollisionGroup _group;
        private Vector3 _lastRotation;
        private Vector3 _lastPosition;
        private readonly float _invertedRotation;
        
        public Door(VertexData Mesh, Vector3 RotationPoint, Vector3 Position, bool InvertedRotation, CollidableStructure Structure) : base(Position)
        {
            _mesh = ObjectMesh.FromVertexData(Mesh);
            _mesh.ApplyNoiseTexture = true;
            _mesh.Position = Position;
            _rotationPoint = RotationPoint;
            _collider = new ObjectMeshCollider(_mesh, Mesh);
            _shape = _collider.Shape;
            _invertedRotation = InvertedRotation ? -1 : 1;
            Structure.AddCollisionGroup(_group = new CollisionGroup(_shape));
        }
        public override void Update()
        {
            if(_mesh != null) _mesh.Position = Position;       
            base.Update();
        }

        protected override void DoUpdate()
        {
            base.DoUpdate();
            if (_mesh != null)
            {
                _mesh.LocalRotation = Mathf.Lerp(_mesh.LocalRotation, _targetRotation, Time.DeltaTime * 4f);
                _mesh.LocalRotationPoint = _rotationPoint;
            }
            if (_collider != null)
            {
                if (!_opened) UpdateBox();
                else IgnoreBox();
            }
        }

        private void IgnoreBox()
        {
            if (ShouldUpdate()) return;

            for (var i = 0; i < _shape.Vertices.Length; i++)
            {
                _shape.Vertices[i] = Vector3.Zero;
            }

            UpdateShape();
        }
        
        private void UpdateBox()
        {
            if (ShouldUpdate()) return;
            
            var collider = _collider.Collider;
            for (var i = 0; i < _shape.Vertices.Length; i++)
            {
                _shape.Vertices[i] = collider.Corners[i];
            }

            UpdateShape();
        }

        private bool ShouldUpdate()
        {
            return (_mesh.LocalRotation - _lastRotation).LengthSquared < 0.005f * 0.005f
                   && (_lastPosition - _mesh.Position).LengthSquared < 0.005f * 0.005f;
        }

        private void UpdateShape()
        {
            _shape.RecalculateBroadphase();
            _group.Recalculate();
            _lastRotation = _mesh.LocalRotation;
            _lastPosition = _mesh.Position;
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            if (_isMoving) return;
            _opened = !_opened;
            _targetRotation = _opened ? Vector3.UnitY * 90 * _invertedRotation : Vector3.Zero;
            SoundPlayer.PlaySound(SoundType.Door, Position);
        }

        public static Vector3 GetRotationPointFromMesh(VertexData Mesh, bool Inverted)
        {
            var xSize = Vector3.UnitX * (Mesh.SupportPoint(Vector3.UnitX).X - Mesh.SupportPoint(-Vector3.UnitX).X);
            var zSize = Vector3.UnitZ * (Mesh.SupportPoint(Vector3.UnitZ).Z - Mesh.SupportPoint(-Vector3.UnitZ).Z);
            return (xSize.LengthFast > zSize.LengthFast ? xSize : zSize) * (Inverted ? -.5f : .5f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _mesh.Dispose();
            _collider.Dispose();
        }
    }
}