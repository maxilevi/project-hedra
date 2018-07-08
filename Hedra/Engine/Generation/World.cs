/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 04:16 p.m.
 *
 */
using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Engine.Rendering;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.Generation
{
	internal delegate void ModulesReloadEvent(string AppPath);

	internal static class World
	{
		public const float OverallDifficulty = 1;
	    public static Dictionary<Vector2, Chunk> SearcheableChunks { get; }
        public static AreaHighlighter Highlighter { get; private set; }
        public static ParticleSystem Particles { get; }
        public static ChunkComparer ChunkComparer { get; }
        public static EnviromentGenerator EnviromentGenerator { get; private set; }
        public static BiomePool BiomePool { get; private set; }
        public static MobFactory MobFactory { get; private set; }
        public static TreeGenerator TreeGenerator { get; private set; }
        public static QuestManager QuestManager { get; private set; }
        public static StructureGenerator StructureGenerator { get; private set; }
        public static event ModulesReloadEvent ModulesReload;
		public static int Seed { get; set; }
		public static bool Enabled {get; set;}
	    public static bool IsGenerated { get; set; }
	    private static SharedWorkerPool _workerPool;
	    private static MeshBuilder _meshBuilder;
	    private static ChunkBuilder _chunkBuilder;
        public static int MeshQueueCount => _meshBuilder.Count;
	    public static int ChunkQueueCount => _chunkBuilder.Count;

        #region Propierties
        private static bool _isChunksCacheDirty = true;
	    private static readonly HashSet<Chunk> _chunks;
	    private static ReadOnlyCollection<Chunk> _chunksCache;
	    public static ReadOnlyCollection<Chunk> Chunks
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
	                return _chunksCache;
	        }
	    }


        private static bool _isItemsCacheDirty = true;
	    private static readonly HashSet<WorldItem> _items;
	    private static ReadOnlyCollection<WorldItem> _itemsCache;
	    public static ReadOnlyCollection<WorldItem> Items
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
	                return _itemsCache;
	        }
	    }

        private static bool _isEntityCacheDirty = true;
        private static readonly HashSet<Entity> _entities;
	    private static ReadOnlyCollection<Entity> _entityCache;
	    public static ReadOnlyCollection<Entity> Entities
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
                    return _entityCache;
	        }
	    }

	    private static bool _isStructuresCacheDirty = true;
	    private static readonly HashSet<BaseStructure> _structures;
	    private static ReadOnlyCollection<BaseStructure> _structuresCache;
        public static ReadOnlyCollection<BaseStructure> Structures
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
	                return _structuresCache;
            }
	    }

	    private static bool _isGlobalCollidersCacheDirty = true;
	    private static readonly HashSet<ICollidable> _globalColliders;
	    private static ReadOnlyCollection<ICollidable> _globalCollidersCache;
        public static ReadOnlyCollection<ICollidable> GlobalColliders
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
	                return _globalCollidersCache;
            }
	    }
