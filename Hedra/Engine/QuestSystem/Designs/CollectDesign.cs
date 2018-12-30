using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CollectDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;
        
        public override string Name => "CollectQuest";
        
        public override string GetDisplayName(QuestObject Quest)
        {
            return Translations.Get("quest_collect_name");
        }

        public override string GetDescription(QuestObject Quest)
        {
            var itemList = string.Join(
                Environment.NewLine,
                Quest.Parameters.Get<ItemCollect[]>("Items").Select(I => I.ToString())
            );
            return $"{Translations.Get("quest_items")}:{Environment.NewLine}{itemList}";
        }

        public override QuestView View { get; } = new BulletView();

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
            var count = Rng.Next(1, Math.Min(4, templates.Length));
            var items = new List<ItemCollect>();
            for (var i = 0; i < count; ++i)
            {
                var template = templates[Rng.Next(0, templates.Length)];
                if(items.Contains(template)) continue;
                items.Add(new ItemCollect
                {
                    Name = template.Name,
                    Amount = template.Amount
                });
            }
            return items.ToArray();
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
                            Name = QuestItem.Mushroom.ToString(),
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
                            Name = QuestItem.Mushroom.ToString(),
                            Amount = Rng.Next(1, 4)
                        },
                        new ItemCollect
                        {
                            Name = QuestItem.Berry.ToString(),
                            Amount = Rng.Next(3, 8)
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

        public override VertexData Icon => VertexData.Empty;

        public override VertexData BuildPreview(QuestObject Object)
        {
            var items = Object.Parameters.Get<ItemCollect[]>("Items").Select(T => ItemPool.Grab(T.Name)).ToArray();
            var model = new VertexData();
            for (var i = 0; i < items.Length; i++)
            {
                var transform = Matrix4.CreateTranslation(Vector3.UnitZ);
                transform *= Matrix4.CreateRotationY(i * (360 / items.Length) * Mathf.Radian);
                model += items[i].Model.Clone().Transform(transform);
            }
            return model;
        }

        private struct ItemCollect
        {
            public int Amount { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"    • {Amount} {ItemPool.Grab(Name).DisplayName}";
            }
        }
    }
}