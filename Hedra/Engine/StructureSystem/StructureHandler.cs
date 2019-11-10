  /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Game;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Numerics;

  namespace Hedra.Engine.StructureSystem
{
    /// <summary>
    /// Description of StructureGenerator.
    /// </summary>
    public class StructureHandler
    {
        private readonly object _lock = new object();
        private readonly object _registerLock = new object();
        public Voronoi SeedGenerator { get; }
        private readonly List<StructureWatcher> _itemWatchers;
        private CollidableStructure[] _itemsCache;
        private BaseStructure[] _structureCache;
        private HashSet<Vector3> _registeredPositions;
        private bool _dirtyStructuresItems;
        private bool _dirtyStructures;
        
        public StructureHandler()
        {
            _registeredPositions = new HashSet<Vector3>();
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
            /* Beware! This is created locally and we don't maintain a static instance because of multi-threading issues. */
            var distribution = new RandomDistribution(true);
            for (var i = 0; i < designs.Length; i++)
            {
                if (designs[i].MeetsRequirements(ChunkOffset))
                    designs[i].CheckFor(ChunkOffset, region, distribution);
            }
        }

        public void Build(CollidableStructure Struct)
        {
            void DoBuild()
            {
                Struct.Design.Build(Struct);
                Struct.Built = true;
            }

            //if (Loader.Hedra.MainThreadId == Thread.CurrentThread.ManagedThreadId)
            //    TaskScheduler.Parallel(DoBuild);
            //else
            DoBuild();
        }

        public void RegisterStructure(Vector3 Position)
        {
            lock(_registerLock)
                _registeredPositions.Add(Position);
        }

        public bool StructureExistsAtPosition(Vector3 Position)
        {
            lock(_registerLock)
                return _registeredPositions.Contains(Position);
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

        public CollidableStructure[] Find(Predicate<CollidableStructure> Match)
        {
            var list = new List<CollidableStructure>();
            var items = StructureItems;
            for (var i = 0; i < items.Length; ++i)
            {
                if (Match(items[i]))
                    list.Add(items[i]);
            }
            return list.ToArray();
        }

        public bool Has(CollidableStructure Structure)
        {
            lock (_lock)
                return StructureItems.Any(S => S == Structure);
        }

        public static CollidableStructure[] GetNearStructures(Vector3 Position)
        {
            return (from item in World.StructureHandler.StructureItems
                where (item.Position.Xz() - Position.Xz()).LengthSquared() < item.Radius * item.Radius
                select item).ToArray();
        }

        public void Discard()
        {
            lock (_lock)
            {
                _registeredPositions.Clear();
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
                if (_dirtyStructuresItems || _itemsCache == null)
                {
                    _itemsCache = Watchers.Select(I => I.Structure).ToArray();
                    _dirtyStructuresItems = false;
                }
                return _itemsCache;     
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
                    _structureCache = Watchers.SelectMany(
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
