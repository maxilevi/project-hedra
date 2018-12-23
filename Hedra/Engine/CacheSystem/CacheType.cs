using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public abstract class CacheType
    {
        private readonly List<VertexData> _models = new List<VertexData>();
        private readonly List<VertexData> _modelParts = new List<VertexData>();
        private readonly List<List<CollisionShape>> _shapes = new List<List<CollisionShape>>();

        public abstract CacheItem Type { get; }
        
        protected void AddModel(VertexData Model)
        {
            _models.Add(Model);
        }
        
        protected void AddModelPart(VertexData Part)
        {
            _modelParts.Add(Part);
        }

        protected void AddShapes(List<CollisionShape> Shapes)
        {
            _shapes.Add(Shapes);
        }

        public VertexData GetPart(VertexData Model)
        {
            var index = _models.IndexOf(Model);
            return index != -1 ? _modelParts[index] : null;
        }
        
        public VertexData GrabModel()
        {
            return _models[Utils.Rng.Next(0, _models.Count)];
        }

        public List<CollisionShape> GetShapes(VertexData Model)
        {
            for (var i = 0; i < _models.Count; i++)
            {
                if (_models[i] == Model) return _shapes[i];
            }
            return null;
        }
    }
}
