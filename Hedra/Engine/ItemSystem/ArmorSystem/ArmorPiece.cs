using System;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class ArmorPiece : IModel, IDisposable
    {
        public bool Disposed { get; set; }
        protected IHumanoid Owner { get; private set; }
        public abstract Matrix4 PlacementMatrix { get; }

        protected ArmorPiece(VertexData Model)
        {
            
        }
        
        public void Update(IHumanoid Humanoid)
        {
            Owner = Humanoid;
        }

        public Vector4 Tint { get; set; }
        public Vector4 BaseTint { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool Enabled { get; set; }
        public bool ApplyFog { get; set; }
        public bool Pause { get; set; }
        public float Alpha { get; set; }
        public float AnimationSpeed { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
