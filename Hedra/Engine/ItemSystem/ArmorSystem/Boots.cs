using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    internal class Boots : DoubleArmorPiece
    {
        public override Matrix4 LeftPlacementMatrix => Owner.Model.LeftFootMatrix;
        public override Matrix4 RightPlacementMatrix => Owner.Model.RightFootMatrix;
    }
}
