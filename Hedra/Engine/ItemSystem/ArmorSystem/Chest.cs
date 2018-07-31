using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public class Chest : ArmorPiece
    {
        public override Matrix4 PlacementMatrix => Owner.Model.ChestMatrix;
    }
}
