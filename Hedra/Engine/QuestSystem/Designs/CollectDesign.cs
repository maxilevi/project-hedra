using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CollectDesign : BaseItemQuestDesign
    {
        public override string Name => "CollectQuest";

        public override string GetThoughtsKeyword(QuestObject Quest) => "quest_collect_dialog";

        public override object[] GetThoughtsParameters(QuestObject Quest)
        {
            return new object[]
            {
                Quest.Parameters.Get<ItemCollect[]>("Items")
                .Select(I => I.ToString())
                .Aggregate((S1,S2) => $"{S1}, {S2}")
                .ToUpperInvariant()
            };
        }
        
        public override string GetShortDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_collect_short",
                Quest.Giver.Name,
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString())
                    .Aggregate((S1,S2) => $"{S1}{S2}")
            );
        }

        public override string GetDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_collect_description",
                Quest.Giver.Name,
                Quest.Parameters.Get<ItemCollect[]>("Items")
                    .Select(I => I.ToString(Quest.Owner))
                    .Aggregate((S1,S2) => $"{S1}{Environment.NewLine}{S2}")
            );
        }

        protected override QuestDesign[] Descendants => new QuestDesign[]
        {
            new CraftDesign()
        };

        protected override QuestDesign[] Auxiliaries => new QuestDesign[]
        {
            new SpeakDesign()
        };

        protected override int RandomItemCount(Random Rng, ItemCollect[] Templates)
        {
            return 1;
        }

        protected override ItemCollect[] SpawnTemplates(Random Rng)
        {
            return WildernessTemplates(Rng);
        }
        
        protected override ItemCollect[] VillageTemplates(Random Rng)
        {
            return new[]
            {
                new ItemCollect
                {
                    Name = QuestItem.Corn.ToString(),
                    Amount = Rng.Next(3, 10),
                    Recipe = ItemType.CornSoupRecipe.ToString()
                },
                new ItemCollect
                {
                    Name = QuestItem.Pumpkin.ToString(),
                    Amount = Rng.Next(4, 10),
                    Recipe = ItemType.PumpkinPieRecipe.ToString()
                },
            };
        }
        
        protected override ItemCollect[] WildernessTemplates(Random Rng)
        {
            return new[]
            {
                new ItemCollect
                {
                    Name = QuestItem.RawMeat.ToString(),
                    Amount = Rng.Next(2, 6),
                    Recipe = ItemType.CookedMeatRecipe.ToString()
                },
                new ItemCollect
                {
                    Name = QuestItem.Mushroom.ToString(),
                    Amount = Rng.Next(1, 4),
                    Recipe = ItemType.HealthPotionRecipe.ToString()
                },
                new ItemCollect
                {
                    Name = QuestItem.Mushroom.ToString(),
                    Amount = Rng.Next(3, 6),
                    Recipe = ItemType.MushroomStewRecipe.ToString(),
                    //StartingItems = 
                },
                new ItemCollect
                {
                    Name = QuestItem.Berry.ToString(),
                    Amount = Rng.Next(3, 8),
                    Recipe = ItemType.HealthPotionRecipe.ToString()
                }
            };
        }
    }
}