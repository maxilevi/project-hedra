using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.PhysicsSystem;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public abstract class CacheType
    {
        private readonly List<VertexData> _modelParts = new List<VertexData>();
        private readonly List<VertexData> _models = new List<VertexData>();
        private readonly List<List<CollisionShape>> _shapes = new List<List<CollisionShape>>();

        public abstract CacheItem Type { get; }

        public int UsedBytes => _models.Sum(M => M.SizeInBytes) + _modelParts.Sum(M => M.SizeInBytes) +
                                _shapes.SelectMany(S => S).Sum(S => S.SizeInBytes);

        protected void AddModel(VertexData Model)
        {
            Model.Name = Type.ToString();
            _models.Add(Model);
        }

        protected void AddModelPart(VertexData Part)
        {
            Part.Name = Type.ToString();
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
                if (_models[i] == Model)
                    return _shapes[i];
            return null;
        }
    }
}