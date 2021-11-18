/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 04:16 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Scenes;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.Windowing;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Framework;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Sound;
using Hedra.Structures;

namespace Hedra.Engine.Generation
{
    public class WorldProvider : IWorldProvider
    {
        private static readonly int[] _menuSeeds =
        {
            843966896,
            365367015,
            -1207602704,
            -1316611004,
            1711677118,
            434316731,
            -1521453338,
            930356700,
            2108869840,
            247179995,
            -488844428,
            -1835204548,
            -1527889388,
            1436884727,
            -1122906719,
            1050378001,
            -65738522,
            1590961909
        };

        private readonly RenderingComparer _renderingComparer;
        private readonly Dictionary<Vector2, Chunk> _unculledChunks;
        private readonly object _worldObjectsLock = new object();
        private int _previousId;
        private WorldType _type;

        public WorldProvider()
        {
            Builder = new WorldBuilder();
            _entities = new HashSet<IEntity>();
            _worldObjects = new HashSet<IWorldObject>();
            _chunks = new HashSet<Chunk>();
            _renderingComparer = new RenderingComparer();
            SearcheableChunks = new Dictionary<Vector2, Chunk>(new FastComparer());
            DrawingChunks = new Dictionary<Vector2, Chunk>();
            ShadowDrawingChunks = new Dictionary<Vector2, Chunk>();
            _unculledChunks = new Dictionary<Vector2, Chunk>();
        }

        public event ModulesReloadEvent ModulesReload;

        public Dictionary<Vector2, Chunk> SearcheableChunks { get; }
        public AreaHighlighter Highlighter { get; private set; }
        public ParticleSystem Particles { get; private set; }
        public EnvironmentGenerator EnvironmentGenerator { get; private set; }
        public FishingZoneHandler FishingZoneHandler { get; private set; }
        public IBiomePool BiomePool { get; private set; }
        public MobFactory MobFactory { get; private set; }
        public TreeGenerator TreeGenerator { get; private set; }
        public IWorldBuilding WorldBuilding { get; private set; }
        public StructureHandler StructureHandler { get; private set; }
        public int Seed { get; private set; }
        public bool IsGenerated { get; private set; }
        public WorldBuilder Builder { get; }

        public Vector3 SpawnPoint { get; private set; }

        public Vector3 SpawnVillagePoint { get; private set; }

        public Dictionary<Vector2, Chunk> DrawingChunks { get; }
        public Dictionary<Vector2, Chunk> ShadowDrawingChunks { get; }

        public void Load()
        {
            Seed = MenuSeed;
            BiomePool = new BiomePool(_type = WorldType.Overworld);
            TreeGenerator = new TreeGenerator();
            WorldBuilding = new WorldBuilding.WorldBuilding();
            StructureHandler = new StructureHandler();
            EnvironmentGenerator = new EnvironmentGenerator();
            MobFactory = new MobFactory();
            Highlighter = new AreaHighlighter();
            FishingZoneHandler = new FishingZoneHandler();
            Particles = new ParticleSystem
            {
                HasMultipleOutputs = true
            };
            ReloadModules();
            IsGenerated = true;
            WorldRenderer.StaticBuffer.Comparer = _renderingComparer;
            WorldRenderer.WaterBuffer.Comparer = _renderingComparer;
            WorldRenderer.InstanceBuffer.Comparer = _renderingComparer;
            MenuBackground.Setup();
        }

        public void ReloadModules()
        {
            MobFactory?.Empty();
            AnimationLoader.EmptyCache();
            AnimationModelLoader.EmptyCache();

            var factories = MobLoader.LoadModules(AssetManager.AppPath);
            MobFactory?.AddFactory(factories);
            AbilityTreeLoader.LoadModules(AssetManager.AppPath);
            ClassLoader.LoadModules(AssetManager.AppPath);
            ItemLoader.LoadModules(AssetManager.AppPath);
            VillageLoader.LoadModules(AssetManager.AppPath);
            HumanoidLoader.LoadModules(AssetManager.AppPath);
            ModulesReload?.Invoke(AssetManager.AppPath);
            ModificationsLoader.Reload();
        }

