/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 04:16 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hedra.BiomeSystem;
using Hedra.Engine;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using OpenTK;

namespace Hedra
{
    public delegate void ModulesReloadEvent(string AppPath);

    public static class World
    {
        public static event ModulesReloadEvent ModulesReload;
        public static event OnChunkEvent OnChunkReady;
        public static event OnChunkEvent OnChunkDisposed;
        public static Dictionary<Vector2, Chunk> SearcheableChunksReference;
        public static Dictionary<Vector2, Chunk> SearcheableChunks => Provider.SearcheableChunks;
        public static AreaHighlighter Highlighter => Provider.Highlighter;
        public static ParticleSystem Particles => Provider.Particles;
        public static EnvironmentGenerator EnvironmentGenerator => Provider.EnvironmentGenerator;
        public static IBiomePool BiomePool => Provider.BiomePool;
        public static MobFactory MobFactory => Provider.MobFactory;
        public static TreeGenerator TreeGenerator => Provider.TreeGenerator;
        public static IWorldBuilding WorldBuilding => Provider.WorldBuilding;
        public static StructureHandler StructureHandler => Provider.StructureHandler;
        public static bool IsGenerated => Provider.IsGenerated;
        public static int MeshQueueCount => Provider.MeshQueueCount;
        public static int ChunkQueueCount => Provider.ChunkQueueCount;
        public static IWorldObject[] WorldObjects => Provider.WorldObjects;
        public static ReadOnlyCollection<Chunk> Chunks => Provider.Chunks;
        public static ReadOnlyCollection<IEntity> Entities => Provider.Entities;
        public static Dictionary<Vector2, Chunk> DrawingChunks => Provider.DrawingChunks;
        public static Dictionary<Vector2, Chunk> ShadowDrawingChunks => Provider.ShadowDrawingChunks;
        public static int AverageBuildTime => Provider.AverageBuildTime;
        public static int AverageGenerationTime => Provider.AverageGenerationTime;
        public static FishingZoneHandler FishingZoneHandler => Provider.FishingZoneHandler;
        public static Vector3 SpawnPoint => Provider.SpawnPoint;
        public static Vector3 SpawnVillagePoint => Provider.SpawnVillagePoint;
        public static int Seed => Provider.Seed;
        public static int MenuSeed => Provider.MenuSeed;
        public static int RandomSeed => Provider.RandomSeed;

        public static IWorldProvider Provider { get; set; }        

        public static void Load()
        {
            Provider = new WorldProvider();
            Provider.Load();
            Provider.ModulesReload += Path => ModulesReload?.Invoke(Path);
            SearcheableChunksReference = Provider.SearcheableChunks;
        }

        public static void ReloadModules()
        {
            Provider.ReloadModules();
        }

        public static void OccludeTest()
        {
            Provider.OccludeTest();
        }
        
        public static void CullTest()
        {
            Provider.CullTest();
        }

        public static void Draw(WorldRenderType Type)
        {
            Provider.Draw(Type);
        }

        public static void Update()
        {
            Provider.Update();
        }

        public static void Recreate(int NewSeed, WorldType Type = WorldType.Overworld)
        {
            Provider.Recreate(NewSeed, Type);
        }

        public static void Discard()
        {
            Provider.Discard();
        }

        public static T[] InRadius<T>(Vector3 Position, float Radius) where T : ISearchable
        {
            return Provider.InRadius<T>(Position, Radius);
        }

        public static void AddChunkToQueue(Chunk Chunk, bool DoMesh)
        {
            Provider.AddChunkToQueue(Chunk, DoMesh);
        }

        public static Chunk GetChunkByOffset(Vector2 vec2)
        {
            return GetChunkByOffset((int) vec2.X, (int) vec2.Y);
        }

        public static void AddEntity(IEntity Entity)
        {
            Provider.AddEntity(Entity);
        }

        public static void RemoveEntity(IEntity Entity)
        {
            Provider.RemoveEntity(Entity);
        }

        public static void RemoveWorldObject(IWorldObject Object)
        {
            Provider.RemoveObject(Object);
        }

