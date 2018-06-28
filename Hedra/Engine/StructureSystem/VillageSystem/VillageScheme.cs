using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal abstract class VillageScheme
    {
        private readonly Dictionary<Vector3, float> _positions;

        protected VillageScheme()
        {
            _positions = new Dictionary<Vector3, float>();
        }

        protected void AddGroundwork(Vector3 Position, float Radius)
        {
            _positions.Add(Position, Radius);
        }

        public void PlaceGroundwork(Vector3 TargetPosition, float MaxHeight)
        {
            foreach (var pair in _positions)
            {
                World.QuestManager.AddPlateau(new Plateau(TargetPosition + pair.Key, pair.Value, 800, MaxHeight));
                World.QuestManager.AddVillagePosition(TargetPosition + pair.Key, pair.Value);
            }
        }

        public abstract Vector3 StablePosition { get; }
        public abstract Vector3 BlacksmithPosition { get; }
        public abstract Vector3 MarketPosition { get; }
        public abstract Vector3 WindmillPosition { get; }
        public abstract Vector3 FarmPosition { get; }
    }
}
