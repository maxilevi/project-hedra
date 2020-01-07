using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Mission;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SolitaryFishermanDesign : QuestGiverStructureDesign<SolitaryFisherman>
    {
        public override int PlateauRadius => 16;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.CauldronIcon);
        public override bool CanSpawnInside => true;
        protected override int StructureChance => StructureGrid.SolitaryFisherman;
        protected override CacheItem? Cache => null;
        protected override BlockType PathType => BlockType.None;

        protected override SolitaryFisherman Create(Vector3 Position, float Size)
        {
            return new SolitaryFisherman(Position);
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            return Rng.Next(0, StructureChance) == 1
                   && FishingPostDesign.IsWater(ChunkOffset.ToVector3(), Biome) &&
                   FishingPostDesign.SearchForShore(ChunkOffset, Biome, out TargetPosition);
        }

        protected override IMissionDesign SelectQuest(Vector3 Position, Random Rng)
        {
            return MissionPool.Random(Position, QuestTier.Any, QuestHint.Fishing);
        }

        protected override IHumanoid CreateQuestGiverNPC(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            var npc = base.CreateQuestGiverNPC(Position, Quest, Rng);
            npc.SetWeapon(ItemPool.Grab(ItemType.FishingRod).Weapon);
            npc.IsSitting = true;
            return npc;
        }

        protected override Vector3 NPCOffset => Vector3.Zero;
        protected override float QuestChance => 1f;
    }
}