using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public sealed class GiantTree : BaseStructureWithChest, ICompletableStructure
    {
        public IEntity Boss { get; set; }

        public bool Completed => Boss?.IsDead ?? false;

        public GiantTree(Vector3 Position) : base(Position)
        {           
        }
        
        public override void Dispose()
        {
            Boss?.Dispose();
            base.Dispose();
        }
        
        public ItemDescription DeliveryItem =>
            ItemDescription.FromItem(Chest.ItemSpecification, Translations.Get("quest_pickup_chest_description", Chest.ItemSpecification.DisplayName));

        public QuestReward Reward => null;
    }
}