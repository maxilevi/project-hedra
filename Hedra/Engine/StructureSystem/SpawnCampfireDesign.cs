using System;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.QuestSystem.Designs;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class SpawnCampfireDesign : CampfireDesign
    {
        private static readonly Vector3 SpawnOffset = new Vector3(Vector3.UnitZ * 16f);
        
        public override void Build(CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            BuildBaseCampfire(Structure, Vector3.Zero, rng, out var transformationMatrix);
            ((SpawnCampfire) Structure.WorldObject).Villager = CreateVillager(Structure, rng);
            SpawnMat(
                Vector3.UnitX * -16f,
                Vector3.Zero,
                transformationMatrix,
                Structure
            );
        }

        private static IHumanoid CreateVillager(CollidableStructure Structure, Random Rng)
        {
            var position = Structure.Position + -SpawnOffset;
            if (QuestPersistence.SpawnVillager(position, Rng, out var villager))
            {
                var quest = QuestPool.Grab(Quests.SpawnQuest).Build(position, new Random(World.Seed), villager);
                QuestPersistence.SetupQuest(quest, quest.Giver);
            }
            villager.Physics.TargetPosition = position;
            villager.Physics.UsePhysics = false;
            villager.IsSitting = true;
            return villager;
        }
        
        public static void AlignPlayer(IPlayer Player)
        {
            Player.Position = World.SpawnPoint + SpawnOffset;
            Player.Model.TargetRotation = Vector3.Zero;
            Player.Movement.Orientate();
            Player.IsSitting = true;
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var realPosition = World.SpawnPoint;
            World.StructureHandler.SpawnCampfireSpawned = true;
            var structure = base.Setup(realPosition, Rng, new SpawnCampfire(realPosition));
            structure.Mountain.Radius = 48;
            structure.AddGroundwork(new RoundedGroundwork(realPosition, 24, BlockType.StonePath));
            return structure;
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !World.StructureHandler.SpawnCampfireSpawned;
        }
    }
}