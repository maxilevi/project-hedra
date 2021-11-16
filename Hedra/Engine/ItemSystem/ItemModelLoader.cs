using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.ItemSystem
{
    public static class ItemModelLoader
    {
        private static readonly Dictionary<string, VertexData> ModelCache;
        private static readonly object Lock = new object();

        static ItemModelLoader()
        {
            ModelCache = new Dictionary<string, VertexData>();
        }

        public static VertexData Load(ItemModelTemplate ModelTemplate)
        {
            var path = ModelTemplate.Path;
            lock (Lock)
            {
                if (!ModelCache.ContainsKey(path))
                    ModelCache.Add(path, AdjustModel(AssetManager.LoadModel(path, Vector3.One)));
            }

            var returnModel = ModelCache[path].Clone();
            returnModel.Transform(Matrix4x4.CreateScale(ModelTemplate.Scale));
            return returnModel;
        }

        private static VertexData AdjustModel(VertexData Model)
        {
            //var center = Model.Vertices.Aggregate( (V1,V2) => V1+V2) / Model.Vertices.Count;
            // Model.Vertices = Model.Vertices.Select(V => V-center).ToList();

            return Model;
        }
    }
}