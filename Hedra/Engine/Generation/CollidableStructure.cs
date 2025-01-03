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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.Generation
{
    public delegate void OnModelAdded(CachedVertexData Models);

    public delegate void OnInstanceAdded(InstanceData Instance);

    public class CollidableStructure : IDisposable
    {
        private readonly HashSet<CollisionGroup> _colliders;
        private readonly HashSet<IGroundwork> _groundworks;
        private readonly HashSet<InstanceData> _instances;
        private readonly object _lock = new object();
        private readonly HashSet<CachedVertexData> _models;
        private readonly HashSet<BasePlateau> _plateaus;
        private bool _structureSetup;

        public CollidableStructure(StructureDesign Design, Vector3 Position, RoundedPlateau Mountain,
            BaseStructure WorldObject)
        {
            this.Position = Position;
            this.Mountain = Mountain;
            this.Design = Design;
            this.WorldObject = WorldObject;
            Parameters = new AttributeArray();
            _colliders = new HashSet<CollisionGroup>();
            _models = new HashSet<CachedVertexData>();
            _groundworks = new HashSet<IGroundwork>();
            _plateaus = new HashSet<BasePlateau>();
            _instances = new HashSet<InstanceData>();
            Reposition();
        }

        public Vector3 Position { get; private set; }
        public Vector2 MapPosition { get; set; }
        public RoundedPlateau Mountain { get; }
        public BaseStructure WorldObject { get; }
        public WaypointGraph Waypoints { get; set; }
        public float Radius { get; private set; }
        public StructureDesign Design { get; }
        public AttributeArray Parameters { get; }
        public bool Built { get; set; }
        public bool Disposed { get; private set; }
        public int ActiveQuests { get; set; }

        public CollisionGroup[] Colliders
        {
            get
            {
                lock (_lock)
                {
                    return _colliders.ToArray();
                }
            }
        }

        public CachedVertexData[] Models
        {
            get
            {
                lock (_lock)
                {
                    return _models.ToArray();
                }
            }
        }

        public InstanceData[] Instances
        {
            get
            {
                lock (_lock)
                {
                    return _instances.ToArray();
                }
            }
        }

        public BasePlateau[] Plateaux
        {
            get
            {
                lock (_lock)
                {
                    return _plateaus.ToArray();
                }
            }
        }

        public IGroundwork[] Groundworks
        {
            get
            {
                lock (_lock)
                {
                    return _groundworks.ToArray();
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var model in _models) model.Dispose();
            }

            Disposed = true;
            World.WorldBuilding.DisposeStructure(this);
            WorldObject?.Dispose();
        }

        public event OnModelAdded ModelAdded;
        public event OnInstanceAdded InstanceAdded;

        public void Draw()
        {
            if (!GameSettings.DebugNavMesh) return;
            Waypoints?.Draw();
        }

        public void Setup()
        {
            World.WorldBuilding.SetupStructure(this);
            _structureSetup = true;
        }

        public void Reposition()
        {
            Position = new Vector3(Position.X, (Mountain?.MaxHeight + 1) * Chunk.BlockSize ?? Position.Y, Position.Z);
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
            AddStaticElement(Ungroup ? Model.Ungroup() : new[] { Model });
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

                CalculateRadius();
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

                CalculateRadius();
            }
        }

        public void AddGroundwork(params IGroundwork[] Groundworks)
        {
            if (_structureSetup)
                throw new ArgumentOutOfRangeException("Cannot add groundworks after the structure has been setup.");
            lock (_lock)
            {
                for (var i = 0; i < Groundworks.Length; i++) _groundworks.Add(Groundworks[i]);
            }
        }

        public void AddPlateau(params BasePlateau[] RoundedPlateaux)
        {
            if (_structureSetup)
                throw new ArgumentOutOfRangeException("Cannot add plateaus after the structure has been setup.");
            lock (_lock)
            {
                for (var i = 0; i < RoundedPlateaux.Length; i++) _plateaus.Add(RoundedPlateaux[i]);
            }
        }

        private void CalculateRadius()
        {
            var radius = 0f;
            foreach (var model in _models)
            {
                var newRadius = (model.Position - Position).LengthFast() + model.Bounds.Xz().LengthFast() * .5f;
                if (newRadius > radius)
                    radius = newRadius;
            }

            foreach (var instance in _instances)
            {
                var newRadius = (instance.Position - Position).LengthFast() +
                                instance.ApproximateBounds.Xz().LengthFast() * .5f;
                if (newRadius > radius)
                    radius = newRadius;
            }

            Radius = radius;
            if (Radius > 2000) Debugger.Break();
        }
    }
}