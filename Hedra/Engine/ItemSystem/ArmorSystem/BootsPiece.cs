using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class BootsPiece : DoubleArmorPiece
    {
        protected override Matrix4 LeftPlacementMatrix => Owner.Model.LeftFootMatrix;

        protected override Vector3 LeftPlacementPosition => Owner.Model.LeftFootPosition;
              
        protected override Matrix4 RightPlacementMatrix => Owner.Model.RightFootMatrix;

        protected override Vector3 RightPlacementPosition => Owner.Model.LeftFootPosition;

        public BootsPiece(VertexData Model) : base(Model)
        {
        }

    }
}
