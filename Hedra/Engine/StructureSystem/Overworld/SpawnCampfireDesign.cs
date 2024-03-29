using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SpawnCampfireDesign : CampfireDesign
    {
        private static readonly Vector3 SpawnOffset = Vector3.UnitZ * 16f;
        public static bool Spawned { get; set; }
        public override bool CanSpawnInside => true;
        public override bool IsFixed => true;

        public override void Build(CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            BuildBaseCampfire(Structure.Position, Vector3.Zero, Structure, rng, out var transformationMatrix);
            ((SpawnCampfire)Structure.WorldObject).Villager = CreateVillager(Structure, rng);
            SpawnCampfireMat(
                Vector3.UnitX * -16f,
                Vector3.Zero,
                transformationMatrix,
                Structure
            );
        }

        private static IHumanoid CreateVillager(CollidableStructure Structure, Random Rng)
        {
            var position = Structure.Position + -SpawnOffset;
            var missionDesign = MissionPool.Grab(Quests.VisitSpawnVillage);
            var villager = NPCCreator.SpawnQuestGiver(position, missionDesign, Rng);
            villager.Position = position;
            villager.Physics.CollidesWithEntities = false;
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
            Spawned = true;
            var structure = base.Setup(realPosition, Rng, new SpawnCampfire(realPosition));
            structure.Mountain.Radius = 48;
            structure.AddGroundwork(new RoundedGroundwork(realPosition, 24, BlockType.StonePath));
            return structure;
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, ref Vector3 TargetPosition, CollidableStructure[] Items,
            Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !Spawned;
        }
    }
}