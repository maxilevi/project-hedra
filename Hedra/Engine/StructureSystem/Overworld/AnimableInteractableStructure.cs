using System;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class AnimableInteractableStructure : InteractableStructure
    {
        public Func<bool> Condition { get; set; }
        protected override bool CanInteract => IsClosed && (Condition?.Invoke() ?? true) && base.CanInteract;
        protected override bool DisposeAfterUse => false;
        private readonly AnimatedModel _model;
        private readonly Animation _idleAnimation;
        private readonly Animation _useAnimation;
        private IHumanoid _lastUser;
        private Chunk _underChunk;

        protected AnimableInteractableStructure(Vector3 Position, Vector3 Scale) : base(Position)
        {
            this._model = AnimationModelLoader.LoadEntity(ModelPath);
            this._idleAnimation = AnimationLoader.LoadAnimation(IdleAnimationPath);
            this._useAnimation = AnimationLoader.LoadAnimation(UseAnimationPath);
            this._useAnimation.Loop = false;
            this._useAnimation.Speed = AnimationSpeed;
            this._useAnimation.OnAnimationEnd += _ =>
            {
                OnUse(_lastUser);
                _lastUser = null;
            };
            this._model.UpdateWhenOutOfView = true;
            this._model.PlayAnimation(_idleAnimation);
            this._model.Scale = ModelScale * Scale;
            this._model.ApplyFog = true;
        }

        protected abstract void OnUse(IHumanoid Humanoid);
        
        public override void Update(float DeltaTime)
        {
            if (_model != null)
            {
                _model.Position = Position;
                _model.Update();
            }
            base.Update(DeltaTime);
        }

        protected override void DoUpdate(float DeltaTime)
        {
            base.DoUpdate(DeltaTime);
            if (_model == null) return;
            HandleColliders();
        }

        protected override void OnSelected(IHumanoid Humanoid)
        {
            base.OnSelected(Humanoid);
            _model.Tint = new Vector4(2.5f, 2.5f, 2.5f, 1);
        }
        
        protected override void OnDeselected(IHumanoid Humanoid)
        {
            base.OnDeselected(Humanoid);
            _model.Tint = new Vector4(1, 1, 1, 1);
        }

        private void HandleColliders()
        {
            var underChunk = World.GetChunkAt(this.Position);
            if (underChunk != null && underChunk.BuildedWithStructures && _underChunk != underChunk)
            {
                _underChunk = underChunk;
                if(EnableLegacyTerrainHeightMode && Position.Y <= 1)
                    Position = new Vector3(Position.X, Physics.HeightAtPosition(Position) + .5f, Position.Z);

                var shape = AssetManager.LoadCollisionShapes(ColliderPath, Vector3.One)[0];
                shape.Transform(Matrix4x4.CreateScale(this.Scale));
                shape.Transform(ColliderOffset);
                shape.Transform(Matrix4x4.CreateRotationY(this.Rotation.Y * Mathf.Radian) *
                                Matrix4x4.CreateRotationX(this.Rotation.X * Mathf.Radian) *
                                Matrix4x4.CreateRotationZ(this.Rotation.Z * Mathf.Radian));

                shape.Transform(Position);
                _underChunk.AddCollisionShape(shape);
            }
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            _lastUser = Humanoid;
            _model.PlayAnimation(_useAnimation);
        }
        
        
        public bool IsClosed => _model.AnimationPlaying == _idleAnimation;
        
        public Vector3 Scale
        {
            get => _model.Scale;
            set => _model.Scale = value;
        }
        
        public Vector3 Rotation
        {
            get => _model.LocalRotation;
            set => _model.LocalRotation = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            this._model.Dispose();
        }
        
        protected abstract string ModelPath { get; }
        protected abstract string IdleAnimationPath { get; }
        protected abstract string UseAnimationPath { get; }
        protected abstract string ColliderPath { get; }
        protected virtual Vector3 ColliderOffset { get; }
        protected virtual bool EnableLegacyTerrainHeightMode { get; }
        protected virtual float AnimationSpeed => 1.0f;
        protected virtual Vector3 ModelScale { get; } = Vector3.One;
    }
}