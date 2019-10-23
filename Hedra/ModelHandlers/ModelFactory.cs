using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Numerics;

namespace Hedra.ModelHandlers
{
    public abstract class ModelFactory
    {
        protected abstract PartGroup[] Parts { get; }
        private readonly ProcessedPartGroup[] _processedGroups;

        protected ModelFactory()
        {
            _processedGroups = Process(Parts);
        }

        private static ProcessedPartGroup[] Process(params PartGroup[] Parts)
        {
            return Parts.Select(P => new ProcessedPartGroup
            {
                Parts = P.Paths.Select(AssetManager.DAELoader).ToArray(),
                Optional = P.Optional,
                Max = P.Max
            }).ToArray();
        }
        
        public void Build(AnimatedUpdatableModel Model)
        {
            var rng = Utils.Rng;
            Model.ClearModel();
            for (var i = 0; i < _processedGroups.Length; ++i)
            {
                var shouldAddAnother = true;
                for (var k = 0; k < _processedGroups[i].Max && shouldAddAnother; ++k)
                {
                    var model = _processedGroups[i].Get(rng.NextFloat());
                    if (model != null) Model.AddModel(model);
                    shouldAddAnother &= rng.NextFloat() > 1f / (k + 2);
                }
            }
        }

        private class ProcessedPartGroup
        {
            public ModelData[] Parts { get; set; }
            public bool Optional { get; set; }
            public int Max { get; set; }

            public ModelData Get(float Chance)
            {
                var weight = 1f / (Parts.Length + (Optional ? 1 : 0));
                var index = (int) Math.Floor(Chance / weight);
                if (index >= Parts.Length) return null;
                return Parts[index];
            }
        }
    }

    public class PartGroup
    {
        public string[] Paths { get; set; }
        public bool Optional { get; set; }
        public int Max { get; set; } = 1;
    }
}