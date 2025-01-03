using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.Generation
{
    public interface IWorldProvider
    {
        WorldBuilder Builder { get; }

        Dictionary<Vector2, Chunk> SearcheableChunks { get; }

        AreaHighlighter Highlighter { get; }

        ParticleSystem Particles { get; }

        EnvironmentGenerator EnvironmentGenerator { get; }

        IBiomePool BiomePool { get; }

        MobFactory MobFactory { get; }

        TreeGenerator TreeGenerator { get; }

        IWorldBuilding WorldBuilding { get; }

        StructureHandler StructureHandler { get; }
        FishingZoneHandler FishingZoneHandler { get; }

        int Seed { get; }

        Vector3 SpawnPoint { get; }

        Vector3 SpawnVillagePoint { get; }

        bool IsGenerated { get; }

        IWorldObject[] WorldObjects { get; }

        ReadOnlyCollection<Chunk> Chunks { get; }

        ReadOnlyCollection<IEntity> Entities { get; }

        Dictionary<Vector2, Chunk> DrawingChunks { get; }

        Dictionary<Vector2, Chunk> ShadowDrawingChunks { get; }

        int MenuSeed { get; }

        int RandomSeed { get; }
        event ModulesReloadEvent ModulesReload;

        void Load();

        void ReloadModules();

        void CullTest();

        void OccludeTest();

        void Draw(WorldRenderType Type);

        void Update();

        void Recreate(int NewSeed, WorldType Type);

        void Discard();

        T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable;

        void AddChunkToQueue(Chunk Chunk, ChunkQueueType Type);

        void AddEntity(IEntity Entity);

        void RemoveEntity(IEntity Entity);

        void RemoveObject(IWorldObject WorldObject);

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

        HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds);

        WorldItem DropItem(Item ItemSpec, Vector3 Position);

        ISkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed);

        Vector3 FindPlaceablePosition(IEntity Mob, Vector3 DesiredPosition);

        Vector3 FindSpawningPoint(Vector3 Around);

        float GetHighest(int X, int Z);

        void SetupStructure(CollidableStructure Structure);

        void AddWorldObject(IWorldObject WorldObject);

        float NearestWaterBlock(Vector3 Position, float SearchRange, out Vector3 WaterPosition);

        float NearestWaterBlockOnChunk(Chunk Chunk, Vector3 Position, out Vector3 WaterPosition);

        float NearestWaterBlockOnChunk(Vector3 Position, out Vector3 WaterPosition);

        float GetNoise(float X, float Y);

        float GetNoise(float X, float Y, float Z);
    }
}