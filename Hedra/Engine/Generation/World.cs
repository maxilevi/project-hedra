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
using Hedra.Engine.Enviroment;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Item;
using Hedra.Engine.Networking;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.Generation
{
	public delegate void ModulesReloadEvent(string AppPath);

	public static class World
	{
		public const float OverallDifficulty = 1;
	    public static Dictionary<Vector2, Chunk> SearcheableChunks;
        public static List<Chunk> Chunks;
	    public static List<ParticleProjectile> Projectiles;
	    public static List<ItemModel> Items;
	    public static AreaHighlighter Highlighter;
	    public static ParticleSystem WorldParticles;
		public static GenerationQueue ChunkGenerationQueue;
		public static MeshBuilderQueue MeshQueue;
	    public static ClosestChunk ClosestChunkComparer;
		public static EnviromentGenerator EnviromentGenerator;
		public static BiomePool BiomePool;
	    public static MobFactory MobFactory;
		public static TreeGenerator TreeGenerator;
		public static QuestManager QuestManager;
		public static StructureGenerator StructureGenerator;
	    public static event ModulesReloadEvent ModulesReload;
		public static int Seed { get; set; }
		public static bool Enabled {get; set;}
	    public static bool IsGenerated { get; set; }

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

        private static readonly Dictionary<Vector2, Chunk> _toDraw;
	    private static Matrix4 _previousModelView;
	    private static int _previousCount;
	    private static int _previousId;

	    static World()
	    {
	        _structures = new HashSet< BaseStructure > ();
	        _entities = new HashSet<Entity>();
            Chunks = new List<Chunk>();
            SearcheableChunks = new Dictionary<Vector2, Chunk>();
            Projectiles = new List<ParticleProjectile>();
            _globalColliders = new HashSet<ICollidable>();
            Items = new List<ItemModel>();
            WorldParticles = new ParticleSystem(Vector3.Zero);
            ClosestChunkComparer = new ClosestChunk();
            _toDraw = new Dictionary<Vector2, Chunk>();
	    }

        public static void Load(){

			MenuBackground.Setup();
			
			Enabled = true;
			Seed = MenuSeed;

			MeshQueue = new MeshBuilderQueue();
			ChunkGenerationQueue = new GenerationQueue();     
			BiomePool = new BiomePool();
			TreeGenerator = new TreeGenerator();
			QuestManager = new QuestManager();
			StructureGenerator = new StructureGenerator();
			EnviromentGenerator = new EnviromentGenerator();
            MobFactory = new MobFactory();
            Highlighter = new AreaHighlighter();

            ReloadModules();

            VillageGenerator.Load();
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
			
			_toDraw.Clear();
			Chunk[] toDrawArray;

			lock(Chunks) 
				toDrawArray = Chunks.ToArray();

			for(int i = 0; i < toDrawArray.Length; i++){
				if(toDrawArray[i].Disposed){
					RemoveChunk(toDrawArray[i]);
					continue;
				}
				
				if( toDrawArray[i].Initialized && FrustumObject.IsInsideFrustum(toDrawArray[i].Mesh) )
					_toDraw.Add(new Vector2(toDrawArray[i].OffsetX, toDrawArray[i].OffsetZ), toDrawArray[i]);
				
			}
			
			_previousModelView = FrustumObject.ModelViewMatrix;
			_previousCount = LocalPlayer.Instance.Loader.ActiveChunks;
		}
		
		public static void Draw(ChunkBufferTypes Type){
			if(Constants.HIDE_CHUNKS || !Enabled)
				return;
			
			if(Constants.LINES)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			
			WorldRenderer.Render(_toDraw, Type);
			
			if(Constants.LINES)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}
		
		public static void Recreate(int Seed){
			if(World.Seed == Seed)
				return;

		    _previousId = 0;
		    World.Seed = Seed;
            BiomePool = new BiomePool();
		    StructureGenerator = new StructureGenerator();
		    QuestManager = new QuestManager();
            OpenSimplexNoise.Load(Seed == MenuSeed ? 23123123 : Seed);//Not really the menu seed.
			MeshQueue.SafeDiscard();
			ChunkGenerationQueue.SafeDiscard();
			SkyManager.SetTime(12000);

			for(int i = 0; i < Items.Count; i++){
				Items[i].Dispose();
			}

			Items.Clear();
		    Highlighter.Reset();

            lock(TreeGenerator)
                TreeGenerator = new TreeGenerator();

            lock (SearcheableChunks)
                SearcheableChunks.Clear();

            lock (Chunks){
				for(int i = Chunks.Count-1; i > -1; i--){
					Chunk chunk = Chunks[i];
					Chunks.RemoveAt(i);
					chunk.Dispose();
				}
			}

		    for (int i = GlobalColliders.Count - 1; i > -1; i--)
		    {
		        RemoveGlobalCollider(GlobalColliders[i]);
		    }

            for (int i = Structures.Count-1; i > -1; i--){
				RemoveStructure(Structures[i]);
			}

			for(int i = Entities.Count-1; i > -1; i--){
				if(Entities[i] is LocalPlayer) continue;
				RemoveEntity(Entities[i]);
			}		

			WorldRenderer.ForceDiscard();
		    CacheManager.Discard();

		    World.AddEntity(SceneManager.Game.LPlayer);

            if (Seed == MenuSeed)
			{
			    MenuBackground.Setup();
			}
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
								if( shapes[j] is CollisionShape && ( ((CollisionShape) shapes[j]).Center.Xz - Position.Xz).LengthSquared <= Radius*Radius)
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

        public static void AddChunkToQueue(Chunk C, bool DoMesh){
			//Queue.Add(C, DoMesh);
			if(!DoMesh)
				ChunkGenerationQueue.Queue.Add(C);
			else
				MeshQueue.Add(C);
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

        public static void AddChunk(Chunk C){
			
			lock(Chunks){
				Chunks.Add(C);
			}
			lock(SearcheableChunks){
				if(!SearcheableChunks.ContainsKey(new Vector2(C.OffsetX, C.OffsetZ)))
					SearcheableChunks.Add(new Vector2(C.OffsetX, C.OffsetZ), C);
			}
		}
		
		public static void RemoveChunk(Chunk Chunk){
			if(Chunk == null) return;
			
			lock(Chunks)
				Chunks.Remove(Chunk);
			lock(SearcheableChunks)
				SearcheableChunks.Remove(new Vector2(Chunk.OffsetX, Chunk.OffsetZ));
			
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
					
				if(Entities[i].BlockPosition.X < Chunk.OffsetX + Chunk.ChunkWidth && Entities[i].BlockPosition.X > Chunk.OffsetX &&
					Entities[i].BlockPosition.Z < Chunk.OffsetZ + Chunk.ChunkWidth && Entities[i].BlockPosition.Z > Chunk.OffsetZ) {
				    if (Entities[i].Removable && !Entities[i].IsBoss && Entities[i].MobType != MobType.Human &&
				        !(Entities[i] is LocalPlayer))
				    {

				        Entities[i].Dispose();
				    }
				}
			}
			
			for(int i = Items.Count-1; i > -1; i--){
				if(Items[i] == null){
					Items.RemoveAt(i);
					continue;
				}
				
				if(Items[i].Position.X < Chunk.OffsetX + Chunk.ChunkWidth && Items[i].Position.X > Chunk.OffsetX &&
				   Items[i].Position.Z < Chunk.OffsetZ + Chunk.ChunkWidth && Items[i].Position.Z > Chunk.OffsetZ){
					Items[i].Dispose();
				}
			}
			ChunkGenerationQueue.Queue.Remove(Chunk);
			MeshQueue.Queue.Remove(Chunk);
			Chunk.Dispose();
		}

        public static Chunk GetChunkByOffset(int OffsetX, int OffsetZ){
			lock(SearcheableChunks)
			{
			    return SearcheableChunks.ContainsKey(new Vector2(OffsetX, OffsetZ)) ? SearcheableChunks[new Vector2(OffsetX, OffsetZ)] : null;
			}
		}

        #region Space Helpers

	    public static bool IsChunkOffset(Vector2 Offset)
	    {
	        return Offset.X % Chunk.ChunkWidth == 0 && Offset.Y % Chunk.ChunkWidth == 0;
	    }

        public static Vector3 ToBlockSpace(Vector3 Vec3){
		
			int ChunkX = (int) Vec3.X / Chunk.ChunkWidth;
			int ChunkZ = (int) Vec3.Z / Chunk.ChunkWidth;
			
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;
			
			var X = (int) Math.Floor( (Vec3.X - ChunkX) / Chunk.BlockSize );
			var Z = (int) Math.Floor( (Vec3.Z - ChunkZ) / Chunk.BlockSize );
			
			return new Vector3(X, Math.Min(Vec3.Y / Chunk.BlockSize, Chunk.ChunkHeight-1) ,Z);
		}
		
		public static Chunk GetChunkAt(Vector3 Vec3){
			int ChunkX = (int) Vec3.X / Chunk.ChunkWidth;
			int ChunkZ = (int) Vec3.Z / Chunk.ChunkWidth;
			
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;
			
			return GetChunkByOffset(ChunkX, ChunkZ);
		}
		
		public static Vector2 ToChunkSpace(Vector3 Vec3){
			int ChunkX = (int) Vec3.X / Chunk.ChunkWidth;
			int ChunkZ = (int) Vec3.Z / Chunk.ChunkWidth;

			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;	
			
			return new Vector2(ChunkX, ChunkZ);
		}

	    public static Block GetBlockAt(int X, int Y, int Z)
	    {
	        return GetBlockAt(new Vector3(X, Y, Z));
	    }

        public static Block GetBlockAt(Vector3 Vec3){
			int ChunkX = (int) Vec3.X / Chunk.ChunkWidth;
			int ChunkZ = (int) Vec3.Z / Chunk.ChunkWidth;
									
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;

			int X = (int) Math.Floor( (Vec3.X - ChunkX) / Chunk.BlockSize );
			int Z = (int)  Math.Floor( (Vec3.Z - ChunkZ) / Chunk.BlockSize );	
			
			Chunk BlockChunk = GetChunkByOffset(ChunkX, ChunkZ);
			if(BlockChunk != null){
				return BlockChunk.GetBlockAt(X, (int) Vec3.Y, Z);
			}
		    return new Block();			
		}

	    public static Block GetHighestBlockAt(float x, float z)
	    {
	        return GetHighestBlockAt( (int) x, (int) z);
	    }

	    public static Block GetHighestBlockAt(int x, int z){
			int ChunkX = (int) x / Chunk.ChunkWidth;
			int ChunkZ = (int) z / Chunk.ChunkWidth;
									
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;

			int X = (int) Math.Floor( (x - ChunkX) / Chunk.BlockSize );
			int Z = (int) Math.Floor( (z - ChunkZ) / Chunk.BlockSize );	
			
			Chunk BlockChunk = GetChunkByOffset(ChunkX, ChunkZ);
			if(BlockChunk != null){
				return BlockChunk.GetHighestBlockAt(X,Z);
			}
			return new Block();	
		}
		public static int GetHighestY(int x, int z){
			int ChunkX = (int) x / Chunk.ChunkWidth;
			int ChunkZ = (int) z / Chunk.ChunkWidth;
									
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;

			int X = (int) Math.Floor( (x - ChunkX) / Chunk.BlockSize );
			int Z = (int) Math.Floor( (z - ChunkZ) / Chunk.BlockSize );	
			
			Chunk BlockChunk = GetChunkByOffset(ChunkX, ChunkZ);
			if(BlockChunk != null)
				return BlockChunk.GetHighestY(X,Z);
			else
				return 0;	
		}
		
		public static Block GetNearestBlockAt(int x, int y, int z){
			int ChunkX = (int) x / Chunk.ChunkWidth;
			int ChunkZ = (int) z / Chunk.ChunkWidth;
									
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;

			int X = (int) Math.Floor( (x - ChunkX) / Chunk.BlockSize );
			int Z = (int) Math.Floor( (z - ChunkZ) / Chunk.BlockSize );	
			
			Chunk BlockChunk = GetChunkByOffset(ChunkX, ChunkZ);
			if(BlockChunk != null){
				return BlockChunk.GetNearestBlockAt(X, y, Z);
			}
			else
				return new Block();	
		}
		
		public static int GetNearestY(int x, int y, int z){
			int ChunkX = (int) x / Chunk.ChunkWidth;
			int ChunkZ = (int) z / Chunk.ChunkWidth;
									
			ChunkX *= Chunk.ChunkWidth;
			ChunkZ *= Chunk.ChunkWidth;

			int X = (int) Math.Floor( (x - ChunkX) / Chunk.BlockSize );
			int Z = (int) Math.Floor( (z - ChunkZ) / Chunk.BlockSize );	
			
			Chunk BlockChunk = GetChunkByOffset(ChunkX, ChunkZ);
			if(BlockChunk != null)
				return BlockChunk.GetNearestY(X, y, Z);
			else
				return 0;	
		}
		
		public static int GetLowestY(int x, int z){
			int chunkX = (int) x / Chunk.ChunkWidth;
			int chunkZ = (int) z / Chunk.ChunkWidth;
									
			chunkX *= Chunk.ChunkWidth;
			chunkZ *= Chunk.ChunkWidth;

			var X = (int) Math.Floor( (x - chunkX) / Chunk.BlockSize );
			var Z = (int) Math.Floor( (z - chunkZ) / Chunk.BlockSize );	
			
			Chunk blockChunk = GetChunkByOffset(chunkX, chunkZ);
			return blockChunk?.GetLowestY(X,Z) ?? 0;	
		}

	    public static Block GetLowestBlock(int x, int z)
	    {
	        int chunkX = (int)x / Chunk.ChunkWidth;
	        int chunkZ = (int)z / Chunk.ChunkWidth;

	        chunkX *= Chunk.ChunkWidth;
	        chunkZ *= Chunk.ChunkWidth;

	        var X = (int)Math.Floor((x - chunkX) / Chunk.BlockSize);
	        var Z = (int)Math.Floor((z - chunkZ) / Chunk.BlockSize);

	        Chunk blockChunk = GetChunkByOffset(chunkX, chunkZ);
	        return blockChunk?.GetLowestBlockAt(X, Z) ?? new Block();
	    }
        #endregion

        public static void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
	    {
	        Highlighter.HighlightArea(Position, Color, Radius, Seconds);
	    }

        public static ItemModel DropItem(InventoryItem Item, Vector3 Position){
			var model = new ItemModel(Item, Position);
			Items.Add(model);
			
			model.OnPickup += delegate(LocalPlayer Player) {
				Player.Inventory.AddItem(model.Item);
				Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, model.Position, false, 1f, 1.2f);
				model.Dispose();
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
	        Box box = Mob.DefaultBox.Cache.Translate(position.Xz.ToVector3()
                + Vector3.UnitY * Physics.HeightAtPosition(position.X, position.Z));
	        while (underChunk != null && collidesOnSurface)
	        {
	            collidesOnSurface = underChunk.CollisionShapes.Any(Shape => Physics.Collides(Shape, box));
	            if (collidesOnSurface)
	            {
	                position = position + new Vector3(Utils.Rng.NextFloat() * 32f - 16f, 0, Utils.Rng.NextFloat() * 32f - 16f);
	                underChunk = World.GetChunkAt(position);
	                box = Mob.DefaultBox.Cache.Translate(position.Xz.ToVector3() 
                        + Vector3.UnitY * Physics.HeightAtPosition(position.X, position.Z));
	            }

	        }
	        return position;
	    }
	}
}
