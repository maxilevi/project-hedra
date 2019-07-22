using System;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Engine.StructureSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class FindOverworldStructureDesign : FindStructureDesign
    {
        protected override QuestReward BuildReward(QuestObject Quest, Random Rng) => throw new NotImplementedException();
        
        public override QuestTier Tier => QuestTier.Normal;

        public override string Name => Quests.FindOverworldStructure.ToString();

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest)
        {
            if (Quest.Parameters.Get<IFindableStructureDesign>("StructureDesign").IsCompletable)
            {
                return new QuestDesign[]
                {
                    new NoneDesign()
                };
            }
            throw new NotImplementedException();
        }

        protected override QuestDesign[] GetDescendants(QuestObject Quest)
        {
            if (Quest.Parameters.Get<IFindableStructureDesign>("StructureDesign").IsCompletable)
            {
                return new QuestDesign[]
                {
                    new CompleteStructureDesign()
                };
            }
            throw new NotImplementedException();
        }

        private static IFindableStructureDesign FindDesign(Vector3 Position, out Vector2 FinalPosition)
        {
            const int radius = 24;
            var rng = BuildRng(Position);
            var chunkSpace = World.ToChunkSpace(Position);
            var offset = new Vector2(rng.NextFloat() * 2 - 1, rng.NextFloat() * 2 -1) * 32 * Chunk.Width;
            for (var x = -radius; x < radius; x++)
            {
                for (var z = -radius; z < radius; z++)
                {
                    FinalPosition = chunkSpace + new Vector2(x, z) * Chunk.Width + offset;
                    var region = World.BiomePool.GetRegion(FinalPosition.ToVector3());
                    var sample = MapBuilder.Sample(FinalPosition.ToVector3(), region);
                    if (sample is IFindableStructureDesign design)
                    {
                        return design;
                    }
                }
            }
            FinalPosition = Position.Xz;
            return null;
        }
        
        protected override Vector3 BuildStructurePosition(QuestObject Quest)
        {
            LazyFind(Quest);
            return Quest.Parameters.Get<Vector2>("StructureLocation").ToVector3();
        }

        protected override float BuildStructureRadius(QuestObject Quest)
        {
            LazyFind(Quest);
            return Quest.Parameters.Get<StructureDesign>("StructureDesign").PlateauRadius;
        }

        private static void LazyFind(QuestObject Quest)
        {
            if (!Quest.Parameters.Has("StructureDesign"))
            {
                var design = FindDesign(Quest.Position, out var final);
                if (design == null)
                {
                    Log.WriteLine("Failed to find suitable structure");
                    throw new ArgumentException("Failed to find suitable structure");
                }

                Quest.Parameters.Set("StructureDesign", design);
                Quest.Parameters.Set("StructureLocation", final);
            }
        }

        public override bool IsAvailable(Vector3 Position)
        {
            return FindDesign(Position, out _) != null;
        }

        private static Random BuildRng(Vector3 Position)
        {
            return new Random(Unique.GenerateSeed(Position.Xz));
        }

        protected override CacheItem IconCache(QuestObject Quest) => throw new NotImplementedException();
        protected override VertexData IconModel(QuestObject Quest) => Quest.Parameters.Get<IFindableStructureDesign>("StructureDesign").QuestIcon;
        protected override string StructureTypeName(QuestObject Quest) => Quest.Parameters.Get<IFindableStructureDesign>("StructureDesign").DisplayName;
    }
}