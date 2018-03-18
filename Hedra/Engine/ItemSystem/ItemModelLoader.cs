using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemModelLoader
    {
        private static readonly Dictionary<string, VertexData> ModelCache;

        static ItemModelLoader()
        {
            ModelCache = new Dictionary<string, VertexData>();
        }

        public static VertexData Load(ItemModelTemplate ModelTemplate)
        {
            var path = ModelTemplate.Path;
            if (ModelCache.ContainsKey(path)) return ModelCache[path].Clone();

            var model = AssetManager.PlyLoader(path, Vector3.One * ModelTemplate.Scale);
            model = AdjustModel(model);

            ModelCache.Add(path, model);

            return ModelCache[path].Clone();
        }

        private static VertexData AdjustModel(VertexData Model)
        {
            //TODO
            return Model;
        }
    }
}
