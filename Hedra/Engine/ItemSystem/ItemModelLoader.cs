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
            if (!ModelCache.ContainsKey(path))
            {
                var model = AssetManager.PlyLoader(path, Vector3.One);
                model = AdjustModel(model);
                ModelCache.Add(path, model);
            }

            var returnModel = ModelCache[path].Clone();
            returnModel.Transform( Matrix3.CreateScale(ModelTemplate.Scale));
            return returnModel;
        }

        private static VertexData AdjustModel(VertexData Model)
        {
            //TODO
            return Model;
        }
    }
}
