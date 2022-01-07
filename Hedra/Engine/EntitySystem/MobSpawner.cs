/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/05/2016
 * Time: 11:05 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    ///     Description of EntitySpawner.
    /// </summary>
    public class MobSpawner
    {
        public static int MobCap = int.MaxValue;
        private const float MiniBossSpawnChance = 1 / 10f;
        private readonly IPlayer _player;
        private readonly Random _rng;
        private readonly AutoResetEvent _waitHandle;

        public MobSpawner(IPlayer Player)
        {
            _player = Player;
            _rng = new Random();
        }

        public float SpawnChance { get; set; } = .8f;
        public bool Enabled { get; set; }

        public virtual void Update()
        {
#if !DEBUG
            if(MobCap == 0)
            {
                throw new ArgumentException("Mob cap cannot be 0");
            }
#endif

            if (!Enabled || GameSettings.Paused || GameManager.InStartMenu) return;

            if (World.Entities.Count >= MobCap || !(_rng.NextFloat() <= SpawnChance)) return;
            var desiredPosition = PlacementPosition();
            var underBlock = World.GetHighestBlockAt((int)desiredPosition.X, (int)desiredPosition.Z);
            if (underBlock.Type == BlockType.Air || underBlock.Type == BlockType.Water ||
                underBlock.Type == BlockType.Seafloor) return;

            var y = World.GetHighestY((int)desiredPosition.X, (int)desiredPosition.Z);
            if (y <= 0) return;

            if ((_player.Position.Xz() - desiredPosition.Xz()).LengthSquared() <
                32f * Chunk.BlockSize * 32f * Chunk.BlockSize)
                return;

            if (Conflicts(desiredPosition)) return;
            if (ShouldSpawnMob(desiredPosition))
            {
                var newPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition),
                    desiredPosition.Z);
                var template = SelectMobTemplate(newPosition);
                if (template == null) return;

                var count = _rng.Next(template.MinGroup, template.MaxGroup + 1);

                void DoSpawn()
                {
                    for (var i = 0; i < count; i++)
                    {
                        var offset = new Vector3(_rng.NextFloat() * 8f - 4f, 0, _rng.NextFloat() * 8f - 4f) *
                                     Chunk.BlockSize;
                        var newNearPosition = new Vector3(newPosition.X + offset.X,
                            Physics.HeightAtPosition(newPosition + offset),
                            newPosition.Z + offset.Z);
                        var mob = World.SpawnMob(template.Type, newNearPosition, _rng);
                        mob.Removable = true;

                        // Log.WriteLine($"Spawned '{template.Type}' at '{newNearPosition}', '{((World.GetChunkAt(newPosition)?.Landscape.FullyGenerated ?? false) ? "EXISTS" : "NOT EXISTS")}'", LogType.WorldBuilding);
                    }
                }

                if (GameSettings.TestingMode) DoSpawn();
                else TaskScheduler.Parallel(DoSpawn);

                Log.WriteLine($"Spawned {count} '{template.Type}' at '{newPosition}'", LogType.WorldBuilding);
            }
            else
            {
                void DoSpawnMiniBoss()
                {
                    SelectAndSpawnMiniBoss(desiredPosition, _rng);
                }

                if (GameSettings.TestingMode) DoSpawnMiniBoss();
                else TaskScheduler.Parallel(DoSpawnMiniBoss);
            }
        }

        private void SelectAndSpawnMiniBoss(Vector3 DesiredPosition, Random Rng)
        {
            var region = World.BiomePool.GetRegion(DesiredPosition);
            var templates = region.Mob.SpawnerSettings.MiniBosses;

            var template = SelectWeightedTemplate(templates);

            if (template == default(MiniBossTemplate))
                throw new ArgumentOutOfRangeException("Failed to select a mini boss template");
            SpawnMiniBoss(template, DesiredPosition, _rng);
        }

        private void SpawnMiniBoss(MiniBossTemplate Template, Vector3 Position, Random Rng)
        {
            var dict = new Dictionary<string, Action>
            {
                { "Explorers", () => TravellingExplorers.Build(Position, _rng) },
                {
                    "AbandonedExplorerWithQuest",
                    () => TravellingExplorers.BuildAbandonedExplorerWithQuest(Position, _rng)
                }
            };
            if (Template.IsCustom)
            {
                if (!dict.ContainsKey(Template.Type))
                    throw new ArgumentOutOfRangeException($"Custom template type '{Template.Type}' does not exist.");
                dict[Template.Type]();
            }
            else
            {
                World.SpawnMob(Template.Type, Position, Rng);
            }
        }

        private bool ShouldSpawnMob(Vector3 NewPosition)
        {
            var region = World.BiomePool.GetRegion(NewPosition);
            return region.Mob.SpawnerSettings.MiniBosses == null || region.Mob.SpawnerSettings.MiniBosses.Length == 0 || _rng.NextFloat() > MiniBossSpawnChance;
        }

        private static bool IsNearWater(Vector3 Position)
        {
            return World.NearestWaterBlock(Position, 128, out _) < 128;
        }

        protected virtual SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
            var region = World.BiomePool.GetRegion(NewPosition);
            var height = NewPosition.Y / Chunk.BlockSize;
            var mountain = height > 96;
            var shore = height >= BiomePool.SeaLevel - 1 && height < 24 || IsNearWater(NewPosition);
            var forest = !shore && World.TreeGenerator.SpaceNoise(NewPosition.X, NewPosition.Z) > 0;
            var plains = !forest && !shore && !mountain;

            var count = (plains ? 1 : 0) + (forest ? 1 : 0) + (shore ? 1 : 0) + (mountain ? 1 : 0);
            var templates = new List<SpawnTemplate>();
            if (forest && region.Mob.SpawnerSettings.Forest != null)
                templates.AddRange(region.Mob.SpawnerSettings.Forest);
            if (mountain && region.Mob.SpawnerSettings.Mountain != null)
                templates.AddRange(region.Mob.SpawnerSettings.Mountain);
            if (plains && region.Mob.SpawnerSettings.Plains != null)
                templates.AddRange(region.Mob.SpawnerSettings.Plains);
            if (shore && region.Mob.SpawnerSettings.Shore != null)
                templates.AddRange(region.Mob.SpawnerSettings.Shore);

            return SelectWeightedTemplate(templates);
        }

        private T SelectWeightedTemplate<T>(IList<T> templates) where T : ISpawnTemplate
        {
            var rng = _rng.NextFloat() * 100f;
            var template = default(T);
            var accum = 0f;
            templates.Shuffle(_rng);
            for (var i = 0; i < templates.Count; ++i)
            {
                var chance = templates[i].Chance;
                if (rng < chance + accum)
                {
                    template = templates[i];
                    break;
                }

                accum += chance;
            }

            return template;
        }

        private bool Conflicts(Vector3 Position)
        {
            try
            {
                for (var i = World.Entities.Count - 1; i > -1; i--)
                {
                    if (World.Entities[i] == _player || World.Entities[i].IsStatic) continue;

                    if ((World.Entities[i].Position.Xz() - Position.Xz()).LengthSquared() <
                        80f * Chunk.BlockSize * 80f * Chunk.BlockSize) return true;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.WriteLine(e.Message);
                return true;
            }

            var items = World.StructureHandler.StructureItems;
            for (var i = 0; i < items.Length; ++i)
                if (items[i].Mountain != null && items[i].Mountain.Collides(Position.Xz()))
                    return true;

            var normal = Physics.NormalAtPosition(Position);
            var river = World.BiomePool.GetRegion(Position).Generation.RiverAtPoint(Position.X, Position.Z);
            return Vector3.Dot(normal, Vector3.UnitY) < .4 || river > 0.005f;
        }

        private Vector3 PlacementPosition()
        {
            return new Vector3(
                       _rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width -
                       GameSettings.ChunkLoaderRadius * Chunk.Width * .5f,
                       0,
                       _rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width -
                       GameSettings.ChunkLoaderRadius * Chunk.Width * .5f)
                   + _player.Position.Xz().ToVector3();
        }

        public void Dispose()
        {
        }
    }

    public enum MobType
    {
        Sheep,
        Pig,
        Boar,
        Bee,
        Horse,
        Wolf,
        Goat,
        GorillaWarrior,
        GiantBeetle,
        Gorilla,
        MeleeBeetle,
        RangedBeetle,
        Troll,
        Pug,
        Cow,
        Bear,
        Lich,
        Crow,
        SkeletonKamikaze,
        SkeletonKing,
        Skeleton,
        Golem,
        TotalCount,
        None,
        Unknown
    }
}