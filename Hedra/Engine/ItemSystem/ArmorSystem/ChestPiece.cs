using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class ChestPiece : ArmorPiece
    {
        protected override Matrix4 PlacementMatrix => Owner.Model.ChestMatrix;

        protected override Vector3 PlacementPosition => Owner.Model.ChestPosition;

        public ChestPiece(VertexData Model) : base(Model)
        {
        }
    }
}
