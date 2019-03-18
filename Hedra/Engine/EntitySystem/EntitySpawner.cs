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
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using OpenTK;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of EntitySpawner.
    /// </summary>
    public class EntitySpawner
    {
        public static int MobCap = int.MaxValue;
        public float SpawnChance { get; set; } = .8f;
        public int MinSpawn { get; set; }= 1;
        public int MaxSpawn { get; set; }= 3;
        private readonly IPlayer _player;
        private readonly Random _rng;
        public bool Enabled { get; set; }
        
        public EntitySpawner(IPlayer Player)
        {
            this._player = Player;
            _rng = new Random();
            var spawnThread = new Thread(delegate()
            {
                while (GameManager.Exists)
                {
                    this.Update();
                    Thread.Sleep(15);
                }
            });
            spawnThread.Start();
        }
        
        public virtual void Update()
        {
            #if !DEBUG
            if(MobCap == 0)
            {
                throw new ArgumentException("Mob cap cannot be 0");
            }
            #endif
            
            if(!Enabled) return;

            if (World.Entities.Count >= MobCap || !(Utils.Rng.NextFloat() <= SpawnChance) ) return;
            var desiredPosition = this.PlacementPosition();
            var underBlock = World.GetHighestBlockAt( (int) desiredPosition.X, (int) desiredPosition.Z);
            if (underBlock.Type == BlockType.Air || underBlock.Type == BlockType.Water ||
                underBlock.Type == BlockType.Seafloor) return;
            
            var y = World.GetHighestY( (int) desiredPosition.X, (int) desiredPosition.Z);
            if (y <= 0) return;

            if ((_player.Position.Xz - desiredPosition.Xz).LengthSquared <
                32f * Chunk.BlockSize * 32f * Chunk.BlockSize)
                return;

            if (this.Conflicts(desiredPosition)) return;

            if (ShouldSpawnMob(desiredPosition))
            {
                var newPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition),
                    desiredPosition.Z);
                var template = this.SelectMobTemplate(newPosition);
                if (template == null) return;

                var count = Utils.Rng.Next(template.MinGroup, template.MaxGroup + 1);

                for (var i = 0; i < count; i++)
                {
                    var offset = new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f) *
                                 Chunk.BlockSize;
                    var newNearPosition = new Vector3(newPosition.X + offset.X,
                        Physics.HeightAtPosition(newPosition + offset),
                        newPosition.Z + offset.Z);
                    World.SpawnMob(template.Type, newNearPosition, Utils.Rng);
                }
            }
            else
            {
                SelectAndSpawnMiniBoss(desiredPosition, Utils.Rng);
            }
        }

        private static void SelectAndSpawnMiniBoss(Vector3 DesiredPosition, Random Rng)
        {
            var region = World.BiomePool.GetRegion(DesiredPosition);
            var templates = region.Mob.SpawnerSettings.MiniBosses.OrderBy(T => T.Chance).ToArray();
            var rng = Rng.NextFloat();
            var sum = Math.Abs(templates.Sum(T => T.Chance) - 100f);
            if(sum > 0.01f)
                throw new ArgumentOutOfRangeException($"MiniBoss templates need to add to 100.0 but only add up to {sum}");
            
            var template = default(MiniBossTemplate);
            var accum = 0f;
            for (var i = 0; i < templates.Length; ++i)
            {
                var chance = templates[i].Chance / 100f;
                if (rng < chance + accum)
                {
                    template = templates[i];
                    break;
                }
                accum += chance;
            }
            if(template == default(MiniBossTemplate))
                throw new ArgumentOutOfRangeException("Failed to select a mini boss template");
            SpawnMiniBoss(template, DesiredPosition, Utils.Rng);
        }
        
        private static void SpawnMiniBoss(MiniBossTemplate Template, Vector3 Position, Random Rng)
        {
            var dict = new Dictionary<string, Action>
            {
                {"Explorers", () => TravellingExplorers.Build(Position, Utils.Rng)}
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
        
        private static bool ShouldSpawnMob(Vector3 NewPosition)
        {
            var region = World.BiomePool.GetRegion(NewPosition);
            return Utils.Rng.Next(0, 50) != 1 || region.Mob.SpawnerSettings.MiniBosses.Length == 0;
        }

        protected virtual SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
            var region = World.BiomePool.GetRegion(NewPosition);
            var mountain = NewPosition.Y > 60 * Chunk.BlockSize;
            var shore = NewPosition.Y / Chunk.BlockSize > 0 && NewPosition.Y / Chunk.BlockSize < 2;
            var forest = !shore && World.TreeGenerator.SpaceNoise(NewPosition.X, NewPosition.Z) > 0;
            var plains = !forest && !shore && !mountain;

            var templates = new[]
            {
                region.Mob.SpawnerSettings.Forest,
                region.Mob.SpawnerSettings.Mountain,
                region.Mob.SpawnerSettings.Plains,
                region.Mob.SpawnerSettings.Shore
            };

            var conditions = new[]
            {
                forest,
                mountain,
                plains,
                shore
            };

            for (var i = 0; i < templates.Length; i++)
            {
                if (conditions[i])
                {
                    SpawnTemplate type = null;
                    while (type == null)
                    {
                        var template = templates[i][_rng.Next(0, templates[i].Length)];

                        if (_rng.NextFloat() < template.Chance)
                            type = template;
                    }
                    return type;
                }
            }
            return null;
        }

        private bool Conflicts(Vector3 Position)
        {
            try
            {
                for (var i = World.Entities.Count - 1; i > -1; i--)
                {
                    if (World.Entities[i] == _player || World.Entities[i].IsStatic) continue;

                    if ((World.Entities[i].BlockPosition.Xz - Position.Xz).LengthSquared <
                        80f * Chunk.BlockSize * 80f * Chunk.BlockSize) return true;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.WriteLine(e.Message);
                return true;
            }

            return Vector3.Dot(Physics.NormalAtPosition(Position), Vector3.UnitY) <= .35;
        }
        
        private Vector3 PlacementPosition()
        {
            return new Vector3(Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f,
                    0,
                    Utils.Rng.NextFloat() * GameSettings.ChunkLoaderRadius * Chunk.Width - GameSettings.ChunkLoaderRadius * Chunk.Width * .5f)
                + _player.Position.Xz.ToVector3();
        }
    }

    public enum MobType
    {
        Sheep,
        Pig,
        Boar,
        Bee,
        Human,
        Horse,
        Wolf,
        Gorilla,
        Beetle,
        Troll,
        Pug,
        Cow,
        Bear,
        TotalCount,
        None,
        Unknown
    }

}
