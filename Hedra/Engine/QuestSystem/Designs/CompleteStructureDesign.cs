using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Engine.StructureSystem;
using Hedra.Items;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CompleteStructureDesign : QuestDesign
    {
        private const string Reward = "QuestReward";

        protected override QuestReward BuildReward(QuestObject Quest, Random Rng)
        {
            if (Quest.Parameters.Has(Reward)) return Quest.Parameters.Get<QuestReward>(Reward);
            var reward = default(QuestReward);
            if (HasDelivery(Quest, out var item))
            {
                reward = new QuestReward
                {
                    Gold = (int) (TradeManager.Price(ItemPool.Grab(item.Name)) *
                                  (1.25f + Utils.Rng.NextFloat() * .5f))
                };
            }    
            else
            {
                reward = new QuestReward
                {
                    Item = ItemPool.Grab(ItemTier.Rare)
                };
            }

            SetReward(Quest.Parameters, reward);
            return reward;
        }

        public override bool HasLocation => true;

        public override Vector3 GetLocation(QuestObject Quest) => Quest.Parameters.Get<Vector2>("StructureLocation").ToVector3();
        
        public override QuestTier Tier => QuestTier.Normal;

        public override string Name => "CompleteStructureDesign";
        
        public override string GetThoughtsKeyword(QuestObject Quest) => throw new NotImplementedException();

        public override object[] GetThoughtsParameters(QuestObject Quest) => throw new NotImplementedException();

        public override string GetShortDescription(QuestObject Quest) => GetDesign(Quest).GetShortDescription(GetStructure(Quest));

        public override string GetDescription(QuestObject Quest) => GetDesign(Quest).GetDescription(GetStructure(Quest));

        protected override QuestParameters BuildParameters(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set("StructureDesign", Previous.Parameters.Get<StructureDesign>("StructureDesign"));
            SetPosition(Parameters, Previous.Parameters.Get<Vector2>("StructureLocation"));
            return base.BuildParameters(Previous, Parameters, Rng);
        }

        public override QuestView BuildView(QuestObject Quest)
        {
            return new ModelView(
                Quest,
                IconModel(Quest).Clone().Scale(Vector3.One)
            );
        }

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => new QuestDesign[]
        {
            HasDelivery(Quest, out _) ? new NoneDesign() : (QuestDesign) new SpeakDesign()
        };

        protected override QuestDesign[] GetDescendants(QuestObject Quest) => HasDelivery(Quest, out _)
            ? new QuestDesign[]
            {
                new PickUpItemDesign()
            }
            : GetRealDescendants(Quest);
        
        private static QuestDesign[] GetRealDescendants(QuestObject Quest)
        {
            return null;
        }

        protected override void Consume(QuestObject Quest)
        {
            base.Consume(Quest);
            if (HasDelivery(Quest, out var deliveryItem))
            {
                Quest.Parameters.Set("QuestDescendants", GetRealDescendants(Quest));
                Quest.Parameters.Set(Reward, CreateReward(Quest));
            }
        }

        private bool HasDelivery(QuestObject Quest, out ItemDescription DeliveryItem)
        {
            var item = default(ItemDescription);
            if (Quest.Parameters.Has(PickUpItemDesign.PickupParameterName))
            {
                item = Quest.Parameters.Get<ItemDescription>(PickUpItemDesign.PickupParameterName);
            }
            else
            {
                var structure = GetStructure(Quest);
                item = structure.DeliveryItem;
                Quest.Parameters.Set(PickUpItemDesign.PickupParameterName, item);
                if(structure.Reward != null)
                    Quest.Parameters.Set(Reward, structure.Reward);
            }              
            return (DeliveryItem = item) != null;
        }
        
        public override bool IsQuestCompleted(QuestObject Quest)
        {
            return GetStructure(Quest).Completed;
        }

        public override Dictionary<string, object> GetContent(QuestObject Quest)
        {
            var dict = new Dictionary<string, object>();
            if (HasDelivery(Quest, out var item))
            {
                dict.Add("HasDelivery", true);
                dict.Add(PickUpItemDesign.PickupParameterName, item);
            }
            dict.Add("Location", GetLocation(Quest));
            dict.Add("Reward", CreateReward(Quest));
            return dict;
        }
        
        public override void LoadContent(QuestObject Quest, Dictionary<string, object> Content)
        {
            if (Content.ContainsKey("HasDelivery"))
            {
                Quest.Parameters.Set(PickUpItemDesign.PickupParameterName, (ItemCollect)Content[PickUpItemDesign.PickupParameterName]);
            }
            SetReward(Quest.Parameters, (QuestReward) Content["Reward"]);
            SetPosition(Quest.Parameters, (Vector2)Content["Location"]);
        }

        private static void SetReward(QuestParameters Parameters, QuestReward RewardObject)
        {
            Parameters.Set(Reward, RewardObject);
        }
        
        private static void SetPosition(QuestParameters Parameters, Vector2 Position)
        {
            Parameters.Set("StructureLocation", Position);
        }

        private QuestReward CreateReward(QuestObject Quest)
        {
            return BuildReward(Quest, new Random(Quest.Parameters.Get<int>("Seed")));
        }
        
        private ICompletableStructure GetStructure(QuestObject Quest)
        {
            var design = GetDesign(Quest);
            var location = World.ToChunkSpace(GetLocation(Quest));
            var structure = World.StructureHandler.Find(
                S => S.Design == design && S.MapPosition == location
            );
            return (ICompletableStructure) structure.WorldObject;
        }

        private static ICompletableStructureDesign GetDesign(QuestObject Quest)
        {
            return Quest.Parameters.Get<ICompletableStructureDesign>("StructureDesign");
        }
        
        private static VertexData IconModel(QuestObject Quest) => Quest.Parameters.Get<IFindableStructureDesign>("StructureDesign").QuestIcon;
    }
}