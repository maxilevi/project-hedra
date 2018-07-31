/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 04:16 p.m.
 *
 */

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
    public delegate void ModulesReloadEvent(string AppPath);

    public static class World
    {
        public static event ModulesReloadEvent ModulesReload;
        
        public static Dictionary<Vector2, Chunk> SearcheableChunks => Provider.SearcheableChunks;
        public static AreaHighlighter Highlighter => Provider.Highlighter;
        public static ParticleSystem Particles => Provider.Particles;
        public static EnviromentGenerator EnviromentGenerator => Provider.EnviromentGenerator;
        public static BiomePool BiomePool => Provider.BiomePool;
        public static MobFactory MobFactory => Provider.MobFactory;
        public static TreeGenerator TreeGenerator => Provider.TreeGenerator;
        public static WorldBuilding.WorldBuilding WorldBuilding => Provider.WorldBuilding;
        public static StructureGenerator StructureGenerator => Provider.StructureGenerator;
        public static bool IsGenerated => Provider.IsGenerated;
        public static int MeshQueueCount => Provider.MeshQueueCount;
        public static int ChunkQueueCount => Provider.ChunkQueueCount;
        public static ReadOnlyCollection<Chunk> Chunks => Provider.Chunks;
        public static ReadOnlyCollection<WorldItem> Items => Provider.Items;
        public static ReadOnlyCollection<Entity> Entities => Provider.Entities;
        public static ReadOnlyCollection<BaseStructure> Structures => Provider.Structures;
        public static ReadOnlyCollection<ICollidable> GlobalColliders => Provider.GlobalColliders;
        public static Dictionary<Vector2, Chunk> DrawingChunks => Provider.DrawingChunks;

        public static int Seed => Provider.Seed;
        public static int MenuSeed => Provider.MenuSeed;
        public static int RandomSeed => Provider.RandomSeed;

        public static IWorldProvider Provider { get; set; }
        

        public static void Load()
        {
            Provider = new WorldProvider();
            Provider.Load();
            Provider.ModulesReload += Path => ModulesReload?.Invoke(Path);
        }

        public static void ReloadModules()
        {
            Provider.ReloadModules();
        }

        public static void CullTest(FrustumCulling FrustumObject)
        {
            Provider.CullTest(FrustumObject);
        }

        public static void Draw(ChunkBufferTypes Type)
        {
            Provider.Draw(Type);
        }

        public static void Update()
        {
            Provider.Update();
        }

        public static void Recreate(int Seed)
        {
            Provider.Recreate(Seed);
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

        public static void AddEntity(Entity Entity)
        {
            Provider.AddEntity(Entity);
        }

        public static void RemoveEntity(Entity Entity)
        {
            Provider.RemoveEntity(Entity);
        }

        public static void AddStructure(BaseStructure Struct)
        {
            Provider.AddStructure(Struct);
        }

        public static void RemoveStructure(BaseStructure Struct)
        {
            Provider.RemoveStructure(Struct);
        }

        public static void AddGlobalCollider(params ICollidable[] Collidable)
        {
            Provider.AddGlobalCollider(Collidable);
        }

        public static void RemoveGlobalCollider(ICollidable Collidable)
        {
            Provider.RemoveGlobalCollider(Collidable);
        }

        public static void AddItem(WorldItem Item)
        {
            Provider.AddItem(Item);
        }

        public static void RemoveItem(WorldItem Item)
        {
            Provider.RemoveItem(Item);
        }

        public static void AddChunk(Chunk Chunk)
        {
            Provider.AddChunk(Chunk);
        }

        public static void RemoveChunk(Chunk Chunk)
        {
            Provider.RemoveChunk(Chunk);
        }

        public static Chunk GetChunkByOffset(int OffsetX, int OffsetZ)
        {
            return Provider.GetChunkByOffset(OffsetX, OffsetZ);
        }

        public static bool IsChunkOffset(Vector2 Offset)
        {
            return Provider.IsChunkOffset(Offset);
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
            return Provider.ToBlockSpace(Vec3);
        }

        public static Vector2 ToChunkSpace(Vector3 Vec3)
        {
            return Provider.ToChunkSpace(Vec3);
        }

        public static Chunk GetChunkAt(Vector3 Coordinates)
        {
            return Provider.GetChunkAt(Coordinates);
        }

        public static Block GetBlockAt(int X, int Y, int Z)
        {
            return GetBlockAt(new Vector3(X, Y, Z));
        }

        public static Block GetBlockAt(Vector3 Vec3)
        {
            return Provider.GetBlockAt(Vec3);
        }

        public static Block GetHighestBlockAt(float X, float Z)
        {
            return GetHighestBlockAt((int) X, (int) Z);
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

        public static Block GetLowestBlock(int X, int Z)
        {
            return Provider.GetLowestBlock(X, Z);
        }

        public static void HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            Provider.HighlightArea(Position, Color, Radius, Seconds);
        }

        public static WorldItem DropItem(Item ItemSpec, Vector3 Position)
        {
            return Provider.DropItem(ItemSpec, Position);
        }

        public static Entity SpawnMob(MobType Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return Provider.SpawnMob(Type, DesiredPosition, SeedRng);
        }

        public static Entity SpawnMob(MobType Type, Vector3 DesiredPosition, int MobSeed)
        {
            return Provider.SpawnMob(Type, DesiredPosition, MobSeed);
        }

        public static Entity SpawnMob(string Type, Vector3 DesiredPosition, Random SeedRng)
        {
            return Provider.SpawnMob(Type, DesiredPosition, SeedRng);
        }

        public static Entity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            return Provider.SpawnMob(Type, DesiredPosition, MobSeed);
        }

        public static Vector3 FindPlaceablePosition(Entity Mob, Vector3 DesiredPosition)
        {
            return Provider.FindPlaceablePosition(Mob, DesiredPosition);
        }
    }
}