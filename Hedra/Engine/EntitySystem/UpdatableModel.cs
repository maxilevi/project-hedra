using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public abstract class UpdatableModel<T> : BaseUpdatableModel, IDisposable where T : class, ICullableModel
    {
        protected HashSet<IModel> AdditionalModels { get; }
        public override Vector3 TargetRotation { get; set; }
        public override bool IsStatic => false;
        public override bool Disposed { get; protected set; }
        public IEntity Parent { get; set; }
        private T _model;
        private List<IModel> _iterableModels;
        private Box _baseBroadphaseBox = new Box(Vector3.Zero, Vector3.One);

        protected UpdatableModel(IEntity Parent)
        {
            this._iterableModels = new List<IModel>();
            this.AdditionalModels = new HashSet<IModel>();
            this.Parent = Parent;
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

        public T Model
        {
            get => _model;
            protected set
            {
                if (_model?.Equals(value) ?? value != null)
                {
                    this.UnregisterModel(_model);
                    _model = value;
                    this.RegisterModel(_model);
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
        public override float Alpha { get; set; } = 1;

        public override Vector3[] Vertices => BroadphaseBox.Vertices.ToArray();
        public override CollisionShape[] Colliders => new []{ BroadphaseBox.ToShape() };
        public override CollisionShape BroadphaseCollider => BroadphaseBox.ToShape();
        public override Box BroadphaseBox => BaseBroadphaseBox.Cache.Translate(Model.Position);
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

        public override float Height => BaseBroadphaseBox.Max.Y - BaseBroadphaseBox.Min.Y;

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

        public override Vector3 Position
        {
            get => Model.Position;
            set => _iterableModels.ForEach(M => M.Position = value);
        }

        public override Vector3 Rotation
        {
            get => Model.Rotation;
            set => _iterableModels.ForEach(M => M.Rotation = value);
        }

        public override Vector3 Scale
        {
            get => Model.Scale;
            set => _iterableModels.ForEach(M => M.Scale = value);
        }

        public override void Update()
        {
            if (_iterableModels.Count > 0)
            {
                _iterableModels.ForEach(M => M.BaseTint = Mathf.Lerp(M.BaseTint, this.BaseTint, Time.IndependantDeltaTime * 6f));
                _iterableModels.ForEach(M => M.Tint = Mathf.Lerp(M.Tint, this.Tint, Time.IndependantDeltaTime * 6f));
                _iterableModels.ForEach(M => M.Alpha = Mathf.Lerp(M.Alpha, this.Alpha, Time.DeltaTime * 8f));
            }
        }

        public override void Idle()
        {

        }

        public override void Run()
        {

        }

        public override void Attack(IEntity Victim)
        {
            this.Attack(Victim, 1);
        }

        public override void Attack(IEntity Victim, float RangeModifier)
        {

        }

        public override void Draw()
        {
            
        }

        public override void Dispose()
        {
            this.Model?.Dispose();
            this.Disposed = true;
        }
    }
}
