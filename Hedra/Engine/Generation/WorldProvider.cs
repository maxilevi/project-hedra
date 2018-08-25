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
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Generation
{
    public class WorldProvider : IWorldProvider
    {
        private readonly ChunkBuilder _chunkBuilder;
        private readonly MeshBuilder _meshBuilder;
        private readonly SharedWorkerPool _workerPool;
        private int _previousCount;
        private int _previousId;
        private Matrix4 _previousModelView;

        public WorldProvider()
        {
            _workerPool = new SharedWorkerPool();
            _meshBuilder = new MeshBuilder(_workerPool);
            _chunkBuilder = new ChunkBuilder(_workerPool);
            _structures = new HashSet<BaseStructure>();
            _entities = new HashSet<IEntity>();
            _items = new HashSet<WorldItem>();
            _chunks = new HashSet<Chunk>();
            _globalColliders = new HashSet<ICollidable>();
            SearcheableChunks = new Dictionary<Vector2, Chunk>();
            DrawingChunks = new Dictionary<Vector2, Chunk>();
        }

        public event ModulesReloadEvent ModulesReload;

        public Dictionary<Vector2, Chunk> SearcheableChunks { get; }
        public AreaHighlighter Highlighter { get; private set; }
        public ParticleSystem Particles { get; private set; }
        public EnviromentGenerator EnviromentGenerator { get; private set; }
        public BiomePool BiomePool { get; private set; }
        public MobFactory MobFactory { get; private set; }
        public TreeGenerator TreeGenerator { get; private set; }
        public WorldBuilding.WorldBuilding WorldBuilding { get; private set; }
        public StructureGenerator StructureGenerator { get; private set; }
        public int Seed { get; private set; }
        public bool IsGenerated { get; private set; }
        public int MeshQueueCount => _meshBuilder.Count;
        public int ChunkQueueCount => _chunkBuilder.Count;

        public Dictionary<Vector2, Chunk> DrawingChunks { get; }

        public void Load()
        {
            MenuBackground.Setup();

            Seed = MenuSeed;
            BiomePool = new BiomePool();
            TreeGenerator = new TreeGenerator();
            WorldBuilding = new WorldBuilding.WorldBuilding();
            StructureGenerator = new StructureGenerator();
            EnviromentGenerator = new EnviromentGenerator();
            MobFactory = new MobFactory();
            Highlighter = new AreaHighlighter();
            Particles = new ParticleSystem
            {
                HasMultipleOutputs = true
            };
            ReloadModules();
            IsGenerated = true;
        }

        public void ReloadModules()
        {
            MobFactory?.Empty();
            AnimationLoader.EmptyCache();
            AnimationModelLoader.EmptyCache();

            var factories = MobLoader.LoadModules(AssetManager.AppPath);
            MobFactory?.AddFactory(factories);
            ItemFactory.LoadModules(AssetManager.AppPath);
            VillageLoader.LoadModules(AssetManager.AppPath);
            HumanoidLoader.LoadModules(AssetManager.AppPath);
            ModulesReload?.Invoke(AssetManager.AppPath);
        }

        public int MenuSeed => 2124321422; //23123123

        public int RandomSeed
        {
            get
            {
                var newSeed = MenuSeed;
                while (newSeed == MenuSeed)
                    newSeed = Utils.Rng.Next(1, int.MaxValue / 2);
                return newSeed;
            }
        }

        public void CullTest(FrustumCulling FrustumObject)
        {
            if (_previousModelView == FrustumObject.ModelViewMatrix &&
                LocalPlayer.Instance.Loader.ActiveChunks == _previousCount)
                return;

            DrawingChunks.Clear();
            var toDrawArray = Chunks;
            for (var i = 0; i < toDrawArray.Count; i++)
            {
                var offset = new Vector2(toDrawArray[i].OffsetX, toDrawArray[i].OffsetZ);
                var chunk = toDrawArray[i];
                if (chunk == null || chunk.Disposed)
                {
                    RemoveChunk(chunk);
                    continue;
                }

                if (!WorldRenderer.EnableCulling || chunk.Initialized && FrustumObject.IsInsideFrustum(chunk.Mesh))
                    DrawingChunks.Add(offset, chunk);
            }

            _previousModelView = FrustumObject.ModelViewMatrix;
            _previousCount = LocalPlayer.Instance.Loader.ActiveChunks;
        }

        public void Draw(ChunkBufferTypes Type)
        {
            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            WorldRenderer.PrepareRendering();
            WorldRenderer.Render(DrawingChunks, Type);

            if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public void Update()
        {
            _meshBuilder.Update();
            _chunkBuilder.Update();
        }

        public void Recreate(int NewSeed)
        {
            if (this.Seed == NewSeed)
                return;

            _previousId = 0;
            this.Seed = NewSeed;
            BiomePool = new BiomePool();
            WorldBuilding = new WorldBuilding.WorldBuilding();
            OpenSimplexNoise.Load(NewSeed == MenuSeed ? 23123123 : NewSeed); //Not really the menu seed.
            _meshBuilder.Discard();
            _chunkBuilder.Discard();
            SkyManager.SetTime(12000);

            for (var i = Items.Count - 1; i > -1; i--)
                Items[i].Dispose();
            Highlighter.Reset();

            lock (TreeGenerator)
            {
                TreeGenerator = new TreeGenerator();
            }

            lock (SearcheableChunks)
            {
                SearcheableChunks.Clear();
            }
            for (var i = Items.Count - 1; i > -1; i--)
                this.RemoveItem(Items[i]);

            var chunks = Chunks;
            for (var i = chunks.Count - 1; i > -1; i--)
                try
                {
                    this.RemoveChunk(chunks[i]);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.WriteLine(e);
                }

            for (var i = GlobalColliders.Count - 1; i > -1; i--)
                this.RemoveGlobalCollider(GlobalColliders[i]);

            for (var i = Structures.Count - 1; i > -1; i--)
                this.RemoveStructure(Structures[i]);

            for (var i = Entities.Count - 1; i > -1; i--)
            {
                if (Entities[i] is LocalPlayer) continue;
                Entities[i].Dispose();
            }

            StructureGenerator.Discard();
            WorldRenderer.ForceDiscard();
            CacheManager.Discard();

            this.AddEntity(GameManager.Player);

            if (NewSeed == MenuSeed)
                MenuBackground.Setup();
        }

        public void Discard()
        {
            _meshBuilder.Discard();
            _chunkBuilder.Discard();
        }

        public void RemoveInstances(Vector3 Position, int Radius)
        {
            lock (Chunks)
            {
                for (var i = 0; i < Chunks.Count; i++)
                    if (new Vector2(Chunks[i].OffsetX - Position.X, Chunks[i].OffsetZ - Position.Z).LengthSquared <=
                        Radius * Radius)
                    {
                        //A pointer to the list
                        var instances = Chunks[i].StaticBuffer.InstanceElements;
                        var shapes = Chunks[i].StaticBuffer.CollisionBoxes;
                        for (var j = instances.Count - 1; j > -1; j--)
                            if ((instances[j].TransMatrix.ExtractTranslation().Xz - Position.Xz).LengthSquared <=
                                Radius * Radius)
                                instances.RemoveAt(j);
                        lock (shapes)
                        {
                            for (var j = shapes.Count - 1; j > -1; j--)
                                if (shapes[j] is CollisionShape &&
                                    (((CollisionShape) shapes[j]).BroadphaseCenter.Xz - Position.Xz).LengthSquared <=
                                    Radius * Radius)
                                    shapes.RemoveAt(j);
                        }
                        AddChunkToQueue(Chunks[i], true);
                    }
            }
        }

        public T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            var results = new List<T>();
            var searchOptions = new List<T>();
            searchOptions.AddRange(Entities.OfType<T>());
            searchOptions.AddRange(Structures.OfType<T>());

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

        public Chunk GetChunkByOffset(Vector2 vec2)
        {
            return GetChunkByOffset((int) vec2.X, (int) vec2.Y);
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

        public void AddStructure(BaseStructure Struct)
        {
            lock (_structures)
            {
                _structures.Add(Struct);
            }
            _isStructuresCacheDirty = true;
        }

        public void RemoveStructure(BaseStructure Struct)
        {
            Struct.Dispose();
            lock (_structures)
            {
                _structures.Remove(Struct);
            }
            _isStructuresCacheDirty = true;
        }

        public void AddGlobalCollider(params ICollidable[] Collidable)
        {
            lock (_globalColliders)
            {
                for (var i = 0; i < Collidable.Length; i++)
                    _globalColliders.Add(Collidable[i]);
            }
            _isGlobalCollidersCacheDirty = true;
        }

        public void RemoveGlobalCollider(ICollidable Collidable)
        {
            lock (_globalColliders)
            {
                _globalColliders.Remove(Collidable);
            }
            _isGlobalCollidersCacheDirty = true;
        }

        public void AddItem(WorldItem Item)
        {
            lock (_items)
            {
                if (_items.Contains(Item)) return;
                _items.Add(Item);
            }
            _isItemsCacheDirty = true;
        }

        public void RemoveItem(WorldItem Item)
        {
            lock (_items)
            {
                if (!_items.Contains(Item)) return;
                _items.Remove(Item);
            }
            _isItemsCacheDirty = true;
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

            lock (_chunks)
            {
                _chunks.Remove(Chunk);
            }
            lock (SearcheableChunks)
            {
                SearcheableChunks.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));
            }

            _isChunksCacheDirty = true;
            WorldRenderer.StaticBuffer.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));
            WorldRenderer.WaterBuffer.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));

            var baseStructuresArray = Structures.ToArray();

            for (var i = baseStructuresArray.Length - 1; i > -1; i--)
            {
                var chunk = GetChunkAt(baseStructuresArray[i].Position);
                if (chunk != Chunk) continue;
                this.RemoveStructure(baseStructuresArray[i]);
            }


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
                    if (Entities[i].Removable && !Entities[i].IsBoss && Entities[i].MobType != MobType.Human &&
                        !(Entities[i] is LocalPlayer))
                        Entities[i].Dispose();
            }

            for (var i = Items.Count - 1; i > -1; i--)
            {
                if (Items[i] == null)
                {
                    this.RemoveItem(Items[i]);
                    continue;
                }

                if (Items[i].Position.X < Chunk.OffsetX + Chunk.Width && Items[i].Position.X > Chunk.OffsetX &&
                    Items[i].Position.Z < Chunk.OffsetZ + Chunk.Width && Items[i].Position.Z > Chunk.OffsetZ)
                    Items[i].Dispose();
            }
            _meshBuilder.Remove(Chunk);
            _chunkBuilder.Remove(Chunk);
            Chunk.Dispose();
        }

        public Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            lock (SearcheableChunks)
            {
                var offset = new Vector2(OffsetX, OffsetZ);
                return SearcheableChunks.ContainsKey(offset) ? SearcheableChunks[offset] : null;
            }
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

        public Block GetLowestBlock(int X, int Z)
        {
            var chunkSpace = this.ToChunkSpace(X, Z);
            var blockSpace = this.ToBlockSpace(X, Z);

            var blockChunk = GetChunkByOffset((int) chunkSpace.X, (int) chunkSpace.Y);
            return blockChunk?.GetLowestBlockAt((int) blockSpace.X, (int) blockSpace.Z) ?? new Block();
        }

        public void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            Highlighter.HighlightArea(Position, Color, Radius, Seconds);
        }

        public WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            var model = new WorldItem(ItemSpec, Position);
            this.AddItem(model);

            model.OnPickup += delegate(IPlayer Player)
            {
                TaskManager.While(() => !model.Disposed, delegate
                {
                    model.Outline = false;
                    model.Position = Mathf.Lerp(model.Position, Player.Position, Time.DeltaTime * 5f);
                    if ((model.Position - Player.Position).LengthSquared < 4 * 4)
                        if (Player.Inventory.AddItem(model.ItemSpecification))
                        {
                            model.Enabled = false;
                            SoundManager.PlaySound(SoundType.NotificationSound, model.Position, false, 1f,
                                1.2f);
                            model.Dispose();
                        }
                });
            };
            return model;
        }

        public Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            var mob = MobFactory.Build(Type, MobSeed);
            var placeablePosition = this.FindPlaceablePosition(mob, DesiredPosition);
            mob.MobId = ++_previousId;
            mob.MobSeed = MobSeed;
            mob.Model.TargetRotation = new Vector3(0, new Random(MobSeed).NextFloat() * 360f, 0);
            mob.Physics.TargetPosition = placeablePosition;
            mob.Model.Position = placeablePosition;

            this.AddEntity(mob);

            return mob;
        }

        public Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition)
        {
            var position = DesiredPosition;
            var underChunk = this.GetChunkAt(position);
            var collidesOnSurface = true;
            var box = Mob.Model.BaseBroadphaseBox.Cache.Translate(position.Xz.ToVector3()
                                                                  + Vector3.UnitY *
                                                                  Physics.HeightAtPosition(position.X, position.Z));
            while (underChunk != null && collidesOnSurface)
            {
                try
                {
                    collidesOnSurface = underChunk.CollisionShapes.Any(Shape => Physics.Collides(Shape, box));
                }
                catch (InvalidOperationException e)
                {
                    Log.WriteLine(e.Message);
                    continue;
                }
                if (collidesOnSurface)
                {
                    position = position + new Vector3(Utils.Rng.NextFloat() * 32f - 16f, 0,
                                   Utils.Rng.NextFloat() * 32f - 16f);
                    underChunk = this.GetChunkAt(position);
                    box = Mob.Model.BaseBroadphaseBox.Cache.Translate(position.Xz.ToVector3()
                                                                      + Vector3.UnitY *
                                                                      Physics.HeightAtPosition(position.X, position.Z));
                }
            }
            return position;
        }

        public Vector3 ToBlockSpace(float X, float Z)
        {
            return ToBlockSpace(new Vector3(X, 0, Z));
        }

        public Vector2 ToChunkSpace(float X, float Z)
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

        public Block GetHighestBlockAt(float X, float Z)
        {
            return GetHighestBlockAt((int) X, (int) Z);
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


        private bool _isItemsCacheDirty = true;
        private readonly HashSet<WorldItem> _items;
        private ReadOnlyCollection<WorldItem> _itemsCache;

        public ReadOnlyCollection<WorldItem> Items
        {
            get
            {
                if (_isItemsCacheDirty)
                {
                    lock (_items)
                    {
                        _itemsCache = _items.ToArray().ToList().AsReadOnly();
                    }
                    _isItemsCacheDirty = false;
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

        private bool _isStructuresCacheDirty = true;
        private readonly HashSet<BaseStructure> _structures;
        private ReadOnlyCollection<BaseStructure> _structuresCache;

        public ReadOnlyCollection<BaseStructure> Structures
        {
            get
            {
                if (_isStructuresCacheDirty)
                {
                    lock (_structures)
                    {
                        _structuresCache = _structures.ToArray().ToList().AsReadOnly();
                    }
                    _isStructuresCacheDirty = false;
                }
                lock (_structuresCache)
                {
                    return _structuresCache;
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