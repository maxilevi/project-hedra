using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Game;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithFarm : Cottage
    {
        private HighlightedAreaWrapper _area;

        public CottageWithFarm(Vector3 Position, float Radius) : base(Position, Radius)
        {
        }

        public void MakePossessed()
        {
            TaskScheduler.When(() => NPC != null, () => NPC.Dispose());
            _area = World.Highlighter.HighlightAreaPermanently(Position, new Vector4(.2f, .2f, .2f, 1f), Radius);
        }

        public void MakeNormal()
        {
            SoundPlayer.PlaySound(SoundType.DarkSound, GameManager.Player.Position);
            _area.Dispose();
            _area = null;
        }
    }
}