using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class DummyProvider : IWorldProvider
    {
        public event ModulesReloadEvent ModulesReload;
        public Dictionary<Vector2, Chunk> SearcheableChunks => null;
        public AreaHighlighter Highlighter => null;
        public ParticleSystem Particles => null;
        public EnviromentGenerator EnviromentGenerator => null;
        public BiomePool BiomePool => null;
        public MobFactory MobFactory => null;
        public TreeGenerator TreeGenerator => null;
        public WorldBuilding.WorldBuilding WorldBuilding => null;
        public StructureGenerator StructureGenerator => null;
        public int Seed => 0;
        public bool IsGenerated => false;
        public int MeshQueueCount => 0;
        public int ChunkQueueCount => 0;
        public ReadOnlyCollection<Chunk> Chunks => null;
        public ReadOnlyCollection<WorldItem> Items => null;
        public ReadOnlyCollection<Entity> Entities => null;
        public ReadOnlyCollection<BaseStructure> Structures => null;
        public ReadOnlyCollection<ICollidable> GlobalColliders => null;
        public Dictionary<Vector2, Chunk> DrawingChunks => null;
        public int MenuSeed => 0;
        public int RandomSeed => 0;
        
        public void Load()
        {
        }

        public void ReloadModules()
        {
        }

        public void CullTest(FrustumCulling FrustumObject)
        {
        }

        public void Draw(ChunkBufferTypes Type)
        {
        }

        public void Update()
        {
        }

        public void Recreate(int NewSeed)
        {
        }

        public void Discard()
        {
        }

        public void RemoveInstances(Vector3 Position, int Radius)
        {
        }

        public T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            return default(T[]);
        }

        public void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
        }

        public Chunk GetChunkByOffset(Vector2 vec2)
        {
            return default(Chunk);
        }

        public void AddEntity(Entity Entity)
        {
        }

        public void RemoveEntity(Entity Entity)
        {
        }

        public void AddStructure(BaseStructure Struct)
        {
        }

        public void RemoveStructure(BaseStructure Struct)
        {
        }

        public void AddGlobalCollider(params ICollidable[] Collidable)
        {
        }

        public void RemoveGlobalCollider(ICollidable Collidable)
        {
        }

        public void AddItem(WorldItem Item)
        {
        }

        public void RemoveItem(WorldItem Item)
        {
        }

        public void AddChunk(Chunk Chunk)
        {
        }

        public void RemoveChunk(Chunk Chunk)
        {
        }

        public Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            return default(Chunk);
        }

        public bool IsChunkOffset(Vector2 Offset)
        {
            return default(bool);
        }

        public Vector3 ToBlockSpace(Vector3 Vec3)
        {
            return default(Vector3);
        }

        public Vector2 ToChunkSpace(Vector3 Vec3)
        {
            return default(Vector2);
        }

        public Chunk GetChunkAt(Vector3 Coordinates)
        {
            return default(Chunk);
        }

        public Block GetBlockAt(Vector3 Vec3)
        {
            return default(Block);
        }

        public Block GetHighestBlockAt(int X, int Z)
        {
            return default(Block);
        }

        public int GetHighestY(int X, int Z)
        {
            return default(int);
        }

        public Block GetNearestBlockAt(int X, int Y, int Z)
        {
            return default(Block);
        }

        public int GetNearestY(int X, int y, int Z)
        {
            return default(int);
        }

        public int GetLowestY(int X, int Z)
        {
            return default(int);
        }

        public Block GetLowestBlock(int X, int Z)
        {
            return default(Block);
        }

        public void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
        }

        public WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            return default(WorldItem);
        }

        public Entity SpawnMob(MobType Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return default(Entity);
        }

        public Entity SpawnMob(MobType Type, Vector3 DesiredPosition, int MobSeed)
        {
            return default(Entity);
        }

        public Entity SpawnMob(string Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return default(Entity);
        }

        public Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            return default(Entity);
        }

        public Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition)
        {
            return default(Vector3);
        }
    }
}