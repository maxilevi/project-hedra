/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/08/2017
 * Time: 12:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.Generation
{

    public delegate void OnModelAdded(CachedVertexData Models);

    public class CollidableStructure : IDisposable
    {
        public event OnModelAdded OnModelAdded;
        public Vector3 Position { get; }
        public Plateau Mountain { get; }
        public float Radius { get; private set; }
        private readonly List<ICollidable> _colliders;
        private readonly List<CachedVertexData> _models;
        public StructureDesign Design { get; }
        public AttributeArray Parameters { get; }
        private readonly object _lock = new object();

        public CollidableStructure(StructureDesign Design, Vector3 Position, Plateau Mountain)
        {
            this.Position = new Vector3(Position.X, (Mountain?.MaxHeight + 1) * Chunk.BlockSize ?? Position.Y, Position.Z);
            this.Mountain = Mountain;
            this.Design = Design;
            this.Parameters = new AttributeArray();
            this._colliders = new List<ICollidable>();
            this._models = new List<CachedVertexData>();
        }

        public ICollidable[] Colliders
        {
            get
            {
                lock(_lock)
                    return _colliders.ToArray();
            }
        }
        
        public CachedVertexData[] Models
        {
            get
            {
                lock(_lock)
                    return _models.ToArray();
            }
        }

        public void AddCollisionShape(params ICollidable[] IColliders)
	    {
	        lock (_lock)
	        {
                for(var i = 0; i < IColliders.Length; i++)
	                _colliders.Add(IColliders[i]);
	        }
	    }

        public void AddStaticElement(params VertexData[] Models)
        {
            lock (_lock)
            {
                for (var i = 0; i < Models.Length; i++)
                {
                    _models.Add(CachedVertexData.FromVertexData(Models[i]));
                    OnModelAdded?.Invoke(_models[_models.Count-1]);
                }
                this.CalculateRadius();
            }
        }

        private void CalculateRadius()
        {
            var radius = 0f;
            for (var i = 0; i < _models.Count; i++)
            {
                var newRadius = (_models[i].Position - this.Position).LengthFast + _models[i].Bounds.Xz.LengthFast * .5f;
                if (newRadius > radius)
                    radius = newRadius;
            }
            this.Radius = radius;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                for (var i = 0; i < _models.Count; i++)
                {
                    _models[i].Dispose();
                }
            }
        }
	}
}
