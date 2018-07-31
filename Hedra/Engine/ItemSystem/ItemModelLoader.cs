using System.Collections.Generic;
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
                var model = AssetManager.PLYLoader(path, Vector3.One);
                model = AdjustModel(model);
                ModelCache.Add(path, model);
            }

            var returnModel = ModelCache[path].Clone();
            returnModel.Transform( Matrix4.CreateScale(ModelTemplate.Scale));
            return returnModel;
        }

        private static VertexData AdjustModel(VertexData Model)
        {
            //var center = Model.Vertices.Aggregate( (V1,V2) => V1+V2) / Model.Vertices.Count;
            //Model.Vertices = Model.Vertices.Select(V => V-center).ToList();
            return Model;
        }
    }
}
