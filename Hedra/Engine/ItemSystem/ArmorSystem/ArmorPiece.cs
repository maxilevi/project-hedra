using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public abstract class ArmorPiece
    {
        protected Humanoid Owner { get; private set; }
        public abstract Matrix4 PlacementMatrix { get; }
    }
}
