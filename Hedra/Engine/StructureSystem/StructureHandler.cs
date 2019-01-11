  /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Game;

namespace Hedra.Engine.StructureSystem
{
    /// <summary>
    /// Description of StructureGenerator.
    /// </summary>
    public class StructureHandler
    {
        private static readonly RandomDistribution Distribution;
        private readonly object _lock = new object();
        public Vector3 MerchantPosition { get; set; }
        public bool SpawnCampfireSpawned { get; set; }
        public bool SpawnVillageSpawned { get; set; }
        public bool MerchantSpawned { get; set; }
        public Voronoi SeedGenerator { get; }
        private readonly List<StructureWatcher> _itemWatchers;
        private CollidableStructure[] _itemsCache;
        private BaseStructure[] _structureCache;
        private bool _dirtyStructuresItems;
        private bool _dirtyStructures;

        static StructureHandler()
        {
            Distribution = new RandomDistribution(true);
        }
        
        public StructureHandler()
        {
            _itemWatchers = new List<StructureWatcher>();
            SeedGenerator = new Voronoi();
            World.OnChunkDisposed += OnChunkDisposed;
        }

        private void OnChunkDisposed(Chunk Chunk)
        {
            lock (_lock)
            {
                for (var i = _itemWatchers.Count-1; i > -1; --i)
                {
                    var item = _itemWatchers[i];
                    if (ShouldRemove(item.Structure))
                    {
                        item.Dispose();
                        _itemWatchers.RemoveAt(i);
                    }
                }
                Dirty();
            }
        }

        private static bool ShouldRemove(CollidableStructure Structure)
        {
            /* Offset is not used because this causes issues when chunks are deleted at chunk edges */
            return Structure.Design.ShouldRemove(Structure) && Structure.Built;
        }

        public static void CheckStructures(Vector2 ChunkOffset)
        {
            if (!World.IsChunkOffset(ChunkOffset))
                throw new ArgumentException("Provided parameter does not represent a valid offset");

            var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
            var region = underChunk != null
                ? underChunk.Biome
                : World.BiomePool.GetRegion(ChunkOffset.ToVector3());
            var designs = region.Structures.Designs;
            for (var i = 0; i < designs.Length; i++)
            {
                if (designs[i].MeetsRequirements(ChunkOffset))
                    designs[i].CheckFor(ChunkOffset, region, Distribution);
            }
        }

        public void Build(CollidableStructure Struct)
        {
            Struct.Design.Build(Struct);
            Struct.Built = true;
        }
        
        public void AddStructure(CollidableStructure Structure)
        {
            lock (_lock)
            {
                _itemWatchers.Add(new StructureWatcher(Structure));
                Structure.Setup();
                Dirty();
            }
        }

        public bool Has(CollidableStructure Structure)
        {
            lock (_lock)
                return StructureItems.Any(S => S == Structure);
        }

        public static CollidableStructure[] GetNearStructures(Vector3 Position)
        {
            return (from item in World.StructureHandler.StructureItems
                where (item.Position.Xz - Position.Xz).LengthSquared < item.Radius * item.Radius
                select item).ToArray();
        }

        public void Discard()
        {
            this.MerchantPosition = Vector3.Zero;
            this.MerchantSpawned = false;
            this.SpawnCampfireSpawned = false;
            lock (_lock)
            {
                for (var i = _itemWatchers.Count - 1; i > -1; i--)
                {
                    _itemWatchers[i].Dispose();
                    _itemWatchers.RemoveAt(i);
                }
                _itemWatchers.Clear();
                Dirty();
            }
        }

        public CollidableStructure[] StructureItems
        {
            get
            {
                lock (_lock)
                {
                    if (_dirtyStructuresItems || _itemsCache == null)
                    {
                        _itemsCache = _itemWatchers.Select(I => I.Structure).ToArray();
                        _dirtyStructuresItems = false;
                    }
                    return _itemsCache;
                }
            }
        }

        public StructureWatcher[] Watchers
        {
            get
            {
                lock(_lock) 
                    return _itemWatchers.ToArray();
            }
        }
        
        public BaseStructure[] Structures
        {
            get
            {
                if (_dirtyStructures || _structureCache == null)
                {
                    _structureCache = _itemWatchers.SelectMany(
                        I => I.Structure.WorldObject.Children.Concat(new [] { I.Structure.WorldObject })
                        ).ToArray();
                    _dirtyStructures = false;
                }
                return _structureCache;
            }
        }

        private void Dirty()
        {
            _dirtyStructuresItems = true;
            _dirtyStructures = true;
        }

        public static Type[] GetTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(StructureHandler).Namespace).ToArray();
            return types.Where(T => T.IsSubclassOf(typeof(StructureDesign)) && !T.IsAbstract).ToArray();
        }
    }
}