#endregion

        public static Dictionary<Vector2, Chunk> DrawingChunks { get; }
	    private static Matrix4 _previousModelView;
	    private static int _previousCount;
	    private static int _previousId;

	    static World()
	    {
            _workerPool = new SharedWorkerPool();
	        _meshBuilder = new MeshBuilder(_workerPool);
	        _chunkBuilder = new ChunkBuilder(_workerPool);
            _structures = new HashSet<BaseStructure>();
	        _entities = new HashSet<Entity>();
            _items = new HashSet<WorldItem>();
            _chunks = new HashSet<Chunk>();

            SearcheableChunks = new Dictionary<Vector2, Chunk>();
            _globalColliders = new HashSet<ICollidable>();
	        Particles = new ParticleSystem
	        {
	            HasMultipleOutputs = true
	        };
	        ChunkComparer = new ChunkComparer();
            DrawingChunks = new Dictionary<Vector2, Chunk>();
	    }

        public static void Load(){

			MenuBackground.Setup();
			
			Enabled = true;
			Seed = MenuSeed;
    
			BiomePool = new BiomePool();
			TreeGenerator = new TreeGenerator();
			QuestManager = new QuestManager();
			StructureGenerator = new StructureGenerator();
			EnviromentGenerator = new EnviromentGenerator();
            MobFactory = new MobFactory();
            Highlighter = new AreaHighlighter();

            ReloadModules();

            IsGenerated = true;
        }

	    public static void ReloadModules()
	    {
            MobFactory?.Empty();
	        AnimationLoader.EmptyCache();
	        AnimationModelLoader.EmptyCache();

            var factories = MobLoader.LoadModules(AssetManager.AppPath);
	        MobFactory?.AddFactory(factories);    
            HumanoidLoader.LoadModules(AssetManager.AppPath);
            ItemFactory.LoadModules(AssetManager.AppPath);
            World.ModulesReload?.Invoke(AssetManager.AppPath);
	    }
		
		public static int MenuSeed => 2124321422;//23123123

        public static int RandomSeed{
			get{
				int newSeed = MenuSeed;
				while(newSeed == MenuSeed){
					newSeed = Utils.Rng.Next(1, int.MaxValue / 2);
				}
				return newSeed;
			}
		}

	    public static void CullTest(FrustumCulling FrustumObject){
			
			if(_previousModelView == FrustumObject.ModelViewMatrix && LocalPlayer.Instance.Loader.ActiveChunks == _previousCount) 
				return;
			
			DrawingChunks.Clear();
			var toDrawArray = Chunks;
			for(var i = 0; i < toDrawArray.Count; i++){
				if(toDrawArray[i] == null || toDrawArray[i].Disposed){
					RemoveChunk(toDrawArray[i]);
					continue;
				}
				
				if( !WorldRenderer.EnableCulling || toDrawArray[i].Initialized && FrustumObject.IsInsideFrustum(toDrawArray[i].Mesh))
				DrawingChunks.Add(new Vector2(toDrawArray[i].OffsetX, toDrawArray[i].OffsetZ), toDrawArray[i]);				
			}
			
			_previousModelView = FrustumObject.ModelViewMatrix;
			_previousCount = LocalPlayer.Instance.Loader.ActiveChunks;
		}
		
		public static void Draw(ChunkBufferTypes Type){
			if(!Enabled)
				return;
			
			if(GameSettings.Wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			
            WorldRenderer.PrepareRendering();
			WorldRenderer.Render(DrawingChunks, Type);
			
			if(GameSettings.Wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

	    public static void Update()
	    {
	        _meshBuilder.Update();
            _chunkBuilder.Update();
	    }
		
		public static void Recreate(int Seed)
        {
			if(World.Seed == Seed)
				return;

		    _previousId = 0;
		    World.Seed = Seed;
            BiomePool = new BiomePool();
		    StructureGenerator = new StructureGenerator();
		    QuestManager = new QuestManager();
            OpenSimplexNoise.Load(Seed == MenuSeed ? 23123123 : Seed);//Not really the menu seed.
		    _meshBuilder.Discard();
			_chunkBuilder.Discard();
			SkyManager.SetTime(12000);

			for(var i = Items.Count-1; i > -1; i--){
				Items[i].Dispose();
			}
		    Highlighter.Reset();

            lock(TreeGenerator)
                TreeGenerator = new TreeGenerator();

            lock (SearcheableChunks)
                SearcheableChunks.Clear();
		    for (int i = Items.Count - 1; i > -1; i--)
		    {
		        World.RemoveItem(Items[i]);
		    }

		    var chunks = Chunks;
            for (int i = chunks.Count - 1; i > -1; i--)
		    {
		        try
		        {
		            World.RemoveChunk(chunks[i]);
		        }
		        catch (ArgumentOutOfRangeException e)
		        {
		            Log.WriteLine(e);
		        }
		    }

		    for (int i = GlobalColliders.Count - 1; i > -1; i--)
		    {
		        World.RemoveGlobalCollider(GlobalColliders[i]);
		    }

            for (int i = Structures.Count-1; i > -1; i--){
                World.RemoveStructure(Structures[i]);
			}

			for(int i = Entities.Count-1; i > -1; i--){
				if(Entities[i] is LocalPlayer) continue;
                Entities[i].Dispose();
			}		

			WorldRenderer.ForceDiscard();
		    CacheManager.Discard();

		    World.AddEntity(GameManager.Player);

            if (Seed == MenuSeed)
			{
			    MenuBackground.Setup();
			}
        }

	    public static void Discard()
	    {
	        _meshBuilder.Discard();
            _chunkBuilder.Discard();
	    }

		public static void RemoveInstances(Vector3 Position, int Radius){
			lock(Chunks){
				for(int i = 0; i < Chunks.Count; i++){
					if( new Vector2(Chunks[i].OffsetX - Position.X, Chunks[i].OffsetZ - Position.Z).LengthSquared <= Radius*Radius){
						//A pointer to the list
						List<InstanceData> instances = Chunks[i].StaticBuffer.InstanceElements;
						List<ICollidable> shapes = Chunks[i].StaticBuffer.CollisionBoxes;
						for(int j = instances.Count-1; j > -1; j--){
							if( (instances[j].TransMatrix.ExtractTranslation().Xz - Position.Xz).LengthSquared <= Radius*Radius)
								instances.RemoveAt(j);
						}
						lock(shapes){
							for(int j = shapes.Count-1; j > -1; j--){
								if( shapes[j] is CollisionShape && ( ((CollisionShape) shapes[j]).BroadphaseCenter.Xz - Position.Xz).LengthSquared <= Radius*Radius)
									shapes.RemoveAt(j);
							}
						}
						AddChunkToQueue(Chunks[i], true);
					}
				}
			}
		}

	    public static T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
	    {
	        var results = new List<T>();
	        var searchOptions = new List<T>();
            searchOptions.AddRange(Entities.OfType<T>());
            searchOptions.AddRange(Structures.OfType<T>());

	        for (var i = 0; i < searchOptions.Count; i++)
	        {
                if( (searchOptions[i].Position - Position).LengthSquared < Radius * Radius)
	                results.Add(searchOptions[i]);
	        }
	        return results.ToArray();
	    }

        public static void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
            if(Chunk == null || Chunk.Disposed) return;
            if (!DoMesh) _chunkBuilder.Add(Chunk);
            else _meshBuilder.Add(Chunk);            
        }
		
		public static Chunk GetChunkByOffset(Vector2 vec2){
			
			 return GetChunkByOffset((int)vec2.X, (int)vec2.Y);
		}

	    public static void AddEntity(Entity Entity)
	    {
	        lock (_entities)
	        {
	            if (_entities.Contains(Entity)) return;
                    _entities.Add(Entity);
	        }

	        _isEntityCacheDirty = true;

	    }

	    public static void RemoveEntity(Entity Entity)
	    {
	        lock (_entities)
	        {
	            if (!_entities.Contains(Entity)) return;
	            _entities.Remove(Entity);
	        }
	        _isEntityCacheDirty = true;

	    }

        public static void AddStructure(BaseStructure Struct){
			lock(_structures)
				_structures.Add(Struct);
            _isStructuresCacheDirty = true;
        }
		
		public static void RemoveStructure(BaseStructure Struct){
			Struct.Dispose();
			lock(_structures)
				_structures.Remove(Struct);
		    _isStructuresCacheDirty = true;
        }

	    public static void AddGlobalCollider(params ICollidable[] Collidable)
	    {
	        lock (_globalColliders)
	        {
	            for (var i = 0; i < Collidable.Length; i++)
	            {
	                _globalColliders.Add(Collidable[i]);
	            }
	        }
	        _isGlobalCollidersCacheDirty = true;
	    }

	    public static void RemoveGlobalCollider(ICollidable Collidable)
	    {
	        lock (_globalColliders)
	            _globalColliders.Remove(Collidable);
	        _isGlobalCollidersCacheDirty = true;
        }

	    public static void AddItem(WorldItem Item)
	    {
	        lock (_items)
	        {
	            if (_items.Contains(Item)) return;
	            _items.Add(Item);
	        }
	        _isItemsCacheDirty = true;
        }

	    public static void RemoveItem(WorldItem Item)
	    {
	        lock (_items)
	        {
	            if (!_items.Contains(Item)) return;
	            _items.Remove(Item);
	        }
	        _isItemsCacheDirty = true;
        }

        public static void AddChunk(Chunk Chunk){
            lock (_chunks)
            {
                _chunks.Add(Chunk);
            }
            lock(SearcheableChunks){
				if(!SearcheableChunks.ContainsKey(new Vector2(Chunk.OffsetX, Chunk.OffsetZ)))
					SearcheableChunks.Add(new Vector2(Chunk.OffsetX, Chunk.OffsetZ), Chunk);
			}
            _isChunksCacheDirty = true;
        }
		
		public static void RemoveChunk(Chunk Chunk){
			if(Chunk == null) return;

		    lock (_chunks)
            { 
                _chunks.Remove(Chunk);
            }
			lock(SearcheableChunks) SearcheableChunks.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));

		    _isChunksCacheDirty = true;
            WorldRenderer.StaticBuffer.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));
			WorldRenderer.WaterBuffer.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));

			BaseStructure[] baseStructuresArray = Structures.ToArray();
			
			for(int i = baseStructuresArray.Length-1; i > -1; i--)
			{
			    var chunk = GetChunkAt(baseStructuresArray[i].Position);
				if(chunk != Chunk) continue;
                    World.RemoveStructure(baseStructuresArray[i]);		
			}
			

			for(int i = Entities.Count-1; i > -1; i--){
				if(Entities[i] == null)
				{
					World.RemoveEntity(Entities[i]);
					continue;
				}
					
				if(Entities[i].BlockPosition.X < Chunk.OffsetX + Chunk.Width && Entities[i].BlockPosition.X > Chunk.OffsetX &&
					Entities[i].BlockPosition.Z < Chunk.OffsetZ + Chunk.Width && Entities[i].BlockPosition.Z > Chunk.OffsetZ) {
				    if (Entities[i].Removable && !Entities[i].IsBoss && Entities[i].MobType != MobType.Human &&
				        !(Entities[i] is LocalPlayer))
				    {

				        Entities[i].Dispose();
				    }
				}
			}
			
			for(int i = Items.Count-1; i > -1; i--){
			    if (Items[i] == null)
			    {
			        World.RemoveItem(Items[i]);
			        continue;
			    }

			    if (Items[i].Position.X < Chunk.OffsetX + Chunk.Width && Items[i].Position.X > Chunk.OffsetX &&
				   Items[i].Position.Z < Chunk.OffsetZ + Chunk.Width && Items[i].Position.Z > Chunk.OffsetZ){
					Items[i].Dispose();
				}
			}
            _meshBuilder.Remove(Chunk);
		    _chunkBuilder.Remove(Chunk);
            Chunk.Dispose();
		}

        public static Chunk GetChunkByOffset(int OffsetX, int OffsetZ){
			lock(SearcheableChunks)
			{
			    return SearcheableChunks.ContainsKey(new Vector2(OffsetX, OffsetZ)) ? SearcheableChunks[new Vector2(OffsetX, OffsetZ)] : null;
			}
		}

	    public static bool IsChunkOffset(Vector2 Offset)
	    {
	        return Offset.X % Chunk.Width == 0 && Offset.Y % Chunk.Width == 0;
	    }

	    public static Vector3 ToBlockSpace(float X, float Z)
	    {
	        return ToBlockSpace(new Vector3(X, 0, Z));
	    }

	    public static Vector2 ToChunkSpace(float X, float Z)
	    {
	        return ToChunkSpace(new Vector3(X, 0, Z));
	    }

        public static Vector3 ToBlockSpace(Vector3 Vec3)
        {
			var chunkSpace = World.ToChunkSpace(Vec3);		
			var x = (int) Math.Abs(Math.Floor( (Vec3.X - chunkSpace.X) / Chunk.BlockSize ));
			var z = (int) Math.Abs(Math.Floor( (Vec3.Z - chunkSpace.Y) / Chunk.BlockSize ));	
			return new Vector3(x, Math.Min(Vec3.Y / Chunk.BlockSize, Chunk.Height-1),  z);
		}
		public static Vector2 ToChunkSpace(Vector3 Vec3){
			int chunkX = ((int)Vec3.X >> 7) << 7;
			int chunkZ = ((int)Vec3.Z >> 7) << 7;
			
			return new Vector2(chunkX, chunkZ);
		}

        public static Chunk GetChunkAt(Vector3 Coordinates)
	    {
	        var chunkSpace = World.ToChunkSpace(Coordinates);
	        return GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
	    }

        public static Block GetBlockAt(int X, int Y, int Z)
	    {
	        return GetBlockAt(new Vector3(X, Y, Z));
	    }

        public static Block GetBlockAt(Vector3 Vec3)
        {
            var chunkSpace = World.ToChunkSpace(Vec3);
            var blockSpace = World.ToBlockSpace(Vec3);
			
			var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
			return blockChunk?.GetBlockAt((int)blockSpace.X, (int) Vec3.Y, (int) blockSpace.Z) ?? new Block();
        }

	    public static Block GetHighestBlockAt(float X, float Z)
	    {
	        return GetHighestBlockAt( (int) X, (int) Z);
	    }

	    public static Block GetHighestBlockAt(int X, int Z){
	        var chunkSpace = World.ToChunkSpace(X,Z);
	        var blockSpace = World.ToBlockSpace(X,Z);

	        var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
	        return blockChunk?.GetHighestBlockAt((int)blockSpace.X, (int)blockSpace.Z) ?? new Block();
        }
		public static int GetHighestY(int X, int Z){
		    var chunkSpace = World.ToChunkSpace(X, Z);
		    var blockSpace = World.ToBlockSpace(X, Z);

		    var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
		    return blockChunk?.GetHighestY((int)blockSpace.X, (int)blockSpace.Z) ?? 0;
        }
		
		public static Block GetNearestBlockAt(int X, int Y, int Z){
		    var chunkSpace = World.ToChunkSpace(X, Z);
		    var blockSpace = World.ToBlockSpace(X, Z);

		    var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
		    return blockChunk?.GetNearestBlockAt((int)blockSpace.X, Y, (int)blockSpace.Z) ?? new Block();
        }
		
		public static int GetNearestY(int X, int y, int Z){
		    var chunkSpace = World.ToChunkSpace(X, Z);
		    var blockSpace = World.ToBlockSpace(X, Z);

		    var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
		    return blockChunk?.GetNearestY((int)blockSpace.X, y, (int)blockSpace.Z) ?? 0;
        }
		
		public static int GetLowestY(int X, int Z){
		    var chunkSpace = World.ToChunkSpace(X, Z);
		    var blockSpace = World.ToBlockSpace(X, Z);

		    var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
		    return blockChunk?.GetLowestY((int)blockSpace.X, (int)blockSpace.Z) ?? 0;
        }

	    public static Block GetLowestBlock(int X, int Z)
	    {
	        var chunkSpace = World.ToChunkSpace(X, Z);
	        var blockSpace = World.ToBlockSpace(X, Z);

	        var blockChunk = GetChunkByOffset((int)chunkSpace.X, (int)chunkSpace.Y);
	        return blockChunk?.GetLowestBlockAt((int)blockSpace.X, (int)blockSpace.Z) ?? new Block();
        }

        public static void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
	    {
	        Highlighter.HighlightArea(Position, Color, Radius, Seconds);
	    }

        public static WorldItem DropItem(Item ItemSpec, Vector3 Position){
			var model = new WorldItem(ItemSpec, Position);
			World.AddItem(model);
			
			model.OnPickup += delegate(LocalPlayer Player)
			{
                TaskManager.While(() => !model.Disposed, delegate
                {
                    model.Model.Outline = false;
                    model.Position = Mathf.Lerp(model.Position, Player.Position, (float) Time.deltaTime * 5f);
                    if ((model.Position - Player.Position).LengthSquared < 4*4)
                    {
                        if (Player.Inventory.AddItem(model.ItemSpecification))
                        {
                            model.Enabled = false;
                            Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, model.Position, false, 1f,
                                1.2f);
                            model.Dispose();
                        }
                    }
                });
            };
			return model;
		}
		
		public static Entity SpawnMob(MobType Type, Vector3 DesiredPosition, Random SeedRng){
			int mobSeed = SeedRng.Next(ushort.MinValue, ushort.MaxValue);
			return SpawnMob(Type.ToString(), DesiredPosition, mobSeed);
		}

	    public static Entity SpawnMob(MobType Type, Vector3 DesiredPosition, int MobSeed)
	    {
	        return SpawnMob(Type.ToString(), DesiredPosition, MobSeed);
	    }

        public static Entity SpawnMob(string Type, Vector3 DesiredPosition, Random SeedRng)
	    {
	        int mobSeed = SeedRng.Next(ushort.MinValue, ushort.MaxValue);
	        return SpawnMob(Type, DesiredPosition, mobSeed);
	    }

        public static Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
		{
            
		    Entity mob = MobFactory.Build(Type, MobSeed);
		    Vector3 placeablePosition = World.FindPlaceablePosition(mob, DesiredPosition); 
		    mob.MobId = ++_previousId;
		    mob.MobSeed = MobSeed;
		    mob.Model.TargetRotation = new Vector3(0, new Random(MobSeed).NextFloat() * 360f, 0);
            mob.Physics.TargetPosition = placeablePosition;
		    mob.Model.Position = placeablePosition;

            World.AddEntity(mob);

			return mob;
		}

	    public static Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition)
	    {
	        Vector3 position = DesiredPosition;
	        Chunk underChunk = World.GetChunkAt(position);
	        var collidesOnSurface = true;
	        Box box = Mob.Model.BaseBroadphaseBox.Cache.Translate(position.Xz.ToVector3()
                + Vector3.UnitY * Physics.HeightAtPosition(position.X, position.Z));
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
	                position = position + new Vector3(Utils.Rng.NextFloat() * 32f - 16f, 0, Utils.Rng.NextFloat() * 32f - 16f);
	                underChunk = World.GetChunkAt(position);
	                box = Mob.Model.BaseBroadphaseBox.Cache.Translate(position.Xz.ToVector3() 
                        + Vector3.UnitY * Physics.HeightAtPosition(position.X, position.Z));
	            }

	        }
	        return position;
	    }
	}
}
