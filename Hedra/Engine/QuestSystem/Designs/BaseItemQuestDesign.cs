using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Rendering;
using Newtonsoft.Json.Linq;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class BaseItemQuestDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;

        protected abstract ItemCollect[] Templates(QuestObject Quest, Random Rng);
        
        protected override QuestParameters BuildParameters(QuestObject Previous, QuestParameters Parameters, Random Rng)
        { 
            return Parameters;
        }

        public override void GenerateContent(QuestObject Quest)
        {
            Quest.Parameters.Set("Items", GetItems(Quest));
        }

        public override Dictionary<string, object> GetContent(QuestObject Quest)
        {
            return Quest.IsFirst ? new Dictionary<string, object>
            {
                {"Items", Quest.Parameters.Get<ItemCollect[]>("Items")}
            } : null;
        }

        public override void LoadContent(QuestObject Quest, Dictionary<string, object> Content)
        {
            var jArray = (JArray) Content["Items"];
            Quest.Parameters.Set("Items", jArray.ToObject<ItemCollect[]>());
        }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            return Object.Parameters.Get<ItemCollect[]>("Items").All(
                I => I.IsCompleted(Object.Owner, out _)
            );
        }
        
        protected override void Consume(QuestObject Quest)
        {
            if (Quest.IsEndQuest)
            {
                Quest.Parameters.Get<ItemCollect[]>("Items").ToList().ForEach(
                    I => I.Consume(Quest.Owner)
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
        
        private ItemCollect[] TemplatesFromContext(QuestObject Quest, Random Rng)
        {
            return Templates(Quest, Rng);
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
                SkillPoint =  rng > 0.95f ? 1 : 0,
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

        protected virtual ItemCollect[] GetItems(QuestObject Quest)
        {
            var rng = new Random(Quest.Seed);
            var templates = TemplatesFromContext(Quest, rng);
            var items = new List<ItemCollect>();
            for (var i = 0; i < 1; ++i)
            {
                var template = templates[rng.Next(0, templates.Length)];
                if(items.Contains(template)) continue;
                items.Add(template);
            }
            return items.ToArray();
        }
    }
}