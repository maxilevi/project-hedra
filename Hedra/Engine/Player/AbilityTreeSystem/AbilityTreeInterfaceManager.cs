using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly Texture[] _lines;
        private readonly IPlayer _player;
        private readonly AbilityTreeInterface _interface;
        private readonly InventoryInterfaceItemInfo _itemInfo;
        public AbilityTreeInterfaceManager(IPlayer Player, InventoryInterfaceItemInfo ItemInfoInterface, AbilityTreeInterface Interface)
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
            var locked = decomposedIndexX * 5 > _player.Level || !_player.AbilityTree.IsCurrentTreeEnabled;
            var previousUnlocked = this.PreviousUnlocked(index);

            if (this.AvailablePoints > 0 && !locked && previousUnlocked)
            {
                _player.AbilityTree.SetPoints(index, item.GetAttribute<int>("Level") + 1);
            }
            else
            {
                if (locked)
                {
                    if(!_player.AbilityTree.IsCurrentTreeEnabled)
                        _player.MessageDispatcher.ShowNotification(Translations.Get("need_specialize_to_unlock", _player.AbilityTree.Specialization.DisplayName), Color.DarkRed, 3.0f);
                    else
                        _player.MessageDispatcher.ShowNotification(Translations.Get("need_level_to_unlock", decomposedIndexX * 5), Color.DarkRed, 3.0f);
                }
                else if (!previousUnlocked)
                {
                    _player.MessageDispatcher.ShowNotification(Translations.Get("unlock_previous_skill"), Color.DarkRed, 3.0f);
                }
                else
                {
                    SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
                }
            }
            UpdateView();
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {

        }

        protected override void HoverEnter(object Sender, MouseEventArgs EventArgs)
        {
            base.HoverEnter(Sender, EventArgs);
            _interface.SpecializationInfo.ShowSpecialization(null);
        }

        protected override void HoverExit(object Sender, MouseEventArgs EventArgs)
        {
            base.HoverExit(Sender, EventArgs);
            var blueprint = _player.AbilityTree.Specialization;
            _interface.SpecializationInfo.ShowSpecialization(blueprint.IsSpecialization ? blueprint : null);
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
