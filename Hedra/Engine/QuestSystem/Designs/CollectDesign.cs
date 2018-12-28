using System;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CollectDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;
        
        public override QuestView View { get; } = new BulletView();

        public override Translation OpeningLine { get; } = Translation.Create("collect_these_object_dialog_0");

        public override QuestDesign[] Descendants => new QuestDesign[]
        {
            new CraftDesign()
        };
        
        public override QuestDesign[] Predecessors => null;

        protected override QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set("Items", GetRandomItems(Context, Rng));
            return Parameters;
        }
        
        public override bool IsQuestCompleted(QuestObject Object, IPlayer Player)
        {
            return Object.Parameters.Get<ItemCollect[]>("Items").All(
                I => Player.Inventory.Search(T => T.Name == I.Name && T.GetAttribute<int>(CommonAttributes.Amount) >= I.Amount
            ) != null);
        }

        private static ItemCollect[] GetRandomItems(QuestContext Context, Random Rng)
        {
            var templates = TemplatesFromContext(Context, Rng);
            var items = new ItemCollect[Rng.Next(1, 4)];
            for (var i = 0; i < items.Length; ++i)
            {
                var template = templates[Rng.Next(0, templates.Length)];
                items[i] = new ItemCollect
                {
                    Name = template.Name,
                    Amount = template.Amount
                };
            }
            return items;
        }

        private static ItemCollect[] TemplatesFromContext(QuestContext Context, Random Rng)
        {
            switch (Context.ContextType)
            {
                case QuestContextType.Spawn:
                    return new[]
                    {
                        new ItemCollect
                        {
                            Name = QuestItem.Berry.ToString(),
                            Amount = Rng.Next(3, 10)
                        },
                        new ItemCollect
                        {
                            Name = QuestItem.Mushrooms.ToString(),
                            Amount = Rng.Next(1, 2)
                        }
                    };
                case QuestContextType.Village:
                    return new[]
                    {
                        new ItemCollect
                        {
                            Name = QuestItem.Corn.ToString(),
                            Amount = Rng.Next(3, 10)
                        },
                        new ItemCollect
                        {
                            Name = QuestItem.Pumpkin.ToString(),
                            Amount = Rng.Next(4, 10)
                        },
                    };
                case QuestContextType.Wilderness:
                    return new[]
                    {
                        new ItemCollect
                        {
                            Name = QuestItem.RawMeat.ToString(),
                            Amount = Rng.Next(2, 6)
                        },
                        new ItemCollect
                        {
                            Name = QuestItem.Mushrooms.ToString(),
                            Amount = Rng.Next(1, 4)
                        }
                    };
                default:
                    throw new ArgumentOutOfRangeException($"Unknown QuestContextType '{Context.ContextType}'");
            }
        }
        
        public override string ToString(QuestObject Object)
        {
            return Object.Parameters.Get<ItemCollect[]>("Items").Select(
                I => I.ToString()).Aggregate((S1,S2) => $"{S1}{Environment.NewLine}{S2}"
            );
        }

        private struct ItemCollect
        {
            public int Amount { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"• {Amount} {ItemPool.Grab(Name).DisplayName}";
            }
        }
    }
}