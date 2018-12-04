using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public abstract class CacheType
    {
        private readonly List<VertexData> _models = new List<VertexData>();
        private readonly List<List<CollisionShape> > _shapes = new List< List<CollisionShape> >();

        public abstract CacheItem Type { get; }
        
        protected void AddModel(VertexData Model)
        {
            _models.Add(Model);
        }

        protected void AddShapes(List<CollisionShape> Shapes)
        {
            _shapes.Add(Shapes);
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
