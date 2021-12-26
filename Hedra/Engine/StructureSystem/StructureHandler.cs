/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Framework;
using Hedra.Numerics;
using Hedra.Structures;

namespace Hedra.Engine.StructureSystem
{
    /// <summary>
    ///     Description of StructureGenerator.
    /// </summary>
    public class StructureHandler
    {
        private readonly List<StructureWatcher> _itemWatchers;
        private readonly object _lock = new object();
        private readonly Dictionary<Vector3, CollidableStructure> _registeredPositions;
        private readonly object _registerLock = new object();
        private bool _dirtyStructures;
        private bool _dirtyStructuresItems;
        private CollidableStructure[] _itemsCache;
        private BaseStructure[] _structureCache;

        public StructureHandler()
        {
            _registeredPositions = new Dictionary<Vector3, CollidableStructure>();
            _itemWatchers = new List<StructureWatcher>();
            SeedGenerator = new Voronoi();
            World.OnChunkDisposed += OnChunkDisposed;
        }

        public Voronoi SeedGenerator { get; }

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
                lock (_lock)
                {
                    return _itemWatchers.ToArray();
                }
            }
        }

        public BaseStructure[] Structures
        {
            get
            {
                if (_dirtyStructures || _structureCache == null)
                {
                    _structureCache = Watchers.SelectMany(
                        I => I.Structure.WorldObject.Children.Concat(new[] { I.Structure.WorldObject })
                    ).ToArray();
                    _dirtyStructures = false;
                }

                return _structureCache;
            }
        }

        private void OnChunkDisposed(Chunk Chunk)
        {
            lock (_lock)
            {
                for (var i = _itemWatchers.Count - 1; i > -1; --i)
                {
                    var item = _itemWatchers[i];
                    if (ShouldRemove(item.Structure))
                    {
                        UnregisterStructure(item.Structure.Position.Xz().ToVector3());
                        item.Dispose();
                        _itemWatchers.RemoveAt(i);
                    }
                }

                Dirty();
            }
        }

        /* Used from MissionCore.py */
        public static List<Pair<StructureDesign, Vector3>> NearbyStructuresPositionDesigns(Vector3 Position, Type Type,
            float MaxDistance)
        {
            var radius = (int)(MaxDistance / Chunk.Width);
            var structs = new List<Pair<StructureDesign, Vector3>>();
            var chunkSpace = World.ToChunkSpace(Position);
            for (var x = -radius; x < radius; x++)
            for (var z = -radius; z < radius; z++)
            {
                var finalPosition = new Vector3(chunkSpace.X + x * Chunk.Width, 0, chunkSpace.Y + z * Chunk.Width);
                if ((finalPosition - Position).Xz().LengthSquared() > MaxDistance * MaxDistance)
                    continue;

                var region = World.BiomePool.GetRegion(finalPosition);
                var sample = MapBuilder.Sample(finalPosition, region);
                if (sample != null && sample.GetType() == Type)
                    structs.Add(new Pair<StructureDesign, Vector3>(sample, finalPosition));
            }

            return structs.OrderBy(S => (Position - S.Two).LengthFast()).ToList();
        }

        private static bool ShouldRemove(CollidableStructure Structure)
        {
            /* Offset is not used because this causes issues when chunks are deleted at chunk edges */
            return Structure.Design.ShouldRemove(Structure) && Structure.Built && Structure.ActiveQuests == 0;
        }

        public void CheckStructures(Vector2 ChunkOffset)
        {
            var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
            var region = underChunk != null
                ? underChunk.Biome
                : World.BiomePool.GetRegion(ChunkOffset.ToVector3());
            CheckStructures(ChunkOffset, region.Structures.Designs);
        }

        /* Used from MissionCore.py */
        public void CheckStructures(Vector2 ChunkOffset, StructureDesign[] Designs)
        {
            if (!World.IsChunkOffset(ChunkOffset))
                throw new ArgumentException("Provided parameter does not represent a valid offset");

            var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
            var region = underChunk != null
                ? underChunk.Biome
                : World.BiomePool.GetRegion(ChunkOffset.ToVector3());
            var maxSearchRadius = Designs.Max(D => D.SearchRadius);
            /* Beware! This is created locally and we don't maintain a static instance because of multi-threading issues. */
            var distribution = new RandomDistribution(true);
            for (var x = Math.Min(-2, -maxSearchRadius / Chunk.Width * 2);
                 x < Math.Max(2, maxSearchRadius / Chunk.Width * 2);
                 x++)
            {
                for (var z = Math.Min(-2, -maxSearchRadius / Chunk.Width * 2);
                     z < Math.Max(2, maxSearchRadius / Chunk.Width * 2);
                     z++)
                {
                    //we should check if chunk is null/doesnt exist
                    var offset = new Vector2(ChunkOffset.X + x * Chunk.Width, ChunkOffset.Y + z * Chunk.Width);
                    var design = MapBuilder.Sample(World.ToChunkSpace(offset).ToVector3(), region);
                    if (design == null) continue;

                    if (!IsWithinSearchRadius(design, offset, ChunkOffset)) continue;
                    if (!design.MeetsRequirements(offset)) continue;

                    design.PlaceDesign(offset, distribution, region, World.StructureHandler.StructureItems);
                }
            }
        }

        private static bool IsWithinSearchRadius(StructureDesign Design, Vector2 Offset, Vector2 Center)
        {
            var chunkDistance = (Offset - Center).LengthFast() / Chunk.Width;
            var structureSearchDistance = Math.Max(2, Design.SearchRadius / Chunk.Width) * 2;
            return chunkDistance <= structureSearchDistance * 2;
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

        public void RegisterStructure(Vector3 Position, CollidableStructure Structure)
        {
            lock (_registerLock)
            {
                if (_registeredPositions.ContainsKey(Position))
                {
#if DEBUG
                    if (_registeredPositions[Position] != Structure)
                    {
                        var a = 0;
                    }
#endif
                    return;
                }

                _registeredPositions.Add(Position, Structure);
            }
        }

        public void UnregisterStructure(Vector3 Position)
        {
            lock (_registerLock)
            {
                /* This commented code fails when a structure setup method generates an exception, causing the structure to exist and dispose but not registeres */
#if DEBUG
                if (!_registeredPositions.ContainsKey(Position))
                    throw new ArgumentOutOfRangeException();
#endif
                _registeredPositions.Remove(Position);
            }
        }

        public bool StructureExistsAtPosition(Vector3 Position)
        {
            lock (_registerLock)
            {
                return _registeredPositions.ContainsKey(Position);
            }
        }

        public void AddStructure(CollidableStructure Structure)
        {
            lock (_lock)
            {
                _itemWatchers.Add(new StructureWatcher(Structure));
                RegisterStructure(Structure.Position.Xz().ToVector3(), Structure);
                Structure.Setup();
                Dirty();
            }
        }

        public CollidableStructure[] Find(Predicate<CollidableStructure> Match, Vector3 SortPosition)
        {
            var list = new List<CollidableStructure>();
            var items = StructureItems;
            for (var i = 0; i < items.Length; ++i)
                if (Match(items[i]))
                    list.Add(items[i]);
            return list.OrderBy(S => (S.Position - SortPosition).LengthFast()).ToArray();
        }

        public bool Has(CollidableStructure Structure)
        {
            lock (_lock)
            {
                return StructureItems.Any(S => S == Structure);
            }
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