        public int MenuSeed { get; } = _menuSeeds[new Random().Next(0, _menuSeeds.Length)];

        public int RandomSeed
        {
            get
            {
                var newSeed = MenuSeed;
                while (newSeed == MenuSeed) newSeed = Utils.Rng.Next(1, int.MaxValue / 2);
                return newSeed;
            }
        }

        public void OccludeTest()
        {
            if (!GameSettings.OcclusionCulling) return;
            Occludable.Bind();

            var chunks = Chunks;
            for (var i = 0; i < chunks.Count; ++i)
                if (_unculledChunks.ContainsKey(chunks[i].Position.Xz()))
                    chunks[i].Mesh?.DrawQuery();

            Occludable.Unbind();
        }

        public void CullTest()
        {
            DrawingChunks.Clear();
            ShadowDrawingChunks.Clear();
            _unculledChunks.Clear();
            var toDrawArray = Chunks;
            DoCullTest(toDrawArray, DrawingChunks, _unculledChunks);

            if (GameSettings.Shadows && !GameSettings.LockFrustum)
            {
                WorldRenderer.PrepareShadowMatrix();
                var prevValue = GameSettings.OcclusionCulling;
                GameSettings.OcclusionCulling = false;
                DoCullTest(toDrawArray, ShadowDrawingChunks, null);
                GameSettings.OcclusionCulling = prevValue;
                WorldRenderer.PrepareCameraMatrix();
            }

            _renderingComparer.Position = GameManager.Player.Position;
        }

        public void Draw(WorldRenderType Type)
        {
            if ((Type & WorldRenderType.StaticAndInstance) == WorldRenderType.StaticAndInstance)
            {
                /*
                var chunks = Chunks;
                for (var i = 0; i < chunks.Count; ++i)
                {
                    BasicGeometry.DrawBox(chunks[i].Mesh.Min + chunks[i].Mesh.Position,
                        chunks[i].Mesh.Max + chunks[i].Mesh.Position, Vector4.One);
                }*/
            }

            var drawingChunks = DrawingChunks;
            var shadowDrawingChunks = ShadowDrawingChunks;
            var type = Type;

            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            WorldRenderer.PrepareCameraMatrix();
            WorldRenderer.Render(drawingChunks, shadowDrawingChunks, type);

            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public float GetNoise(float X, float Y)
        {
            return (float)OpenSimplexNoise.Evaluate(X, Y); //_noise.GetSimplex(X, Y);
        }

        public float GetNoise(float X, float Y, float Z)
        {
            return (float)OpenSimplexNoise.Evaluate(X, Y, Z); //_noise.GetSimplex(X, Y, Z);
        }

        public void Update()
        {
            Builder.Update();
        }

        public void Recreate(int NewSeed, WorldType Type)
        {
            if (Seed == NewSeed)
                return;

            _previousId = 0;
            Seed = NewSeed;
            _type = Type;
            BiomePool = new BiomePool(_type);
            WorldBuilding = new WorldBuilding.WorldBuilding();
            OpenSimplexNoise.Load(NewSeed);
            Builder.Discard();
            SpawnPoint = FindSpawningPoint(GeneralSettings.SpawnPoint);
            var rng = new Random(Seed);
            SpawnVillagePoint = FindSpawningPoint(
                SpawnPoint
                + new Vector3(rng.NextBool() ? -1 : 1, 0, rng.NextBool() ? -1 : 1)
                * new Vector3(1.5f + rng.NextFloat() * 2, 0, 1.5f + rng.NextFloat() * 2)
                * VillageDesign.MaxVillageRadius
            );
            SkyManager.SetTime(12000);

            var worldObjects = WorldObjects;
            for (var i = worldObjects.Length - 1; i > -1; i--)
            {
                worldObjects[i].Dispose();
                RemoveObject(worldObjects[i]);
            }

            Highlighter.Reset();

            lock (TreeGenerator)
            {
                TreeGenerator = new TreeGenerator();
            }

            lock (SearcheableChunks)
            {
                SearcheableChunks.Clear();
            }

            var chunks = Chunks;
            for (var i = chunks.Count - 1; i > -1; i--)
                try
                {
                    RemoveChunk(chunks[i]);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.WriteLine(e);
                }

            var entities = Entities;
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (entities[i] == LocalPlayer.Instance ||
                    entities[i] == LocalPlayer.Instance.Companion.Entity) continue;
                entities[i].Dispose();
            }

            StructureHandler.Discard();
            WorldRenderer.ForceDiscard();
            FishingZoneHandler.Discard();
            CacheManager.Discard();
            MapBuilder.Discard();

            AddEntity(GameManager.Player);

            if (NewSeed == MenuSeed)
                MenuBackground.Setup();

            GC.Collect();
        }

