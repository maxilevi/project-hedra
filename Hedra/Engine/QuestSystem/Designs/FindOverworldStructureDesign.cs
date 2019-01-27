using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class FindOverworldStructureDesign : FindStructureDesign
    {
        protected override QuestReward BuildReward(QuestObject Quest, Random Rng)
        {
            throw new NotImplementedException();
        }

        public override string Name => "FindOverworldStructureQuest";

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => new QuestDesign[]
        {
            new NoneDesign()
        };

        protected override QuestDesign[] GetDescendants(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        protected override Vector3 BuildStructurePosition(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        protected override float BuildStructureRadius(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        protected override CacheItem IconCache { get; }
        protected override string StructureTypeName { get; }
    }
}