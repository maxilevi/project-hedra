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
        protected abstract Matrix4 PlacementMatrix { get; }
        protected abstract Vector3 PlacementPosition { get; }
        protected ObjectMesh Mesh { get; }

        protected ArmorPiece(VertexData Model)
        {
            Mesh = ObjectMesh.FromVertexData(Model);
        }
        
        public void Update(IHumanoid Humanoid)
        {
            Owner = Humanoid;
            Mesh.TransformationMatrix = /*Owner.Class.
                                        * */PlacementMatrix.ClearTranslation() 
                                        * Matrix4.CreateTranslation(-Owner.Model.Position + PlacementPosition);
            Mesh.Position = Owner.Model.Position;
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
            Mesh.Dispose();
            Disposed = true;
        }
    }
}
