using System;
using System.Linq;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class SpawnQuestDesign : FindStructureDesign
    {   
        protected override bool HasNext => false;
        
        public override string Name => Quests.SpawnQuest.ToString();
        
        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => null;
        
        protected override QuestDesign[] GetDescendants(QuestObject Quest) => null;

        public override string GetThoughtsKeyword(QuestObject Quest) => "quest_spawn_dialog";
        
        public override object[] GetThoughtsParameters(QuestObject Quest) => new object[0];
        
        protected override Vector3 BuildStructurePosition(QuestObject Quest)
        {
            return World.SpawnVillagePoint;
        }

        protected override float BuildStructureRadius(QuestObject Quest) => VillageDesign.MaxVillageRadius;

        protected override string StructureTypeName => Translations.Get("quest_village");
        
        public override void SetupDialog(QuestObject Quest, IPlayer Owner)
        {
            Quest.Giver.SearchComponent<TalkComponent>().AddDialogLine(
                Translation.Create("quest_spawn_find_structure", new object[] {StructureTypeName})
            );
        }

        protected override CacheItem IconCache => CacheItem.VillageIcon;

        protected override float IconScale => .05f;

        protected override QuestReward BuildReward(QuestObject Quest, Random Rng)
        {
            return new QuestReward();
        }
    }
}