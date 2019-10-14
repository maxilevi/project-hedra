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
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.Rendering.UI;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{

    public delegate void OnModelAdded(CachedVertexData Models);
    public delegate void OnInstanceAdded(InstanceData Instance);

    public class CollidableStructure : IDisposable
    {
        private readonly HashSet<CollisionGroup> _colliders;
        private readonly HashSet<CachedVertexData> _models;
        private readonly HashSet<InstanceData> _instances;
        private readonly HashSet<IGroundwork> _groundworks;
        private readonly HashSet<BasePlateau> _plateaus;
        private readonly object _lock = new object();
        public event OnModelAdded ModelAdded;
        public event OnInstanceAdded InstanceAdded;
        public Vector3 Position { get; }
        public Vector2 MapPosition { get; set; }
        public RoundedPlateau Mountain { get; }
        public BaseStructure WorldObject { get; }
        public float Radius { get; private set; }
        public StructureDesign Design { get; }
        public AttributeArray Parameters { get; }
        public bool Built { get; set; }
        public bool Disposed { get; private set; }
        private bool _structureSetup;

        public CollidableStructure(StructureDesign Design, Vector3 Position, RoundedPlateau Mountain, BaseStructure WorldObject)
        {
            this.Position = new Vector3(Position.X, (Mountain?.MaxHeight + 1) * Chunk.BlockSize ?? Position.Y, Position.Z);
            this.Mountain = Mountain;
            this.Design = Design;
            this.WorldObject = WorldObject;
            this.Parameters = new AttributeArray();
            this._colliders = new HashSet<CollisionGroup>();
            this._models = new HashSet<CachedVertexData>();
            this._groundworks = new HashSet<IGroundwork>();
            this._plateaus = new HashSet<BasePlateau>();
            this._instances = new HashSet<InstanceData>();
        }

        public void Setup()
        {
            World.WorldBuilding.SetupStructure(this);
            _structureSetup = true;
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
        
        public InstanceData[] Instances
        {
            get
            {
                lock(_lock)
                    return _instances.ToArray();
            }
        }
        
        public BasePlateau[] Plateaux
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

        public void AddCollisionShape(params CollisionShape[] IColliders)
        {
            if (IColliders.Length == 0) return;
            lock (_lock)
            {
                _colliders.Add(new CollisionGroup(IColliders));
            }
        }
        public void AddCollisionGroup(CollisionGroup Group)
        {
            lock (_lock)
            {
                _colliders.Add(Group);
            }
        }

        public void AddStaticElement(VertexData[] Models, bool Ungroup = true)
        {
            AddStaticElement(Ungroup ? Models.SelectMany(M => M.Ungroup()).ToArray() : Models.ToArray());
        }
        
        public void AddStaticElement(VertexData Model, bool Ungroup = true)
        {
            AddStaticElement(Ungroup ? Model.Ungroup() : new []{Model});
        }

        private void AddStaticElement(VertexData[] Models)
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
        
        public void AddInstance(params InstanceData[] Instances)
        {
            lock (_lock)
            {
                for (var i = 0; i < Instances.Length; i++)
                {
                    _instances.Add(Instances[i]);
                    InstanceAdded?.Invoke(Instances[i]);
                }
                this.CalculateRadius();
            }
        }
        
        public void AddGroundwork(params IGroundwork[] Groundworks)
        {
            if(_structureSetup) throw new ArgumentOutOfRangeException("Cannot add groundworks after the structure has been setup.");
            lock (_lock)
            {
                for (var i = 0; i < Groundworks.Length; i++)
                {
                    _groundworks.Add(Groundworks[i]);
                }
            }
        }
        
        public void AddPlateau(params BasePlateau[] RoundedPlateaux)
        {
            if(_structureSetup) throw new ArgumentOutOfRangeException("Cannot add plateaus after the structure has been setup.");
            lock (_lock)
            {
                for (var i = 0; i < RoundedPlateaux.Length; i++)
                {
                    _plateaus.Add(RoundedPlateaux[i]);
                }
            }
        }

        private void CalculateRadius()
        {
            var radius = 0f;
            foreach (var model in _models)
            {
                var newRadius = (model.Position - this.Position).LengthFast() + model.Bounds.Xz().LengthFast() * .5f;
                if (newRadius > radius)
                    radius = newRadius;
            }
            foreach (var instance in _instances)
            {
                var newRadius = (instance.Position - this.Position).LengthFast() + instance.ApproximateBounds.Xz().LengthFast() * .5f;
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
