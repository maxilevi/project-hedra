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
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{

    public delegate void OnModelAdded(CachedVertexData Models);

    public class CollidableStructure : IDisposable
    {
        private readonly HashSet<CollisionGroup> _colliders;
        private readonly HashSet<CachedVertexData> _models;
        private readonly HashSet<IGroundwork> _groundworks;
        private readonly HashSet<Plateau> _plateaus;
        private readonly object _lock = new object();
        public event OnModelAdded ModelAdded;
        public Vector3 Position { get; }
        public Plateau Mountain { get; }
        public BaseStructure WorldObject { get; }
        public float Radius { get; private set; }
        public StructureDesign Design { get; }
        public AttributeArray Parameters { get; }
        public bool Built { get; set; }
        public bool Disposed { get; private set; }

        public CollidableStructure(StructureDesign Design, Vector3 Position, Plateau Mountain, BaseStructure WorldObject)
        {
            this.Position = new Vector3(Position.X, (Mountain?.MaxHeight + 1) * Chunk.BlockSize ?? Position.Y, Position.Z);
            this.Mountain = Mountain;
            this.Design = Design;
            this.WorldObject = WorldObject;
            this.Parameters = new AttributeArray();
            this._colliders = new HashSet<CollisionGroup>();
            this._models = new HashSet<CachedVertexData>();
            this._groundworks = new HashSet<IGroundwork>();
            this._plateaus = new HashSet<Plateau>();
        }

        public void Setup()
        {
            World.WorldBuilding.SetupStructure(this);
        }

        public CollisionGroup[] Colliders
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
        
        public Plateau[] Plateaus
        {
            get
            {
                lock(_lock)
                    return _plateaus.ToArray();
            }
        }
        
        public IGroundwork[] Groundworks
        {
            get
            {
                lock(_lock)
                    return _groundworks.ToArray();
            }
        }

        public void AddCollisionShape(params ICollidable[] IColliders)
        {
            lock (_lock)
            {
                _colliders.Add(new CollisionGroup(IColliders));
            }
        }

        public void AddStaticElement(params VertexData[] Models)
        {
            lock (_lock)
            {
                for (var i = 0; i < Models.Length; i++)
                {
                    var model = CachedVertexData.FromVertexData(Models[i]);
                    _models.Add(model);
                    ModelAdded?.Invoke(model);
                }
                this.CalculateRadius();
            }
        }
        
        public void AddGroundwork(params IGroundwork[] Groundworks)
        {
            lock (_lock)
            {
                for (var i = 0; i < Groundworks.Length; i++)
                {
                    _groundworks.Add(Groundworks[i]);
                }
            }
        }
        
        public void AddPlateau(params Plateau[] Plateaus)
        {
            lock (_lock)
            {
                for (var i = 0; i < Plateaus.Length; i++)
                {
                    _plateaus.Add(Plateaus[i]);
                }
            }
        }

        public bool CanAddPlateau(Plateau Mount)
        {
            lock(_lock)
                return World.WorldBuilding.CanAddPlateau(Mount, _plateaus.ToArray());
        }

        private void CalculateRadius()
        {
            var radius = 0f;
            foreach (var model in _models)
            {
                var newRadius = (model.Position - this.Position).LengthFast + model.Bounds.Xz.LengthFast * .5f;
                if (newRadius > radius)
                    radius = newRadius;
            }
            this.Radius = radius;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var model in _models)
                {
                    model.Dispose();
                }
            }
            Disposed = true;
            World.WorldBuilding.DisposeStructure(this);
            WorldObject?.Dispose();
        }
    }
}
