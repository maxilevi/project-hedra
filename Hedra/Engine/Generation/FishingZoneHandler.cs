using System.Collections.Generic;
using IronPython.Runtime;
using OpenTK;

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

        public FishingZone(Vector3 Zone, Vector4 Color, float Radius)
        {
            _area = World.Highlighter.HighlightAreaPermanently(Zone, Color, Radius * 2f);
            _area.OnlyWater = true;
        }

        public void Dispose()
        {
            _area.Dispose();
        }
    }
}