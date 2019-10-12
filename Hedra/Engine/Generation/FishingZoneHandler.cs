using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using IronPython.Runtime;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Generation
{
    public class FishingZoneHandler
    {
        private readonly List<FishingZone> _zones;

        public FishingZoneHandler()
        {
            _zones = new List<FishingZone>();
        }
        
        public void AddZone(FishingZone Zone)
        {
            _zones.Add(Zone);
        }

        public void RemoveZone(FishingZone Zone)
        {
            _zones.Remove(Zone);
            Zone.Dispose();
        }

        public void Discard()
        {
            var zones = Zones;
            for (var i = 0; i < zones.Length; ++i)
            {
                RemoveZone(zones[i]);
            }
        }

        public FishingZone[] Zones => _zones.ToArray();
    }

    public class FishingZone
    {
        private readonly HighlightedAreaWrapper _area;
        private readonly Vector3 _zone;
        private readonly float _radius;
        public float Chance { get; }
        public Item FishingReward { get; }

        public FishingZone(Vector3 Zone, Vector4 Color, float Radius, float Chance, Item FishingReward)
        {
            this.FishingReward = FishingReward;
            this.Chance = Chance;
            _radius = Radius;
            _zone = Zone;
            _area = World.Highlighter.HighlightAreaPermanently(_zone, Color, _radius * 2f);
            _area.OnlyWater = true;
        }

        public bool Affects(Vector3 Position)
        {
            return (Position - _zone).Xz.LengthSquared < _radius * _radius;
        }

        public void Dispose()
        {
            _area.Dispose();
        }
    }
}