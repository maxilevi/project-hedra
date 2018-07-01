using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    internal abstract class DoubleArmorPiece : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => default(Matrix4);
        public abstract Matrix4 LeftPlacementMatrix { get; }
        public abstract Matrix4 RightPlacementMatrix { get; }
    }
}
