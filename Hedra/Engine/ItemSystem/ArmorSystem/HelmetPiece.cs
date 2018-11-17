using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class HelmetPiece : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => Owner.Model.HeadMatrix;

        public HelmetPiece(VertexData Model) : base(Model)
        {
        }
    }
}