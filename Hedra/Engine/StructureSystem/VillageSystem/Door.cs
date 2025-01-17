using System;
using System.Numerics;
using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Door : InteractableStructure
    {
        private readonly RigidBody _body;
        private readonly float _invertedRotation;
        private readonly ObjectMesh _mesh;
        private readonly Vector3 _rotationPoint;
        private bool _isMoving;
        private Vector3 _lastPosition;
        private Vector3 _lastRotation;
        private bool _opened;
        private Vector3 _position;
        private Vector3 _targetRotation;

        public Door(VertexData Mesh, Vector3 RotationPoint, Vector3 Position, bool InvertedRotation) : base(Position)
        {
            _mesh = ObjectMesh.FromVertexData(Mesh);
            _mesh.ApplyNoiseTexture = true;
            _rotationPoint = RotationPoint;
            _invertedRotation = InvertedRotation ? -1 : 1;
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), BuildShape(Mesh)))
            {
                _body = new RigidBody(bodyInfo);
                _body.Translate(Position.Compatible());
                BulletPhysics.Add(_body, new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.StaticFilter,
                    Mask = CollisionFilterGroups.AllFilter,
                    Name = $"Door at {Position}",
                    StaticOffsets = new[] { World.ToChunkSpace(Position) }
                });
                _body.Gravity = BulletSharp.Math.Vector3.Zero;
            }
        }

        protected override bool SingleUse => false;
        protected override bool AllowThroughCollider => true;
        public override string Message => Translations.Get(!_opened ? "open_door" : "close_door");
        protected override bool CanInteract => !IsLocked && base.CanInteract;
        public bool IsLocked { get; set; }
        public override int InteractDistance => 12;

        public override Vector3 Position
        {
            get => _position;
            set
            {
                _body?.Translate((-_position + value).Compatible());
                _position = value;
            }
        }

        public override void Update(float DeltaTime)
        {
            if (_mesh != null) _mesh.Position = Position;
            base.Update(DeltaTime);
        }

        private BoxShape BuildShape(VertexData Mesh)
        {
            var collider = BoneBox.From(new BoneData
            {
                Id = 0,
                Vertices = Mesh.Vertices.ToArray()
            });
            if (collider.Size.LengthSquared() <= 0.25f) throw new ArgumentOutOfRangeException();
            return new BoxShape(collider.Size.Compatible() * .5f);
        }

        protected override void DoUpdate(float DeltaTime)
        {
            base.DoUpdate(DeltaTime);
            if (_mesh != null)
            {
                _mesh.LocalRotation = Mathf.Lerp(_mesh.LocalRotation, _targetRotation, DeltaTime * 4f);
                _mesh.LocalRotationPoint = _rotationPoint;
            }
        }

        private void EnableBox()
        {
            _body.CollisionFlags ^= CollisionFlags.NoContactResponse;
        }

        private void DisableBox()
        {
            _body.CollisionFlags |= CollisionFlags.NoContactResponse;
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            if (_isMoving) return;
            _opened = !_opened;
            _targetRotation = _opened ? Vector3.UnitY * 90 * _invertedRotation : Vector3.Zero;
            if (_opened) DisableBox();
            else EnableBox();
            SoundPlayer.PlaySound(SoundType.Door, Position);
        }

        public static Vector3 GetRotationPointFromMesh(VertexData Mesh, bool Inverted)
        {
            var xSize = Vector3.UnitX * (Mesh.SupportPoint(Vector3.UnitX).X - Mesh.SupportPoint(-Vector3.UnitX).X);
            var zSize = Vector3.UnitZ * (Mesh.SupportPoint(Vector3.UnitZ).Z - Mesh.SupportPoint(-Vector3.UnitZ).Z);
            return (xSize.LengthFast() > zSize.LengthFast() ? xSize : zSize) * (Inverted ? -.5f : .5f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _mesh.Dispose();
            BulletPhysics.RemoveAndDispose(_body);
        }
    }
}