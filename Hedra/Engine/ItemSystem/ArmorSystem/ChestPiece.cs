using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class ChestPiece : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => Owner.Model.ChestMatrix;

        public ChestPiece(VertexData Model) : base(Model)
        {
        }
    }
}
