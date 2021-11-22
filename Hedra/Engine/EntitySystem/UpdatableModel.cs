using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Engine.EntitySystem
{
    public abstract class UpdatableModel<T> : BaseUpdatableModel, IDisposable where T : class, ICullableModel
    {
        protected const float PitchSpeed = 1.11f;
        private Box _baseBroadphaseBox = new Box(Vector3.Zero, Vector3.One);
        private List<IModel> _iterableModels;
        private T _model;
        private readonly Timer _movingTimer;

        protected UpdatableModel(IEntity Parent)
        {
            _iterableModels = new List<IModel>();
            _movingTimer = new Timer(.05f)
            {
                UseTimeScale = false
            };
            AdditionalModels = new HashSet<IModel>();
            this.Parent = Parent;
            if (Parent?.Physics != null)
                Parent.Physics.OnMove += OnMove;
        }

        protected HashSet<IModel> AdditionalModels { get; }
        public override Vector3 TargetRotation { get; set; }
        public override bool IsStatic => false;
        public override bool Disposed { get; protected set; }
        public IEntity Parent { get; set; }

        protected T Model
        {
            get => _model;
            set
            {
                if (value != null)
                {
                    UnregisterModel(_model);
                    _model = value;
                    RegisterModel(_model);
                }
            }
        }

        public override float AnimationSpeed
        {
            get => _model.AnimationSpeed;
            set => _model.AnimationSpeed = value;
        }

        public override bool IsAttacking { get; protected set; }
        public override bool IsIdling { get; protected set; }
        public override bool IsWalking { get; protected set; }
        public override bool IsMoving { get; protected set; }
        public override float Alpha { get; set; } = 1;
        public override CollisionShape HorizontalBroadphaseCollider => BaseBroadphaseBox.ToShape();
        public override Box Dimensions { get; protected set; }

        public override Box BaseBroadphaseBox
        {
            get => _baseBroadphaseBox;
            protected set
            {
                _baseBroadphaseBox = value;
                _model.CullingBox = _baseBroadphaseBox;
            }
        }

        public override float Height => BaseBroadphaseBox.Max.Y;

        public override bool Pause
        {
            get => Model.Pause;
            set => _iterableModels.ForEach(M => M.Pause = value);
        }

        public override bool ApplyFog
        {
            get => Model.ApplyFog;
            set => _iterableModels.ForEach(M => M.ApplyFog = value);
        }

        public override bool Enabled
        {
            get => Model.Enabled;
            set => _iterableModels.ForEach(M => M.Enabled = value);
        }

        public override Vector4 BaseTint
        {
            get => Model.BaseTint;
            set => _iterableModels.ForEach(M => M.BaseTint = value);
        }

        public override Vector4 Tint
        {
            get => Model.Tint;
            set => _iterableModels.ForEach(M => M.Tint = value);
        }

        public override bool Outline
        {
            get => Model.Outline;
            set => _iterableModels.ForEach(M => M.Outline = value);
        }

        public override Vector4 OutlineColor
        {
            get => Model.OutlineColor;
            set => _iterableModels.ForEach(M => M.OutlineColor = value);
        }

        public override Vector3 Position
        {
            get => Model.Position;
            set => _iterableModels.ForEach(M => M.Position = value);
        }

        public override Vector3 LocalRotation
        {
            get => Model.LocalRotation;
            set => _iterableModels.ForEach(M => M.LocalRotation = value);
        }

        public override Vector3 Scale
        {
            get => Model.Scale;
            set => _iterableModels.ForEach(M => M.Scale = value);
        }

        public override void Dispose()
        {
            if (Parent != null)
                Parent.Physics.OnMove -= OnMove;
            Model?.Dispose();
            Disposed = true;
        }

        protected void RegisterModel(IModel Model)
        {
            AdditionalModels.Add(Model);
            _iterableModels = AdditionalModels.ToList();
        }

        protected void UnregisterModel(IModel Model)
        {
            AdditionalModels.Remove(Model);
            _iterableModels = AdditionalModels.ToList();
        }

        public override void Update()
        {
            if (_iterableModels.Count > 0)
            {
                _iterableModels.ForEach(M =>
                    M.BaseTint = Mathf.Lerp(M.BaseTint, BaseTint, Time.IndependentDeltaTime * 6f));
                _iterableModels.ForEach(M => M.Tint = Mathf.Lerp(M.Tint, Tint, Time.IndependentDeltaTime * 6f));
                _iterableModels.ForEach(M => M.Alpha = Mathf.Lerp(M.Alpha, Alpha, Time.DeltaTime * 8f));
            }
        }

        public override void BaseUpdate()
        {
            if (Parent != null)
            {
                if (IsMoving && _movingTimer.Tick()) IsMoving = false;
                Position = Parent.Position;
            }
        }

        public override void Attack(IEntity Victim, float RangeModifier)
        {
        }

        public override bool CanAttack(IEntity Victim, float RangeModifier)
        {
            throw new NotImplementedException();
        }

        private void OnMove()
        {
            _movingTimer.Reset();
            IsMoving = true;
        }

        public void StopMoving()
        {
            _movingTimer.Reset();
            IsMoving = false;
        }
    }
}