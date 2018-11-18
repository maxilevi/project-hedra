using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class DoubleArmorPiece : ArmorPiece
    {
        protected override Matrix4 PlacementMatrix => default(Matrix4);
        protected override Vector3 PlacementPosition => default(Vector3);
        protected abstract Matrix4 LeftPlacementMatrix { get; }
        protected abstract Matrix4 RightPlacementMatrix { get; }
        protected abstract Vector3 LeftPlacementPosition { get; }
        protected abstract Vector3 RightPlacementPosition { get; }

        protected DoubleArmorPiece(VertexData Model) : base(Model)
        {
        }
    }
}
