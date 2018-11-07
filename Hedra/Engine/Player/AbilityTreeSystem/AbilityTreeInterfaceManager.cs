using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly Texture[] _lines;
        private readonly IPlayer _player;
        private readonly InventoryArrayInterface _interface;
        private readonly InventoryInterfaceItemInfo _itemInfo;
        public AbilityTreeInterfaceManager(IPlayer Player, InventoryInterfaceItemInfo ItemInfoInterface, InventoryArrayInterface Interface)
            : base(ItemInfoInterface, Interface)
        {
            _player = Player;
            _interface = Interface;
            _itemInfo = ItemInfoInterface;
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            var button = (Button) Sender;
            var index = this.IndexFromButton(button);
            var decomposedIndexY = index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns-1 - (index - decomposedIndexY) / AbilityTree.Columns;
            var item = this.ItemFromButton(button);
            var locked = decomposedIndexX * 5 > _player.Level;
            var previousUnlocked = this.PreviousUnlocked(index);

            if (this.AvailablePoints > 0 && !locked && previousUnlocked)
            {
                _player.AbilityTree.SetPoints(index, item.GetAttribute<int>("Level")+1);
            }
            else
            {
                if (locked)
                    _player.MessageDispatcher.ShowNotification("YOU NEED LEVEL " + decomposedIndexX * 5 + " TO UNLOCK THIS SKILL", Color.DarkRed, 3.0f);
                else if(!previousUnlocked)
                    _player.MessageDispatcher.ShowNotification("YOU NEED TO UNLOCK THE PREVIOUS SKILL", Color.DarkRed, 3.0f);
                else
                    Sound.SoundManager.PlayUISound(Sound.SoundType.ButtonHover, 1.0f, 0.6f);              
            }
            this.UpdateView();
            _itemInfo.Show(item);
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {

        }

        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTree.Columns;
            var decomposedIndexX = AbilityTree.AbilityCount / AbilityTree.Columns - 1 - (Index - decomposedIndexY) / AbilityTree.Columns;
            if (decomposedIndexX == 0) return true;
            else if (!_interface.Array[Index + AbilityTree.Columns].GetAttribute<bool>("Enabled"))
                return this.PreviousUnlocked(Index + AbilityTree.Columns);
            return _interface.Array[Index + AbilityTree.Columns].GetAttribute<int>("Level") > 0;
        }

        private int IndexFromButton(Button Sender)
        {
            for (var i = 0; i < _interface.Buttons.Length; i++)
            {
                if (Sender == _interface.Buttons[i])
                    return i;
            }
            return -1;
        }

        private Item ItemFromButton(Button Sender)
        {
            for (var i = 0; i < _interface.Buttons.Length; i++)
            {
                if (Sender == _interface.Buttons[i])
                    return _interface.Array[i];
            }
            return null;
        }

        public int AvailablePoints => _player.Level - UsedPoints;
        public int UsedPoints
        {
            get
            {
                var used = 0;
                for (var i = 0; i < _interface.Array.Length; i++)
                    used += _interface.Array[i].HasAttribute("Level") ? _interface.Array[i].GetAttribute<int>("Level") : 0;
                return used;
            }
        }
    }
}