        public static void AddChunk(Chunk Chunk)
        {
            Provider.AddChunk(Chunk);
        }

        public static void RemoveChunk(Chunk Chunk)
        {
            Provider.RemoveChunk(Chunk);
            OnChunkDisposed?.Invoke(Chunk);
        }

        public static Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            return Provider.GetChunkByOffset(OffsetX, OffsetZ);
        }

        public static bool IsChunkOffset(Vector2 Offset)
        {
            return Provider.IsChunkOffset(Offset);
        }

        public static Vector3 ToBlockSpace(Vector3 Vec3)
        {
            return Provider.ToBlockSpace(Vec3);
        }

        public static Vector2 ToChunkSpace(Vector3 Vec3)
        {
            return Provider.ToChunkSpace(Vec3);
        }
        public static Vector2 ToChunkSpace(Vector2 Vec2)
        {
            return Provider.ToChunkSpace(Vec2.ToVector3());
        }

        public static Chunk GetChunkAt(Vector3 Coordinates)
        {
            return Provider.GetChunkAt(Coordinates);
        }

        public static Block GetBlockAt(Vector3 Vec3)
        {
            return Provider.GetBlockAt(Vec3);
        }

        public static Block GetHighestBlockAt(float X, float Z)
        {
            return GetHighestBlockAt((int) X, (int) Z);
        }
        
        public static float GetHighest(int X, int Z)
        {
            return Provider.GetHighest(X, Z);
        }

        public static Block GetHighestBlockAt(int X, int Z)
        {
            return Provider.GetHighestBlockAt(X, Z);
        }

        public static int GetHighestY(int X, int Z)
        {
            return Provider.GetHighestY(X, Z);
        }

        public static Block GetNearestBlockAt(int X, int Y, int Z)
        {
            return Provider.GetNearestBlockAt(X, Y, Z);
        }

        public static int GetNearestY(int X, int Y, int Z)
        {
            return Provider.GetNearestY(X, Y, Z);
        }

        public static int GetLowestY(int X, int Z)
        {
            return Provider.GetLowestY(X, Z);
        }

        public static HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            return Provider.HighlightArea(Position, Color, Radius, Seconds);
        }

        public static WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            return Provider.DropItem(ItemSpec, Position);
        }

        public static Entity SpawnMob(MobType Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return SpawnMob(Type.ToString(), DesiredPosition, SeedRng);
        }

        public static Entity SpawnMob(string Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return SpawnMob(Type, DesiredPosition, SeedRng.Next(ushort.MinValue, ushort.MaxValue));
        }

        public static SkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            return Provider.SpawnMob(Type, DesiredPosition, MobSeed);
        }

        public static Vector3 FindPlaceablePosition(IEntity Mob, Vector3 DesiredPosition)
        {
            return Provider.FindPlaceablePosition(Mob, DesiredPosition);
        }

        public static void MarkChunkReady(Chunk Object)
        {
            OnChunkReady?.Invoke(Object);
        }

        public static Chest SpawnChest(Vector3 Position, Item Specification)
        {
            return WorldBuilding.SpawnChest(Position, Specification);
        }

        public static Vector3 FindSpawningPoint(Vector3 Around)
        {
            return Provider.FindSpawningPoint(Around);
        }

        public static void AddWorldObject(IWorldObject WorldObject)
        {
            Provider.AddWorldObject(WorldObject);
        }

        public static Region GetRegion(Vector3 Position)
        {
            return BiomePool.GetRegion(Position);
        }

        public static float NearestWaterBlock(Vector3 Position, float SearchRange, out Vector3 WaterPosition)
        {
            return Provider.NearestWaterBlock(Position, SearchRange, out WaterPosition);
        }

        public static float NearestWaterBlockOnChunk(Chunk Chunk, Vector3 Position, out Vector3 WaterPosition)
        {
            return Provider.NearestWaterBlockOnChunk(Chunk, Position, out WaterPosition);
        }
        public static float NearestWaterBlockOnChunk(Vector3 Position, out Vector3 WaterPosition)
        {
            return Provider.NearestWaterBlockOnChunk(Position, out WaterPosition);
        }
    }
}