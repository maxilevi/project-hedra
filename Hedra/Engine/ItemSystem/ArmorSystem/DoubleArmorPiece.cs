using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class DoubleArmorPiece : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => default(Matrix4);
        public abstract Matrix4 LeftPlacementMatrix { get; }
        public abstract Matrix4 RightPlacementMatrix { get; }

        protected DoubleArmorPiece(VertexData Model) : base(Model)
        {
        }
    }
}
