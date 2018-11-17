using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class BootsPiece : DoubleArmorPiece
    {
        public override Matrix4 LeftPlacementMatrix => Owner.Model.LeftFootMatrix;
        public override Matrix4 RightPlacementMatrix => Owner.Model.RightFootMatrix;

        public BootsPiece(VertexData Model) : base(Model)
        {
        }
    }
}
