using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace HedraTests
{
    public class SimpleWorldProviderMock : IWorldProvider
    {
        public virtual event ModulesReloadEvent ModulesReload;
        public virtual Dictionary<Vector2, Chunk> SearcheableChunks => null;
        public virtual AreaHighlighter Highlighter => null;
        public virtual ParticleSystem Particles => null;
        public virtual EnviromentGenerator EnviromentGenerator => null;
        public virtual IBiomePool BiomePool => null;
        public virtual MobFactory MobFactory => null;
        public virtual TreeGenerator TreeGenerator => null;
        public virtual Hedra.Engine.WorldBuilding.WorldBuilding WorldBuilding => null;
        public virtual StructureGenerator StructureGenerator => null;
        public virtual int Seed => 0;
        public virtual bool IsGenerated => false;
        public virtual int MeshQueueCount => 0;
        public virtual int ChunkQueueCount => 0;
        public virtual ReadOnlyCollection<Chunk> Chunks => null;
        public virtual ReadOnlyCollection<WorldItem> Items => null;
        public virtual ReadOnlyCollection<IEntity> Entities => null;
        public virtual ReadOnlyCollection<BaseStructure> Structures => null;
        public virtual ReadOnlyCollection<ICollidable> GlobalColliders => null;
        public virtual Dictionary<Vector2, Chunk> DrawingChunks => null;
        public virtual int MenuSeed => 0;
        public virtual int RandomSeed => 0;
        
        public virtual void Load()
        {
        }

        public virtual void ReloadModules()
        {
        }

        public virtual void CullTest(FrustumCulling FrustumObject)
        {
        }

        public virtual void Draw(WorldRenderType Type)
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Recreate(int NewSeed)
        {
        }

        public virtual void Discard()
        {
        }

        public virtual void RemoveInstances(Vector3 Position, int Radius)
        {
        }

        public virtual T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            return default(T[]);
        }

        public virtual void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
        }

        public virtual Chunk GetChunkByOffset(Vector2 Vec2)
        {
            return default(Chunk);
        }

        public virtual void AddEntity(IEntity Entity)
        {
        }

        public virtual void RemoveEntity(IEntity Entity)
        {
        }

        public virtual void AddStructure(BaseStructure Struct)
        {
        }

        public virtual void RemoveStructure(BaseStructure Struct)
        {
        }

        public virtual void AddGlobalCollider(params ICollidable[] Collidable)
        {
        }

        public virtual void RemoveGlobalCollider(ICollidable Collidable)
        {
        }

        public virtual void AddItem(WorldItem Item)
        {
        }

        public virtual void RemoveItem(WorldItem Item)
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

        public virtual int GetLowestY(int X, int Z)
        {
            return default(int);
        }

        public virtual Block GetLowestBlock(int X, int Z)
        {
            return default(Block);
        }

        public virtual void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
        }

        public virtual WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            return default(WorldItem);
        }

        public virtual Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            return default(Entity);
        }

        public virtual Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition)
        {
            return default(Vector3);
        }
    }
}