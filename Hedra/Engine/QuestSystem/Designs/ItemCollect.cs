using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.QuestSystem.Designs
{
    public struct ItemCollect
    {
        public int Amount { get; set; }
        public string Name { get; set; }
        public string Recipe { get; set; }
        public ItemCollect[] StartingItems { get; set; }

        public void Consume(IPlayer Player)
        {
            var name = Name;
            Player.Inventory.RemoveItem(Player.Inventory.Search(I => I.Name == name), Amount);
        }

        public bool IsCompleted(IPlayer Player, out int CurrentAmount)
        {
            CurrentAmount = 0;
            var amount = Amount;
            var name = Name;
            var item = Player.Inventory.Search(T => T.Name == name);
            return item != null && (CurrentAmount = item.GetAttribute<int>(CommonAttributes.Amount)) >= amount;
        }
            
        public string ToString(IPlayer Player)
        {
            var completed = IsCompleted(Player, out var currentAmount);
            var text = $"• {currentAmount}/{Amount} {ItemPool.Grab(Name).DisplayName}";               
            return $"{new string(' ', 8)}${(completed ? TextFormatting.Green : TextFormatting.Red)}{{{text}}}";
        }
            
        public override string ToString()
        {
            return $"{Amount} {ItemPool.Grab(Name).DisplayName}";
        }
    }
}