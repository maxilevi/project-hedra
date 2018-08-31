/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;


namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Inventory.
    /// </summary>
    public class PlayerInventory : PlayerInterface, IPlayerInventory
    {
        public const int MainSpaces = 8;
        public const int InventorySpaces = 20;
        public const int BootsHolder = 20;
        public const int PantsHolder = 21;
        public const int ChestplateHolder = 22;
        public const int HelmetHolder = 23;
        public const int PetHolder = 24;
        public const int GliderHolder = 25;
        public const int RingHolder = 26;
        public const int WeaponHolder = 27;
        public const int FoodHolder = 19;
        public const int GoldHolder = 18;

        private readonly Vector2 _targetResolution;
        private readonly LocalPlayer _player;
        private readonly InventoryArray _items;
        private readonly InventoryArray _mainItems;
        private readonly InventoryArrayInterface _itemsArrayInterface;
        private readonly InventoryArrayInterface _leftMainItemsArrayInterface;
        private readonly InventoryArrayInterface _rightMainItemsArrayInterface;
        private readonly InventoryArrayInterfaceManager _interfaceManager;
        private readonly InventoryBackground _inventoryBackground;
        private readonly InventoryStateManager _stateManager;
        private readonly RestrictionsInterface _restrictions;
        private bool _show;

        public PlayerInventory(LocalPlayer Player)
        {
            _targetResolution = new Vector2(1366, 705);
            _player = Player;
            _items = new InventoryArray(InventorySpaces);
            _mainItems = new InventoryArray(MainSpaces);
            _restrictions = new RestrictionsInterface(_mainItems);
            _stateManager = new InventoryStateManager(_player);
            _inventoryBackground = new InventoryBackground(Vector2.UnitY * .65f);
            _itemsArrayInterface = new InventoryArrayInterface(_items, 0, _items.Length, 10, Vector2.One)
            {
                Position = Vector2.UnitY * -.65f
            };
            _leftMainItemsArrayInterface = new InventoryArrayInterface(_mainItems, 0, 4, 1, Vector2.One,
                new [] { "Assets/UI/InventorySlotBoots.png", "Assets/UI/InventorySlotPants.png", "Assets/UI/InventorySlotChest.png", "Assets/UI/InventorySlotHelmet.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * -.25f + Vector2.UnitY * .05f
            };
            _rightMainItemsArrayInterface = new InventoryArrayInterface(_mainItems, 4, 4, 1, Vector2.One,
                new[] { "Assets/UI/InventorySlotPet.png", "Assets/UI/InventorySlotGlider.png", "Assets/UI/InventorySlotRing.png", "Assets/UI/InventorySlotWeapon.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * +.25f + Vector2.UnitY * .05f
            };
            _mainItems.OnItemSet += delegate(int Index, Item New)
            {
                if (Index+InventorySpaces == WeaponHolder)
                    _player.Model.SetWeapon( New == null ? Weapon.Empty : New.Weapon);
                if (Index + InventorySpaces == RingHolder)
                    _player.Ring = New;
            };
            var itemInfoInterface = new InventoryInterfaceItemInfo(_itemsArrayInterface.Renderer)
            {
                Position = Vector2.UnitX * .6f + Vector2.UnitY * .1f
            };
            _interfaceManager = new InventoryArrayInterfaceManager(itemInfoInterface, _itemsArrayInterface,
                _leftMainItemsArrayInterface, _rightMainItemsArrayInterface);
            _stateManager.OnStateChange += State =>
            {
                base.Invoke(State);
            };
        }

        public void UpdateInventory()
        {
            _interfaceManager.UpdateView();
        }

        public void ClearInventory()
        {
            _items.Empty();
            _mainItems.Empty();
        }

        public Item Search(Func<Item, bool> Matches)
        {
            var firstResult = _items.Search(Matches);
            if (firstResult != null) return firstResult;
            var secondResult = _mainItems.Search(Matches);
            return secondResult;
        }

        public int IndexOf(Item Item)
        {
            var firstResult = _items.IndexOf(Item);
            if (firstResult != -1) return firstResult;
            var secondResult = _mainItems.IndexOf(Item);
            return secondResult;
        }

        public bool AddItem(Item New)
        {
            var result = _items.AddItem(New);
            _interfaceManager.UpdateView();
            return result;
        }

        public void SetItem(int Index, Item New)
        {
            var array = Index >= InventorySpaces ? _mainItems : _items;
            var index = ToCorrectItemSpace(Index);
            array.SetItem(index, New);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                this.SetItem(Items[i].Key, Items[i].Value);
            }
        }

        public KeyValuePair<int, Item>[] ItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null) list.Add(new KeyValuePair<int, Item>(i, _items[i]));
            }
            return list.ToArray();
        }

        public KeyValuePair<int, Item>[] EquipmentItemsToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _mainItems.Length; i++)
            {
                if (_mainItems[i] != null) list.Add(new KeyValuePair<int, Item>(i + InventorySpaces, _mainItems[i]));
            }
            return list.ToArray();
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            list.AddRange(this.ItemsToArray());
            list.AddRange(this.EquipmentItemsToArray());
            return list.ToArray();
        }

        private void SetInventoryState(bool State)
        {        
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                UpdateManager.CursorShown = true;
            }
            else
            {
                _stateManager.ReleaseState();
            }
        }

        public void Update()
        {
            if (_show)
            {
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float) Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float) Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float) Math.Acos(-_player.Orientation.X),
                    (float) Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float) Time.DeltaTime * 16f);
                _inventoryBackground.UpdateView(_player);
            }
        }

        public bool HasRestrictions(int Index, EquipmentType Type)
        {
            return _restrictions.HasRestriction(ToCorrectItemSpace(WeaponHolder), Type.ToString());
        }

        public void AddRestriction(int Index, EquipmentType Type)
        {
            this.AddRestriction(Index, Type.ToString());
        }

        public void AddRestriction(int Index, string Type)
        {
            _restrictions.AddRestriction(ToCorrectItemSpace(Index), Type);
        }

        private static int ToCorrectItemSpace(int Index)
        {
            return Index >= InventorySpaces ? Index - InventorySpaces : Index;
        }

        public Item Food
        {
            get { return this.Search(I => I.IsFood); }
        }

        public bool HasAvailableSpace => _items.HasAvailableSpace;
        public Item this[int Index] => (Index >= InventorySpaces ? _mainItems : _items)[ToCorrectItemSpace(Index)];
        public Item MainWeapon => this[WeaponHolder];
        public Item Ring => this[RingHolder];
        public Item Glider => this[GliderHolder];
        public Item Pet => this[PetHolder];
        public Item Helmet => this[HelmetHolder];
        public Item Chest => this[ChestplateHolder];
        public Item Pants => this[PantsHolder];
        public Item Boots => this[BootsHolder];
        public int Length => _items.Length + _mainItems.Length;

        public override Key OpeningKey => Key.I;
        public override bool Show
        {
            get => _show;
            set
            {
                if(_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _itemsArrayInterface.Enabled = _show;
                _leftMainItemsArrayInterface.Enabled = _show;
                _rightMainItemsArrayInterface.Enabled = _show;
                _inventoryBackground.Enabled = _show;
                _interfaceManager.Enabled = _show;
                this.UpdateInventory();
                this.SetInventoryState(_show);
                SoundManager.PlayUISound(SoundType.OnOff, 1.0f, 0.6f);
            }
        }
    }
}
