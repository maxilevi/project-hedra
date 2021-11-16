using Hedra.Engine.ItemSystem;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public class ItemDescription : ItemCollect
    {
        public string PickupMessage { get; set; }

        public static ItemDescription FromItem(Item Specification, string Message)
        {
            return new ItemDescription
            {
                Name = Specification.Name,
                Amount = Specification.HasAttribute(CommonAttributes.Amount)
                    ? Specification.GetAttribute<int>(CommonAttributes.Amount)
                    : 1,
                PickupMessage = Message
            };
        }
    }
}