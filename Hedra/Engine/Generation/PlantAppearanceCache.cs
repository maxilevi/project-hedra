using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Rendering;
using Hedra.Framework;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{
    // A single chunk's placement pass owns this cache. It never outlives world caches.
    public sealed class PlantAppearanceCache
    {
        private static readonly bool Verify = Environment.GetEnvironmentVariable("HEDRA_VERIFY_PLANT_PAINT_CACHE") == "1";
        private readonly Dictionary<(Type, VertexData, PreparedPlantPaint), Appearance> _appearances = new();
        private long _hits;
        private long _misses;

        public sealed class Appearance
        {
            public object ColorCache { get; init; }
            public object ExtraDataCache { get; init; }
            public Vector4[] Colors { get; init; }
            public float[] ExtraData { get; init; }
        }

        public bool TryGet(PlantDesign Design, VertexData Model, PreparedPlantPaint Paint, IAllocator Allocator,
            out Appearance Appearance)
        {
            if (!_appearances.TryGetValue((Design.GetType(), Model, Paint), out Appearance))
            {
                _misses++;
                return false;
            }
            _hits++;
            if (Verify)
            {
                var reference = Model.NativeClone(Allocator);
                try
                {
                    Paint.Apply(reference);
                    if (!reference.Colors.SequenceEqual(Appearance.Colors) || !reference.Extradata.SequenceEqual(Appearance.ExtraData))
                        throw new InvalidOperationException($"Cached plant paint differs: {Design.GetType().Name}");
                }
                finally { reference.Dispose(); }
            }
            return true;
        }

        public void Add(PlantDesign Design, VertexData Model, PreparedPlantPaint Paint, InstanceData Instance, NativeVertexData Painted)
        {
            _appearances[(Design.GetType(), Model, Paint)] = new Appearance
            {
                ColorCache = Instance.ColorCache,
                ExtraDataCache = Instance.ExtraDataCache,
                Colors = Verify ? Painted.Colors.ToArray() : null,
                ExtraData = Verify ? Painted.Extradata.ToArray() : null
            };
        }

        public void Report()
        {
            GenerationMetrics.Count("plant_cache_hits", _hits);
            GenerationMetrics.Count("plant_cache_misses", _misses);
            if (Verify) GenerationMetrics.Count("plant_cache_verified", _hits);
        }
    }
}
