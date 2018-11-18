using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class HelmetPiece : ArmorPiece
    {
        protected override Matrix4 PlacementMatrix => Owner.Model.HeadMatrix;

        protected override Vector3 PlacementPosition => Owner.Model.HeadPosition;

        public HelmetPiece(VertexData Model) : base(Model)
        {
        }
    }
}