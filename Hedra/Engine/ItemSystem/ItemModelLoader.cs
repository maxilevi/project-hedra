using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Rendering;
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
                ModelCache.Add(path, ProcessPath(path));
            }

            var returnModel = ModelCache[path].Clone();
            returnModel.Transform( Matrix4.CreateScale(ModelTemplate.Scale));
            return returnModel;
        }

        private static VertexData ProcessPath(string Path)
        {
            VertexData model;
            if (Path.EndsWith(".dae"))
                model = AdjustModel(AssetManager.DAELoader(Path).ToVertexData());
            else if (Path.EndsWith(".ply"))
                model = AssetManager.PLYLoader(Path, Vector3.One);
            else
                throw new ArgumentOutOfRangeException($"Unsupported model '{Path}'.");
            return model;
        }
        
        private static VertexData AdjustModel(VertexData Model)
        {
            //var center = Model.Vertices.Aggregate( (V1,V2) => V1+V2) / Model.Vertices.Count;
           // Model.Vertices = Model.Vertices.Select(V => V-center).ToList();
            
            return Model;
        }
    }
}
