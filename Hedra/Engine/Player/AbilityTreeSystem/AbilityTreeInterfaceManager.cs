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
        private readonly LocalPlayer _player;
        private readonly InventoryArrayInterface _interface;
        public AbilityTreeInterfaceManager(LocalPlayer Player, InventoryInterfaceItemInfo ItemInfoInterface, InventoryArrayInterface Interface)
            : base(ItemInfoInterface, Interface)
        {
            _player = Player;
            _interface = Interface;
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
            var button = (Button) Sender;
            var index = this.IndexFromButton(button);
            var decomposedIndexY = index % AbilityTreeSystem.AbilityTree.Layers;
            var decomposedIndexX = AbilityTreeSystem.AbilityTree.AbilityCount / AbilityTreeSystem.AbilityTree.Layers-1 - (index - decomposedIndexY) / AbilityTreeSystem.AbilityTree.Layers;
            var item = this.ItemFromButton(button);
            var locked = decomposedIndexX * 5 >= _player.Level;
            var previousUnlocked = this.PreviousUnlocked(index);

            if (this.AvailablePoints > 0 && !locked && previousUnlocked)
            {
                item.SetAttribute("Level", item.GetAttribute<int>("Level")+1);
            }
            else
            {
                if (locked)
                    _player.MessageDispatcher.ShowNotification("YOU NEED LEVEL " + decomposedIndexX * 5 + " TO UNLOCK THIS SKILL", Color.DarkRed, 3.0f);
                else if(!previousUnlocked)
                    _player.MessageDispatcher.ShowNotification("YOU NEED TO UNLOCK THE PREVIOUS SKILL", Color.DarkRed, 3.0f);
                else
                    Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1.0f, 0.6f);              
            }
            this.UpdateView();
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {

        }

        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTreeSystem.AbilityTree.Layers;
            var decomposedIndexX = AbilityTreeSystem.AbilityTree.AbilityCount / AbilityTreeSystem.AbilityTree.Layers - 1 - (Index - decomposedIndexY) / AbilityTreeSystem.AbilityTree.Layers;
            if (decomposedIndexX == 0) return true;
            else if (!_interface.Array[Index + AbilityTreeSystem.AbilityTree.Layers].GetAttribute<bool>("Enabled"))
                return this.PreviousUnlocked(Index + AbilityTreeSystem.AbilityTree.Layers);
            return _interface.Array[Index + AbilityTreeSystem.AbilityTree.Layers].GetAttribute<int>("Level") > 0;
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

        private int AvailablePoints => _player.Level - UsedPoints;
        private int UsedPoints
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
