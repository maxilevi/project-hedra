using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithFarm : BaseStructure, IQuestStructure
    {
        private HighlightedAreaWrapper _area;
        private readonly float _radius;
        public CottageWithFarm(Vector3 Position, float Radius) : base(Position)
        {
            _radius = Radius;
        }

        public IHumanoid NPC { get; set; }

        public void MakePossessed()
        {
            NPC?.Dispose();
            NPC = null;
            _area = World.Highlighter.HighlightAreaPermanently(Position, new Vector4(.2f, .2f, .2f, 1f), _radius);
        }

        public void MakeNormal()
        {
            SoundPlayer.PlaySound(SoundType.DarkSound, GameManager.Player.Position);
            _area.Dispose();
            _area = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
        }
    }
}