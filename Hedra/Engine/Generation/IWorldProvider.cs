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
using Hedra.Engine.Rendering;
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

        IBiomePool BiomePool { get; }

        MobFactory MobFactory { get; }

        TreeGenerator TreeGenerator { get; }

        IWorldBuilding WorldBuilding { get; }

        StructureHandler StructureHandler { get; }
        
        int AverageBuildTime { get; }
        
        int AverageGenerationTime { get; }

        int Seed { get; }

        bool IsGenerated { get; }

        int MeshQueueCount { get; }

        int ChunkQueueCount { get; }

        WorldItem[] Items { get; }
        
        ReadOnlyCollection<Chunk> Chunks { get; }

        ReadOnlyCollection<IEntity> Entities { get; }

        ReadOnlyCollection<ICollidable> GlobalColliders { get; }

        Dictionary<Vector2, Chunk> DrawingChunks { get; }
        
        Dictionary<Vector2, Chunk> ShadowDrawingChunks { get; }

        void Load();

        void ReloadModules();

        int MenuSeed { get; }

        int RandomSeed { get; }

        void CullTest(FrustumCulling FrustumObject);

        void Draw(WorldRenderType Type);

        void Update();

        void Recreate(int NewSeed);

        void Discard();

        T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable;

        void AddChunkToQueue(Chunk Chunk, bool DoMesh);

        void AddEntity(IEntity Entity);

        void RemoveEntity(IEntity Entity);

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

        HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds);

        WorldItem DropItem(Item ItemSpec, Vector3 Position);

        Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed);

        Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition);

        Vector3 FindSpawningPoint(Vector3 Around);

        float GetHighest(int X, int Z);

        void SetupStructure(CollidableStructure Structure);
    }
}