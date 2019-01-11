using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class BaseItemQuestDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;

        protected abstract ItemCollect[] Templates(Random Rng);
        
        protected override QuestParameters BuildParameters(QuestObject Previous, QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set("Items", GetItems(Previous, Parameters, Rng));
            return Parameters;
        }
        
        public override bool IsQuestCompleted(QuestObject Object)
        {
            return Object.Parameters.Get<ItemCollect[]>("Items").All(
                I => I.IsCompleted(Object.Owner, out _)
            );
        }
        
        protected override void Consume(QuestObject Object)
        {
            if (Object.IsEndQuest)
            {
                Object.Parameters.Get<ItemCollect[]>("Items").ToList().ForEach(
                    I => I.Consume(Object.Owner)
                );
            }
        }
      
        public override QuestView BuildView(QuestObject Quest)
        {
            var items = Quest.Parameters.Get<ItemCollect[]>("Items").Select(T => ItemPool.Grab(T.Name)).ToArray();
            var model = new VertexData();
            for (var i = 0; i < items.Length; i++)
            {
                var transform = Matrix4.CreateTranslation(Vector3.UnitZ);
                transform *= Matrix4.CreateRotationY(i * (360 / items.Length) * Mathf.Radian);
                model += items[i].Model.Clone().Transform(transform);
            }
            return new ModelView(Quest, model);
        }
        
        private ItemCollect[] TemplatesFromContext(QuestContext Context, Random Rng)
        {
            return Templates(Rng);
        }

        private static float GetRewardMultiplier(QuestObject Quest, Random Rng)
        {
            return Quest.Parameters.Get<ItemCollect[]>("Items")
                       .Sum(I => ItemPool.Grab(I.Name).GetAttribute<int>(CommonAttributes.Price) * I.Amount) / 25f;
        }
        
        protected override QuestReward BuildReward(QuestObject Quest, Random Rng)
        {
            var rng = Rng.NextFloat();
            return new QuestReward
            {
                Experience = rng < 0.3f ? (int) (Rng.Next(3, 9) * GetRewardMultiplier(Quest, Rng)) : 0,
                Gold = rng > 0.3f && rng < 0.7f ? (int) (Rng.Next(11, 25) * GetRewardMultiplier(Quest, Rng)) : 0,
                Item = rng > 0.75f && rng < 0.95f ? RandomReward(Rng) : null,
            };
        }

        protected virtual Item RandomReward(Random Rng)
        {
            Item[] items;
            var rng = Rng.NextFloat();
            if (rng < .3f)
                items = ItemPool.Matching(T => T.EquipmentType == EquipmentType.Recipe.ToString() && (int) T.Tier == (int) ItemTier.Uncommon);
            else if (rng < .8f)
                items = ItemPool.Matching(T => T.EquipmentType == EquipmentType.Recipe.ToString() && (int) T.Tier == (int) ItemTier.Common);
            else
                items = ItemPool.Matching(T => T.Tier == ItemTier.Misc);

            return items[Rng.Next(0, items.Length)];
        }

        protected virtual ItemCollect[] GetItems(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            var templates = TemplatesFromContext(Parameters.Get<QuestContext>("Context"), Rng);
            var items = new List<ItemCollect>();
            for (var i = 0; i < 1; ++i)
            {
                var template = templates[Rng.Next(0, templates.Length)];
                if(items.Contains(template)) continue;
                items.Add(template);
            }
            return items.ToArray();
        }
    }
}