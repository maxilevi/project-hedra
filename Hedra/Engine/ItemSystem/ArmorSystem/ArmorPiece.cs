using System;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class ArmorPiece : IDisposable, IModel
    {
        public bool Disposed { get; set; }
        protected IHumanoid Owner { get; private set; }
        private readonly ModelData _model;

        protected ArmorPiece(ModelData Model)
        {
            _model = Model;
        }
        
        public void Update(IHumanoid Humanoid)
        {
            UpdateOwner(Humanoid);
        }

        private void UpdateOwner(IHumanoid Humanoid)
        {
            if (Humanoid != Owner)
            {
                if(Owner != null) 
                    UnregisterOwner(Owner);
                Owner = Humanoid;
                if(Humanoid != null)
                    RegisterOwner(Humanoid);
            }
        }

        private void RegisterOwner(IHumanoid Humanoid)
        {
            Humanoid.Model.AddModel(_model);
        }
        
        private void UnregisterOwner(IHumanoid Humanoid)
        {
            Humanoid.Model.RemoveModel(_model);
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
        public ModelData Model => _model;

        public void Dispose()
        {
            if(Owner != null) UnregisterOwner(Owner);
            Disposed = true;
        }
    }
}