        public void Discard()
        {
            Builder.Discard();
        }

        public T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            var results = new List<T>();
            var searchOptions = new List<T>();
            searchOptions.AddRange(Entities.OfType<T>());
            searchOptions.AddRange(StructureHandler.Structures.OfType<T>());

            for (var i = 0; i < searchOptions.Count; i++)
                if ((searchOptions[i].Position - Position).LengthSquared() < Radius * Radius)
                    results.Add(searchOptions[i]);
            return results.ToArray();
        }

        public void AddChunkToQueue(Chunk Chunk, ChunkQueueType Type)
        {
            Builder.Process(Chunk, Type);
        }

        public void AddEntity(IEntity Entity)
        {
            lock (_entities)
            {
                if (_entities.Contains(Entity)) return;
                _entities.Add(Entity);
            }

            _isEntityCacheDirty = true;
        }

        public void RemoveEntity(IEntity Entity)
        {
            lock (_entities)
            {
                if (!_entities.Contains(Entity)) return;
                _entities.Remove(Entity);
            }

            _isEntityCacheDirty = true;
        }

        public void RemoveObject(IWorldObject WorldObject)
        {
            lock (_worldObjectsLock)
            {
                if (!_worldObjects.Contains(WorldObject)) return;
                _worldObjects.Remove(WorldObject);
            }

            _isWorldObjectCacheDirty = true;
        }

        public void AddChunk(Chunk Chunk)
        {
            lock (SearcheableChunks)
            {
                if (!SearcheableChunks.ContainsKey(new Vector2(Chunk.OffsetX, Chunk.OffsetZ)))
                {
                    lock (_chunks)
                    {
                        _chunks.Add(Chunk);
                    }

                    SearcheableChunks.Add(new Vector2(Chunk.OffsetX, Chunk.OffsetZ), Chunk);
                }
            }

            _isChunksCacheDirty = true;
        }

