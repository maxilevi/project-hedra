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

        protected abstract int RandomItemCount(Random Rng, ItemCollect[] Templates);

        protected abstract ItemCollect[] SpawnTemplates(Random Rng);
        
        protected abstract ItemCollect[] VillageTemplates(Random Rng);
        
        protected abstract ItemCollect[] WildernessTemplates(Random Rng);
        
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
      
        public override QuestView BuildView(QuestObject Object)
        {
            var items = Object.Parameters.Get<ItemCollect[]>("Items").Select(T => ItemPool.Grab(T.Name)).ToArray();
            var model = new VertexData();
            for (var i = 0; i < items.Length; i++)
            {
                var transform = Matrix4.CreateTranslation(Vector3.UnitZ);
                transform *= Matrix4.CreateRotationY(i * (360 / items.Length) * Mathf.Radian);
                model += items[i].Model.Clone().Transform(transform);
            }
            return new ModelView(Object, model);
        }
        
        private ItemCollect[] TemplatesFromContext(QuestContext Context, Random Rng)
        {
            switch (Context.ContextType)
            {
                case QuestContextType.Spawn:
                    return SpawnTemplates(Rng);
                case QuestContextType.Village:
                    return VillageTemplates(Rng);
                case QuestContextType.Wilderness:
                    return WildernessTemplates(Rng);
                default:
                    throw new ArgumentOutOfRangeException($"Unknown QuestContextType '{Context.ContextType}'");
            }
        }

        protected virtual ItemCollect[] GetItems(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            var templates = TemplatesFromContext(Parameters.Get<QuestContext>("Context"), Rng);
            var count = RandomItemCount(Rng, templates);
            var items = new List<ItemCollect>();
            for (var i = 0; i < count; ++i)
            {
                var template = templates[Rng.Next(0, templates.Length)];
                if(items.Contains(template)) continue;
                items.Add(template);
            }
            return items.ToArray();
        }
    }
}