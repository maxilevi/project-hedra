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
    public interface IWorldProvider
    {
        event ModulesReloadEvent ModulesReload;

        Dictionary<Vector2, Chunk> SearcheableChunks { get; }

        AreaHighlighter Highlighter { get; }

        ParticleSystem Particles { get; }

        EnviromentGenerator EnviromentGenerator { get; }

        BiomePool BiomePool { get; }

        MobFactory MobFactory { get; }

        TreeGenerator TreeGenerator { get; }

        WorldBuilding.WorldBuilding WorldBuilding { get; }

        StructureGenerator StructureGenerator { get; }

        int Seed { get; }

        bool IsGenerated { get; }

        int MeshQueueCount { get; }

        int ChunkQueueCount { get; }

        ReadOnlyCollection<Chunk> Chunks { get; }

        ReadOnlyCollection<WorldItem> Items { get; }

        ReadOnlyCollection<Entity> Entities { get; }

        ReadOnlyCollection<BaseStructure> Structures { get; }

        ReadOnlyCollection<ICollidable> GlobalColliders { get; }

        Dictionary<Vector2, Chunk> DrawingChunks { get; }

        void Load();

        void ReloadModules();

        int MenuSeed { get; }

        int RandomSeed { get; }

        void CullTest(FrustumCulling FrustumObject);

        void Draw(ChunkBufferTypes Type);

        void Update();

        void Recreate(int NewSeed);

        void Discard();

        void RemoveInstances(Vector3 Position, int Radius);

        T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable;

        void AddChunkToQueue(Chunk Chunk, bool DoMesh);

        Chunk GetChunkByOffset(Vector2 Vec2);

        void AddEntity(Entity Entity);

        void RemoveEntity(Entity Entity);

        void AddStructure(BaseStructure Struct);

        void RemoveStructure(BaseStructure Struct);

        void AddGlobalCollider(params ICollidable[] Collidable);

        void RemoveGlobalCollider(ICollidable Collidable);

        void AddItem(WorldItem Item);

        void RemoveItem(WorldItem Item);

        void AddChunk(Chunk Chunk);

        void RemoveChunk(Chunk Chunk);

        Chunk GetChunkByOffset(int OffsetX, int OffsetZ);

        bool IsChunkOffset(Vector2 Offset);

        Vector3 ToBlockSpace(Vector3 Vec3);

        Vector2 ToChunkSpace(Vector3 Vec3);

        Chunk GetChunkAt(Vector3 Coordinates);

        Block GetBlockAt(Vector3 Vec3);

        Block GetHighestBlockAt(int X, int Z);

        int GetHighestY(int X, int Z);

        Block GetNearestBlockAt(int X, int Y, int Z);

        int GetNearestY(int X, int Y, int Z);

        int GetLowestY(int X, int Z);

        Block GetLowestBlock(int X, int Z);

        void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds);

        WorldItem DropItem(Item ItemSpec, Vector3 Position);

        Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed);

        Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition);
    }
}