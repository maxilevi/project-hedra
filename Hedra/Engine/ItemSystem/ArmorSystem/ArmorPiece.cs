using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.EntitySystem;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class ArmorPiece : IDisposable, IModel
    {
        private readonly ModelData _originalModel;
        private ModelData _currentModel;

        protected ArmorPiece(ModelData Model)
        {
            _originalModel = Model;
        }

        public bool Disposed { get; set; }
        protected IHumanoid Owner { get; private set; }

        public void Dispose()
        {
            if (Owner != null) UnregisterOwner(Owner);
            Disposed = true;
        }

        public Vector4 Tint { get; set; }
        public Vector4 BaseTint { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LocalRotation { get; set; }
        public bool Enabled { get; set; }
        public bool ApplyFog { get; set; }
        public bool Pause { get; set; }
        public float Alpha { get; set; }
        public float AnimationSpeed { get; set; }
        public Vector4 OutlineColor { get; set; }
        public bool Outline { get; set; }

        public void Update(IHumanoid Humanoid)
        {
            UpdateOwner(Humanoid);
        }

        private void UpdateOwner(IHumanoid Humanoid)
        {
            if (Humanoid != Owner)
            {
                if (Owner != null)
                    UnregisterOwner(Owner);
                Owner = Humanoid;
                if (Humanoid != null)
                    RegisterOwner(Humanoid);
            }
        }

        private void RegisterOwner(IHumanoid Humanoid)
        {
            UpdateCurrentModel(Humanoid);
            Humanoid.Model.AddModel(_currentModel);
        }

        private void UnregisterOwner(IHumanoid Humanoid)
        {
            Humanoid.Model.RemoveModel(_currentModel);
        }

        private void UpdateCurrentModel(IHumanoid Humanoid)
        {
            _currentModel = _originalModel.Clone();
            HumanoidModel.PaintModelWithCustomization(Humanoid, _currentModel);
        }
    }
}