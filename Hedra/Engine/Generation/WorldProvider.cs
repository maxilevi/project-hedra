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
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Sound;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Generation
{
    public class WorldProvider : IWorldProvider
    {
        private readonly ChunkBuilder _chunkBuilder;
        private readonly MeshBuilder _meshBuilder;
        private readonly RenderingComparer _renderingComparer;
        private readonly SharedWorkerPool _meshWorkerPool;
        private readonly SharedWorkerPool _genWorkerPool;
        private Vector3 _spawningVillagePoint;
        private Vector3 _spawningPoint;
        private WorldType _type;
        private int _previousId;

        public WorldProvider()
        {
            _meshWorkerPool = new SharedWorkerPool(2);
            _genWorkerPool = new SharedWorkerPool(1);
            _meshBuilder = new MeshBuilder(_meshWorkerPool);
            _chunkBuilder = new ChunkBuilder(_genWorkerPool);
            _entities = new HashSet<IEntity>();
            _worldObjects = new HashSet<IWorldObject>();
            _chunks = new HashSet<Chunk>();
            _globalColliders = new HashSet<ICollidable>();
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
        public IBiomePool BiomePool { get; private set; }
        public MobFactory MobFactory { get; private set; }
        public TreeGenerator TreeGenerator { get; private set; }
        public IWorldBuilding WorldBuilding { get; private set; }
        public StructureHandler StructureHandler { get; private set; }
        public int Seed { get; private set; }
        public bool IsGenerated { get; private set; }
        public int MeshQueueCount => _meshBuilder.Count;
        public int ChunkQueueCount => _chunkBuilder.Count;
        public int AverageBuildTime => _meshBuilder.AverageWorkTime;     
        public int AverageGenerationTime => _chunkBuilder.AverageWorkTime;
        public Vector3 SpawnPoint => _spawningPoint;
        public Vector3 SpawnVillagePoint => _spawningVillagePoint;
        public Dictionary<Vector2, Chunk> DrawingChunks { get; }
        public Dictionary<Vector2, Chunk> ShadowDrawingChunks { get; }
        private readonly Dictionary<Vector2, Chunk> _unculledChunks;
        private readonly object _worldObjectsLock = new object();

        public void Load()
        {
            MenuBackground.Setup();

            Seed = MenuSeed;
            BiomePool = new BiomePool(_type = WorldType.Overworld);
            TreeGenerator = new TreeGenerator();
            WorldBuilding = new WorldBuilding.WorldBuilding();
            StructureHandler = new StructureHandler();
            EnvironmentGenerator = new EnvironmentGenerator();
            MobFactory = new MobFactory();
            Highlighter = new AreaHighlighter();
            Particles = new ParticleSystem
            {
                HasMultipleOutputs = true
            };
            ReloadModules();
            IsGenerated = true;
            WorldRenderer.StaticBuffer.Comparer = _renderingComparer;
            WorldRenderer.WaterBuffer.Comparer = _renderingComparer;
            WorldRenderer.InstanceBuffer.Comparer = _renderingComparer;
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

        public int MenuSeed => 2124321422;

        public int RandomSeed
        {
            get
            {
                var newSeed = MenuSeed;
                while (newSeed == MenuSeed)
                {
                    newSeed = Utils.Rng.Next(1, int.MaxValue / 2);
                }
                return newSeed;
            }
        }

        public void OccludeTest()
        {
            if(!GameSettings.OcclusionCulling) return;
            Occludable.Bind();
            
            var chunks = Chunks;
            for (var i = 0; i < chunks.Count; ++i)
            {
                if(_unculledChunks.ContainsKey(chunks[i].Position.Xz))
                    chunks[i].Mesh?.DrawQuery();
            }

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

        private static void AssertNoDuplicates(IList<Chunk> Chunks)
        {
            if(Chunks.Any(C => Chunks.Count(K => C.Position == K.Position) > 1))
                throw new ArgumentException($"Found duplicate chunk at index {Chunks.IndexOf(Chunks.First(C => Chunks.Count(K => C.Position == K.Position) > 1))}");
        }

        private static void DoCullTest(ReadOnlyCollection<Chunk> ToDrawArray, Dictionary<Vector2, Chunk> Output, Dictionary<Vector2, Chunk> OutputIfFrustumCulled)
        {
            for (var i = 0; i < ToDrawArray.Count; i++)
            {
                var chunk = ToDrawArray[i];
                if (chunk == null || chunk.Disposed)
                {
                    World.RemoveChunk(chunk);
                    continue;
                }
                var offset = chunk.Position.Xz;
                
                if (WorldRenderer.EnableCulling)
                {
                    if(!chunk.Initialized) continue;
                    
                    if (Culling.IsInside(chunk.Mesh))
                    {
                        if(!chunk.Mesh.Occluded)
                            Output.Add(offset, chunk);
                        OutputIfFrustumCulled?.Add(offset, chunk);
                    }
                }
                else
                {
                    if (Output.ContainsKey(offset))
                    {
                        var b = Output[offset];
                        int a = 0;
                        continue;
                    }
                    Output.Add(offset, chunk);
                }
            }
        }

        public void Draw(WorldRenderType Type)
        {
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            WorldRenderer.PrepareCameraMatrix();
            WorldRenderer.Render(DrawingChunks, ShadowDrawingChunks, Type);

            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public void Update()
        {
            _meshBuilder.Update();
            _chunkBuilder.Update();
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
            OpenSimplexNoise.Load(NewSeed == MenuSeed ? 23123123 : NewSeed); //Not really the menu seed.
            _meshBuilder.Discard();
            _chunkBuilder.Discard();
            _spawningPoint = FindSpawningPoint(GeneralSettings.SpawnPoint);
            var rng = new Random(Seed);
            _spawningVillagePoint = FindSpawningPoint(
                _spawningPoint
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
            {
                try
                {
                    this.RemoveChunk(chunks[i]);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.WriteLine(e);
                }
            }

            for (var i = Entities.Count - 1; i > -1; i--)
            {
                if (Entities[i] is LocalPlayer) continue;
                Entities[i].Dispose();
            }

            StructureHandler.Discard();
            WorldRenderer.ForceDiscard();
            CacheManager.Discard();

            this.AddEntity(GameManager.Player);

            if (NewSeed == MenuSeed)
                MenuBackground.Setup();
            
            GC.Collect();
        }

        public void Discard()
        {
            _meshBuilder.Discard();
            _chunkBuilder.Discard();
        }

        public T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            var results = new List<T>();
            var searchOptions = new List<T>();
            searchOptions.AddRange(Entities.OfType<T>());
            searchOptions.AddRange(StructureHandler.Structures.OfType<T>());

            for (var i = 0; i < searchOptions.Count; i++)
                if ((searchOptions[i].Position - Position).LengthSquared < Radius * Radius)
                    results.Add(searchOptions[i]);
            return results.ToArray();
        }

        public void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
            if (Chunk == null || Chunk.Disposed) return;
            if (!DoMesh) _chunkBuilder.Add(Chunk);
            else _meshBuilder.Add(Chunk);
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
       
            for (var i = Entities.Count - 1; i > -1; i--)
            {
                if (Entities[i] == null)
                {
                    this.RemoveEntity(Entities[i]);
                    continue;
                }

                if (Entities[i].BlockPosition.X < Chunk.OffsetX + Chunk.Width &&
                    Entities[i].BlockPosition.X > Chunk.OffsetX &&
                    Entities[i].BlockPosition.Z < Chunk.OffsetZ + Chunk.Width &&
                    Entities[i].BlockPosition.Z > Chunk.OffsetZ)
                    if (Entities[i].Removable && !(Entities[i] is IPlayer))
                        Entities[i].Dispose();
            }

            var items = WorldObjects;
            for (var i = items.Length - 1; i > -1; i--)
            {
                if (items[i] == null)
                {
                    this.RemoveObject(items[i]);
                    continue;
                }

                if (items[i].Position.X < Chunk.OffsetX + Chunk.Width && items[i].Position.X > Chunk.OffsetX &&
                    items[i].Position.Z < Chunk.OffsetZ + Chunk.Width && items[i].Position.Z > Chunk.OffsetZ)
                    items[i].Dispose();
            }
            _meshBuilder.Remove(Chunk);
            _chunkBuilder.Remove(Chunk);
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
            SearcheableChunks.TryGetValue(offset, out var chunk);
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
            var chunkSpace = this.ToChunkSpace(Vec3);
            var x = (int) Math.Abs(Math.Floor((Vec3.X - chunkSpace.X) / Chunk.BlockSize));
            var z = (int) Math.Abs(Math.Floor((Vec3.Z - chunkSpace.Y) / Chunk.BlockSize));
            return new Vector3(x, Math.Min(Vec3.Y / Chunk.BlockSize, Chunk.Height - 1), z);
        }

        public Vector2 ToChunkSpace(Vector3 Vec3)
        {
            var chunkX = ((int) Vec3.X >> 7) << 7;
            var chunkZ = ((int) Vec3.Z >> 7) << 7;

            return new Vector2(chunkX, chunkZ);
        }

        public Chunk GetChunkAt(Vector3 Coordinates)
        {
            var chunkSpace = this.ToChunkSpace(Coordinates);
            return GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
        }

        public Block GetBlockAt(int X, int Y, int Z)
        {
            return GetBlockAt(new Vector3(X, Y, Z));
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetHighestBlockAt((int) blockSpace.X, (int) blockSpace.Z) ?? new Block();
        }

        public int GetHighestY(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetHighestY((int) blockSpace.X, (int) blockSpace.Z) ?? 0;
        }

        public Block GetNearestBlockAt(int X, int Y, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetNearestBlockAt((int) blockSpace.X, Y, (int) blockSpace.Z) ?? new Block();
        }

        public int GetNearestY(int X, int Y, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetNearestY((int) blockSpace.X, Y, (int) blockSpace.Z) ?? 0;
        }

        public int GetLowestY(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetLowestY((int) blockSpace.X, (int) blockSpace.Z) ?? 0;
        }

        public float GetHighest(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetHighest((int) blockSpace.X, (int) blockSpace.Z) ?? 0;
        }

        public Block GetLowestBlock(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetLowestBlockAt((int) blockSpace.X, (int) blockSpace.Z) ?? new Block();
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
                    if ((model.Position - Player.Position).LengthSquared < 4 * 4)
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

        public SkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            var mob = MobFactory.Build(Type, MobSeed);
            var placeablePosition = this.FindPlaceablePosition(mob, new Vector3(DesiredPosition.X, Physics.HeightAtPosition(DesiredPosition.X, DesiredPosition.Z), DesiredPosition.Z));
            mob.MobId = ++_previousId;
            mob.Seed = MobSeed;
            mob.Model.TargetRotation = new Vector3(0, (new Random(MobSeed)).NextFloat() * 360f, 0);
            mob.Physics.TargetPosition = placeablePosition;
            mob.Model.Position = placeablePosition;
            MobFactory.Polish(mob);
            
            this.AddEntity(mob);
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
                return region.Generation.GetHeight(Point.X, Point.Z, null, out _) < Engine.BiomeSystem.BiomePool.SeaLevel
                    || LandscapeGenerator.River(point.Xz) > 0;
            }
            while (IsWater(point))
            {
                point += 
                    new Vector3( (192f * rng.NextFloat() - 96f) * Chunk.BlockSize, 0, (192f * rng.NextFloat() - 96f) * Chunk.BlockSize);
            }

            return point;
        }

        public Vector3 FindPlaceablePosition(IEntity Mob, Vector3 DesiredPosition)
        {
            var originalPosition = Mob.Physics.TargetPosition;
            var collidesOnSurface = true;
            Mob.Physics.TargetPosition = DesiredPosition;
            while (!Mob.Physics.Translate(Vector3.One * .1f))
            {
                DesiredPosition += new Vector3(Utils.Rng.NextFloat() * 32f - 16f, 0, Utils.Rng.NextFloat() * 32f - 16f);
                Mob.Physics.TargetPosition = DesiredPosition;
                Mob.Physics.UpdateColliders();
            }
            Mob.Physics.TargetPosition = originalPosition;
            return DesiredPosition;
        }

        private Vector3 ToBlockSpace(float X, float Z)
        {
            return ToBlockSpace(new Vector3(X, 0, Z));
        }

        private Vector2 ToChunkSpace(float X, float Z)
        {
            return ToChunkSpace(new Vector3(X, 0, Z));
        }

        public Block GetBlockAt(Vector3 Vec3)
        {
            var chunkSpace = this.ToChunkSpace(Vec3);
            var blockSpace = this.ToBlockSpace(Vec3);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetBlockAt((int) blockSpace.X, (int) Vec3.Y, (int) blockSpace.Z) ?? new Block();
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

        private bool _isGlobalCollidersCacheDirty = true;
        private readonly HashSet<ICollidable> _globalColliders;
        private ReadOnlyCollection<ICollidable> _globalCollidersCache;

        public ReadOnlyCollection<ICollidable> GlobalColliders
        {
            get
            {
                if (_isGlobalCollidersCacheDirty)
                {
                    lock (_globalColliders)
                    {
                        _globalCollidersCache = _globalColliders.ToArray().ToList().AsReadOnly();
                    }
                    _isGlobalCollidersCacheDirty = false;
                }
                lock (_globalCollidersCache)
                {
                    return _globalCollidersCache;
                }
            }
        }

        #endregion
    }
}