        public void RemoveChunk(Chunk Chunk)
        {
            if (Chunk == null) return;

            _isChunksCacheDirty = true;
            WorldRenderer.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));

            var entities = Entities;
            for (var i = entities.Count - 1; i > -1; i--)
            {
                if (entities[i] == null)
                {
                    RemoveEntity(entities[i]);
                    continue;
                }

                var entityPosition = World.ToChunkSpace(entities[i].Position);
                if (entityPosition == Chunk.Position.Xz() || World.GetChunkByOffset(entityPosition) == null)
                    if (entities[i].Removable && !(entities[i] is IPlayer))
                        entities[i].Dispose();
            }

            var items = WorldObjects;
            for (var i = items.Length - 1; i > -1; i--)
            {
                if (items[i] == null)
                {
                    RemoveObject(items[i]);
                    continue;
                }

                var itemPosition = items[i].Position;
                if (itemPosition.X < Chunk.OffsetX + Chunk.Width && itemPosition.X > Chunk.OffsetX &&
                    itemPosition.Z < Chunk.OffsetZ + Chunk.Width && itemPosition.Z > Chunk.OffsetZ)
                    items[i].Dispose();
            }

            Builder.Remove(Chunk);
            Chunk.Dispose();
            lock (_chunks)
            {
                _chunks.Remove(Chunk);
            }

            lock (SearcheableChunks)
            {
                SearcheableChunks.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));
            }
        }

        public Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            var offset = new Vector2(OffsetX, OffsetZ);
            Chunk chunk;
            lock (SearcheableChunks)
                SearcheableChunks.TryGetValue(offset, out chunk);
            return chunk;
        }

        public void SetupStructure(CollidableStructure Structure)
        {
            WorldBuilding.SetupStructure(Structure);
        }

        public bool IsChunkOffset(Vector2 Offset)
        {
            return Offset.X % Chunk.Width == 0 && Offset.Y % Chunk.Width == 0;
        }

        public Vector3 ToBlockSpace(Vector3 Vec3)
        {
            var chunkSpace = ToChunkSpace(Vec3);
            var x = (int)Math.Abs(Math.Floor((Vec3.X - chunkSpace.X) / Chunk.BlockSize));
            var z = (int)Math.Abs(Math.Floor((Vec3.Z - chunkSpace.Y) / Chunk.BlockSize));
            return new Vector3(x, Math.Min(Vec3.Y / Chunk.BlockSize, Chunk.Height - 1), z);
        }

        public Vector2 ToChunkSpace(Vector3 Vec3)
        {
            return new Vector2(((int)Vec3.X >> 7) << 7, ((int)Vec3.Z >> 7) << 7);
        }

        public Chunk GetChunkAt(Vector3 Coordinates)
        {
            var chunkSpace = ToChunkSpace(Coordinates);
            return GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            var chunkSpace = ToChunkSpace(X, Z);
            var blockSpace = ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
            return blockChunk?.GetHighestBlockAt((int)blockSpace.X, (int)blockSpace.Z) ?? new Block();
        }

        public int GetHighestY(int X, int Z)
        {
            var chunkSpace = ToChunkSpace(X, Z);
            var blockSpace = ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
            return blockChunk?.GetHighestY((int)blockSpace.X, (int)blockSpace.Z) ?? 0;
        }

        public float GetHighest(int X, int Z)
        {
            var chunkSpace = ToChunkSpace(X, Z);
            var blockSpace = ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
            return blockChunk?.GetHighest((int)blockSpace.X, (int)blockSpace.Z) ?? 0;
        }

        public HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            return Highlighter.HighlightArea(Position, Color, Radius, Seconds);
        }

        public void AddWorldObject(IWorldObject WorldObject)
        {
            lock (_worldObjectsLock)
            {
                if (_worldObjects.Contains(WorldObject))
                    throw new ArgumentException("WorldObject already exists in this world.");
                _worldObjects.Add(WorldObject);
            }

            _isWorldObjectCacheDirty = true;
        }

        public WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            var model = new WorldItem(ItemSpec, Position);
            model.OnPickup += delegate(IPlayer Player)
            {
                TaskScheduler.While(() => !model.Disposed, delegate
                {
                    model.Outline = false;
                    model.Position = Mathf.Lerp(model.Position, Player.Position, Time.DeltaTime * 5f);
                    if ((model.Position - Player.Position).LengthSquared() < 4 * 4)
                    {
                        if (Player.Inventory.AddItem(model.ItemSpecification))
                        {
                            model.Enabled = false;
                            SoundPlayer.PlaySound(SoundType.NotificationSound, model.Position, false, 1f, 1.2f);
                        }
                        else
                        {
                            World.DropItem(model.ItemSpecification, model.Position);
                        }

                        model.Dispose();
                    }
                });
            };
            return model;
        }

        public ISkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            var mob = MobFactory.Build(Type, MobSeed);
            var placeablePosition = FindPlaceablePosition(mob,
                new Vector3(DesiredPosition.X, Physics.HeightAtPosition(DesiredPosition.X, DesiredPosition.Z),
                    DesiredPosition.Z));
            mob.MobId = ++_previousId;
            mob.Seed = MobSeed;
            mob.Model.TargetRotation = new Vector3(0, new Random(MobSeed).NextFloat() * 360f, 0);
            mob.Position = placeablePosition;
            mob.Model.Position = placeablePosition;
            MobFactory.Polish(mob);

            AddEntity(mob);
            return mob;
        }

        public Vector3 FindSpawningPoint(Vector3 Around)
        {
            if (_type != WorldType.Overworld) return Around;
            var point = Around;
            var rng = new Random(World.Seed + 43234);

            bool IsWater(Vector3 Point)
            {
                var region = BiomePool.GetRegion(Point);
                return region.Generation.GetMaxHeight(Point.X, Point.Z) < BiomeSystem.BiomePool.SeaLevel
                       || region.Generation.RiverAtPoint(Point.X, Point.Z) > 0;
            }

            var structures = StructureHandler.StructureItems;

            bool IsInsideStructure(Vector3 Point)
            {
                for (var i = 0; i < structures.Length; ++i)
                {
                    var radius = structures[i].Mountain?.Radius ?? structures[i].Radius;
                    if ((structures[i].Position - Point).LengthSquared() < radius * radius)
                        return true;
                }

                return false;
            }

            while (IsWater(point) || IsInsideStructure(point))
                point +=
                    new Vector3((192f * rng.NextFloat() - 96f) * Chunk.BlockSize, 0,
                        (192f * rng.NextFloat() - 96f) * Chunk.BlockSize);

            return point;
        }

        public Vector3 FindPlaceablePosition(IEntity Mob, Vector3 DesiredPosition)
        {
            var offset = DesiredPosition;
            var firstCollision = Mob.Physics.CollidesWithOffset(-Mob.Position + offset);
            if (firstCollision)
            {
                var possibleWaypoints = StructureHandler.GetNearStructures(DesiredPosition).Select(S => S.Waypoints)
                    .Where(S => S != null).ToArray();
                for (var i = 0; i < possibleWaypoints.Length; ++i)
                {
                    var nearest = possibleWaypoints[i].GetNearestVertex(offset, out var distance);
                    if (distance < Mathf.FastSqrt(32 * 32 + 32 * 32))
                        return nearest.Position;
                }
            }

            while (Mob.Physics.CollidesWithOffset(-Mob.Position + offset))
                offset += new Vector3(Utils.Rng.NextFloat() * 32f - 16f, 0, Utils.Rng.NextFloat() * 32f - 16f);
            return offset;
        }

        public Block GetBlockAt(Vector3 Vec3)
        {
            var chunkSpace = ToChunkSpace(Vec3);
            var blockSpace = ToBlockSpace(Vec3);

            var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
            return blockChunk?.GetBlockAt((int)blockSpace.X, (int)(Vec3.Y / Chunk.BlockSize), (int)blockSpace.Z) ??
                   new Block();
        }

        public float NearestWaterBlock(Vector3 Position, float SearchRange, out Vector3 WaterPosition)
        {
            var nearest = Math.Pow(SearchRange + 1, 2);
            WaterPosition = Vector3.Zero;
            for (var x = -1; x < 2; x++)
            for (var z = -1; z < 2; z++)
            {
                var chunk = World.GetChunkAt(Position + new Vector3(x, 0, z) * Chunk.Width);
                if (chunk == null || !chunk.BuildedWithStructures || !chunk.HasWater) continue;
                var dist = NearestWaterBlockOnChunk(chunk, Position, out WaterPosition);
                if (dist < nearest) nearest = dist;
            }

            return (float)Math.Sqrt(nearest);
        }

        public unsafe float NearestWaterBlockOnChunk(Chunk Chunk, Vector3 Position, out Vector3 WaterPosition)
        {
            var nearest = float.MaxValue;
            WaterPosition = Vector3.Zero;
            var size = Allocator.Kilobyte * 64;
            var mem = stackalloc byte[size];
            using (var allocator = new StackAllocator(size, mem))
            {
                var positions = Chunk.GetWaterPositions(allocator);
                for (var i = 0; i < positions.Length; i++)
                {
                    WaterPosition = positions[i].ToVector3() * Chunk.BlockSize + Chunk.Position;
                    var dist = (WaterPosition - Position).Xz().LengthSquared();
                    if (dist < nearest) nearest = dist;
                }
            }

            return nearest;
        }

        public float NearestWaterBlockOnChunk(Vector3 Position, out Vector3 WaterPosition)
        {
            var nearest = float.MaxValue;
            var chunk = World.GetChunkAt(Position);
            WaterPosition = Vector3.Zero;
            if (chunk == null || !chunk.HasWater) return nearest;
            return NearestWaterBlockOnChunk(chunk, Position, out WaterPosition);
        }

        private static void AssertNoDuplicates(IList<Chunk> Chunks)
        {
            if (Chunks.Any(C => Chunks.Count(K => C.Position == K.Position) > 1))
                throw new ArgumentException(
                    $"Found duplicate chunk at index {Chunks.IndexOf(Chunks.First(C => Chunks.Count(K => C.Position == K.Position) > 1))}");
        }

        private static void DoCullTest(ReadOnlyCollection<Chunk> ToDrawArray, Dictionary<Vector2, Chunk> Output,
            Dictionary<Vector2, Chunk> OutputIfFrustumCulled, bool IsRefractionPass = false)
        {
            for (var i = 0; i < ToDrawArray.Count; i++)
            {
                var chunk = ToDrawArray[i];
                if (IsRefractionPass && !chunk.HasWater) continue;
                if (chunk == null || chunk.Disposed)
                {
                    World.RemoveChunk(chunk);
                    continue;
                }

                var offset = chunk.Position.Xz();

                if (WorldRenderer.EnableCulling)
                {
                    if (Culling.IsInside(chunk.Mesh))
                    {
                        if (!chunk.Mesh.Occluded)
                            Output.Add(offset, chunk);
                        OutputIfFrustumCulled?.Add(offset, chunk);
                    }
                }
                else
                {
                    if (Output.ContainsKey(offset))
                    {
                        var b = Output[offset];
                        var a = 0;
                        continue;
                    }

                    Output.Add(offset, chunk);
                }
            }
        }

        public void SaveChunk()
        {
            SaveRegion();
        }

        private void SaveRegion()
        {
        }

        public void Save(string Folder)
        {
            const int region = 12;
            var regionSize = Math.Pow(2, 12);
            var regions = new Dictionary<Vector2, List<Chunk>>();
            var chunks = Chunks;
            foreach (var chunk in chunks)
            {
                var src = new Vector2((chunk.OffsetX >> region) << region, (chunk.OffsetZ >> region) << region);
                if (!regions.ContainsKey(src)) regions[src] = new List<Chunk>();
                regions[src].Add(chunk);
            }
        }

        public Block GetBlockAt(int X, int Y, int Z)
        {
            return GetBlockAt(new Vector3(X, Y, Z));
        }

        private Vector3 ToBlockSpace(float X, float Z)
        {
            return ToBlockSpace(new Vector3(X, 0, Z));
        }

        private Vector2 ToChunkSpace(float X, float Z)
        {
            return ToChunkSpace(new Vector3(X, 0, Z));
        }

        #region Propierties

        private bool _isChunksCacheDirty = true;
        private readonly HashSet<Chunk> _chunks;
        private ReadOnlyCollection<Chunk> _chunksCache;

        public ReadOnlyCollection<Chunk> Chunks
        {
            get
            {
                if (_isChunksCacheDirty)
                {
                    lock (_chunks)
                    {
                        _chunksCache = _chunks.ToArray().ToList().AsReadOnly();
                    }

                    _isChunksCacheDirty = false;
                }

                lock (_chunksCache)
                {
                    return _chunksCache;
                }
            }
        }


        private bool _isWorldObjectCacheDirty = true;
        private readonly HashSet<IWorldObject> _worldObjects;
        private IWorldObject[] _itemsCache;

        public IWorldObject[] WorldObjects
        {
            get
            {
                if (_isWorldObjectCacheDirty)
                {
                    lock (_worldObjects)
                    {
                        _itemsCache = _worldObjects.ToArray();
                    }

                    _isWorldObjectCacheDirty = false;
                }

                lock (_itemsCache)
                {
                    return _itemsCache;
                }
            }
        }

        private bool _isEntityCacheDirty = true;
        private readonly HashSet<IEntity> _entities;
        private ReadOnlyCollection<IEntity> _entityCache;

        public ReadOnlyCollection<IEntity> Entities
        {
            get
            {
                if (_isEntityCacheDirty)
                {
                    lock (_entities)
                    {
                        _entityCache = _entities.ToArray().ToList().AsReadOnly();
                    }

                    _isEntityCacheDirty = false;
                }

                lock (_entityCache)
                {
                    return _entityCache;
                }
            }
        }

        #endregion
    }
}