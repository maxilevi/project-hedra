using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hedra;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using System.Numerics;
using Hedra.Engine.SkillSystem;

namespace HedraTests
{
    public class SimpleWorldProviderMock : IWorldProvider
    {
        public virtual event ModulesReloadEvent ModulesReload;
        public WorldBuilder Builder { get; }
        public virtual Dictionary<Vector2, Chunk> SearcheableChunks => null;
        public virtual AreaHighlighter Highlighter => null;
        public virtual ParticleSystem Particles => null;
        public virtual EnvironmentGenerator EnvironmentGenerator => null;
        public virtual IBiomePool BiomePool { get; set; }
        public virtual MobFactory MobFactory => new MobFactory();
        public virtual TreeGenerator TreeGenerator => null;
        public virtual IWorldBuilding WorldBuilding { get; set; }
        public virtual StructureHandler StructureHandler { get; } = new StructureHandler();
        public FishingZoneHandler FishingZoneHandler => null;
        public void ResetGenerationProfile()
        {
            throw new NotImplementedException();
        }

        public void ResetMeshProfile()
        {
            throw new NotImplementedException();
        }

        public int AverageBuildTime => 0;
        public int AverageGenerationTime => 0;
        public virtual int Seed => 0;
        public Vector3 SpawnPoint { get; }
        public Vector3 SpawnVillagePoint { get; }
        public virtual bool IsGenerated => false;
        public virtual int MeshQueueCount => 0;
        public virtual int ChunkQueueCount => 0;
        public virtual ReadOnlyCollection<Chunk> Chunks => null;
        public virtual IWorldObject[] WorldObjects => null;
        public virtual ReadOnlyCollection<IEntity> Entities => null;
        public virtual BaseStructure[] Structures => null;
        public virtual Dictionary<Vector2, Chunk> DrawingChunks => null;
        public virtual Dictionary<Vector2, Chunk> ShadowDrawingChunks => null;
        public virtual int MenuSeed => 0;
        public virtual int RandomSeed => 0;
        
        public virtual void Load()
        {
        }

        public virtual void ReloadModules()
        {
        }

        public void CullTest()
        {
            throw new NotImplementedException();
        }

        public void OccludeTest()
        {
            throw new NotImplementedException();
        }

        public virtual void Draw(WorldRenderType Type)
        {
        }

        public virtual void Update()
        {
        }

        public void Recreate(int NewSeed, WorldType Type)
        {
            throw new NotImplementedException();
        }

        public virtual void Recreate(int NewSeed)
        {
        }

        public virtual void Discard()
        {
        }

        public virtual T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            return default(T[]);
        }

        public void AddChunkToQueue(Chunk Chunk, ChunkQueueType Type)
        {
            throw new NotImplementedException();
        }

        public virtual void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
        }

        public virtual void AddEntity(IEntity Entity)
        {
        }

        public virtual void RemoveEntity(IEntity Entity)
        {
        }

        public void RemoveObject(IWorldObject WorldObject)
        {
            
        }

        public virtual void AddChunk(Chunk Chunk)
        {
        }

        public virtual void RemoveChunk(Chunk Chunk)
        {
        }

        public virtual Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            return default(Chunk);
        }

        public virtual bool IsChunkOffset(Vector2 Offset)
        {
            return default(bool);
        }

        public virtual Vector3 ToBlockSpace(Vector3 Vec3)
        {
            return default(Vector3);
        }

        public virtual Vector2 ToChunkSpace(Vector3 Vec3)
        {
            return default(Vector2);
        }

        public virtual Chunk GetChunkAt(Vector3 Coordinates)
        {
            return default(Chunk);
        }

        public virtual Block GetBlockAt(Vector3 Vec3)
        {
            return default(Block);
        }

        public virtual Block GetHighestBlockAt(int X, int Z)
        {
            return default(Block);
        }

        public virtual int GetHighestY(int X, int Z)
        {
            return default(int);
        }

        public virtual Block GetNearestBlockAt(int X, int Y, int Z)
        {
            return default(Block);
        }

        public virtual int GetNearestY(int X, int Y, int Z)
        {
            return default(int);
        }
        
        public virtual float GetHighest(int X, int Z)
        {
            return default(float);
        }

        public void SetupStructure(CollidableStructure Structure)
        {
            throw new NotImplementedException();
        }

        public void AddWorldObject(IWorldObject WorldObject)
        {
        }

        public float NearestWaterBlock(Vector3 Position, float SearchRange, out Vector3 WaterPosition)
        {
            WaterPosition = Vector3.Zero;
            return 0;
        }

        public float NearestWaterBlockOnChunk(Chunk Chunk, Vector3 Position, out Vector3 WaterPosition)
        {
            WaterPosition = Vector3.Zero;
            return 0;
        }

        public float NearestWaterBlockOnChunk(Vector3 Position, out Vector3 WaterPosition)
        {
            WaterPosition = Vector3.Zero;
            return 0;
        }

        public float GetNoise(float X, float Y)
        {
            return 0;
        }

        public float GetNoise(float X, float Y, float Z)
        {
            return 0;
        }

        public float NearestWaterBlock(Vector3 Position, float SearchRange)
        {
            return default(float);
        }

        public float NearestWaterBlockOnChunk(Chunk Chunk, Vector3 Position)
        {
            return default(float);
        }

        public float NearestWaterBlockOnChunk(Vector3 Position)
        {
            return default(float);
        }

        public virtual int GetLowestY(int X, int Z)
        {
            return default(int);
        }

        public virtual Block GetLowestBlock(int X, int Z)
        {
            return default(Block);
        }

        public virtual HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            return default(HighlightedAreaWrapper);
        }

        public virtual WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            return default(WorldItem);
        }

        public virtual ISkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            return default(SkilledAnimableEntity);
        }

        public virtual Vector3 FindPlaceablePosition(IEntity Mob, Vector3 DesiredPosition)
        {
            return default(Vector3);
        }
        
        public virtual Vector3 FindSpawningPoint(Vector3 DesiredPosition)
        {
            return default(Vector3);
        }
    }
}