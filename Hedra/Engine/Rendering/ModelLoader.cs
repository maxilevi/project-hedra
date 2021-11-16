using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation.ColladaParser;

namespace Hedra.Engine.Rendering
{
    public static class ModelLoader
    {
        private static readonly Dictionary<string, ModelData> ModelCache = new Dictionary<string, ModelData>();

        public static ModelData Load(string Path)
        {
            if (!ModelCache.ContainsKey(Path))
            {
                var model = AssetManager.DAELoader(Path);
                ModelCache[Path] = model;
            }

            return ModelCache[Path].Clone();
        }
    }
}