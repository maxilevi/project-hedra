using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class FindStructureDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;

        public override string GetShortDescription(QuestObject Quest) 
            => Translations.Get("quest_find_structure_short", StructureTypeName);

        public override string GetDescription(QuestObject Quest)
            => Translations.Get("quest_find_structure_description", Quest.Giver.Name, StructureTypeName);
        
        public override string GetThoughtsKeyword(QuestObject Quest)
            => "quest_find_structure_dialog";

        public override object[] GetThoughtsParameters(QuestObject Quest)
            => new object[]
            {
                StructureTypeName
            };

        protected override QuestObject Setup(QuestObject Quest)
        {
            Quest.Parameters.Set("StructurePosition", BuildStructurePosition(Quest));
            Quest.Parameters.Set("StructureRadius", BuildStructureRadius(Quest));
            return Quest;
        }

        protected abstract Vector3 BuildStructurePosition(QuestObject Quest);
        
        protected abstract float BuildStructureRadius(QuestObject Quest);

        public override QuestView BuildView(QuestObject Quest)
        {
            return new ModelView(
                Quest,
                CacheManager.GetModel(IconCache).Clone().Scale(IconScale * Vector3.One)
            );
        }

        public override bool IsQuestCompleted(QuestObject Quest)
        {
            return (Quest.Parameters.Get<Vector3>("StructurePosition") - Quest.Owner.Position)
                   .LengthSquared < Math.Pow(Quest.Parameters.Get<float>("StructureRadius"), 2);
        }

        protected virtual float IconScale => 1;
        
        protected abstract CacheItem IconCache { get; }
        
        protected abstract string StructureTypeName { get; }
    }
}