using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public abstract class EntityModel : IDisposable
    {
        protected HashSet<IModel> AdditionalModels { get; }
        public virtual bool IsStatic => false;
        public Entity Parent { get; set; }
        public virtual Vector3 TargetRotation { get; set; }
        public bool Disposed { get; protected set; }
        private List<IModel> _iterableModels;

        protected EntityModel(Entity Parent)
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

        public virtual bool IsAttacking { get; protected set; }
        public virtual float Height { get; protected set; }
        public virtual float Alpha { get; set; } = 1;

        public abstract IModel Model { get; set; }

        public virtual bool Pause
        {
            get => Model.Pause;
            set => _iterableModels.ForEach(M => M.Pause = value);
        }

        public virtual bool ApplyFog
        {
            get => Model.ApplyFog;
            set => _iterableModels.ForEach(M => M.ApplyFog = value);
        }

        public virtual bool Enabled
        {
            get => Model.Enabled;
            set => _iterableModels.ForEach(M => M.Enabled = value);
        }

        public virtual Vector4 BaseTint
        {
            get => Model.BaseTint;
            set => _iterableModels.ForEach(M => M.BaseTint = value);
        }

        public virtual Vector4 Tint
        {
            get => Model.Tint;
            set => _iterableModels.ForEach(M => M.Tint = value);
        }

        public virtual Vector3 Position
        {
            get => Model.Position;
            set => _iterableModels.ForEach(M => M.Position = value);
        }

        public virtual Vector3 Rotation
        {
            get => Model.Rotation;
            set => _iterableModels.ForEach(M => M.Rotation = value);
        }

        public virtual Vector3 Scale
        {
            get => Model.Scale;
            set => _iterableModels.ForEach(M => M.Scale = value);
        }

        public virtual void Update()
        {
            if (Model != null)
            {
                Model.BaseTint = Mathf.Lerp(Model.BaseTint, this.BaseTint, Time.unScaledDeltaTime * 6f);
                Model.Tint = Mathf.Lerp(Model.Tint, this.Tint, Time.unScaledDeltaTime * 6f);
                Model.Alpha = Mathf.Lerp(Model.Alpha, this.Alpha, Time.ScaledFrameTimeSeconds * 8f);
            }
        }

        public virtual void Idle()
        {

        }

        public virtual void Run()
        {

        }

        public virtual void Attack(Entity Target, float Damage)
        {

        }

        public virtual void Draw()
        {
            
        }

        public virtual void Dispose()
        {
            this.Model?.Dispose();
            this.Disposed = true;
        }
    }
}
