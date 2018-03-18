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
using System.Runtime.CompilerServices;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using OpenTK;


namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Inventory.
    /// </summary>
    public class PlayerInventory
    {
        public const int MainSpaces = 8;
        public const int InventorySpaces = 20;
        public const int BootsHolder = 20;
        public const int PantsHolder = 21;
        public const int ChestHolder = 22;
        public const int HelmetHolder = 23;
        public const int PetHolder = 24;
        public const int GliderHolder = 25;
        public const int RingHolder = 26;
        public const int WeaponHolder = 27;
        public const int FoodHolder = 19;
        public const int GoldHolder = 18;

        private readonly LocalPlayer _player;
        private readonly InventoryArray _items;
        private readonly InventoryArray _mainItems;
        private readonly InventoryArrayInterface _itemsArrayInterface;
        private readonly InventoryArrayInterface _leftMainItemsArrayInterface;
        private readonly InventoryArrayInterface _rightMainItemsArrayInterface;
        private readonly InventoryBackground _inventoryBackground;
        private readonly InventoryStateManager _stateManager;
        private readonly RestrictionsInterface _restrictions;
        private bool _show;

        public PlayerInventory(LocalPlayer Player)
        {
            _player = Player;
            _items = new InventoryArray(InventorySpaces);
            _mainItems = new InventoryArray(MainSpaces);
            _restrictions = new RestrictionsInterface(_mainItems);
            _stateManager = new InventoryStateManager(_player);
            _itemsArrayInterface = new InventoryArrayInterface(_items, 0, _items.Length, 10)
            {
                Position = Vector2.UnitY * -.65f
            };
            _leftMainItemsArrayInterface = new InventoryArrayInterface(_mainItems, 0, 4, 1, 
                new [] { "Assets/UI/InventorySlotBoots.png", "Assets/UI/InventorySlotPants.png", "Assets/UI/InventorySlotChest.png", "Assets/UI/InventorySlotHelmet.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * -.25f + Vector2.UnitY * .05f
            };
            _rightMainItemsArrayInterface = new InventoryArrayInterface(_mainItems, 4, 4, 1,
                new[] { "Assets/UI/InventorySlotPet.png", "Assets/UI/InventorySlotGlider.png", "Assets/UI/InventorySlotRing.png", "Assets/UI/InventorySlotWeapon.png" })
            {
                Position = Vector2.UnitY * .05f + Vector2.UnitX * +.25f + Vector2.UnitY * .05f
            };
            _inventoryBackground = new InventoryBackground(Vector2.UnitY * .55f + Vector2.UnitY * .1f);
            _inventoryBackground.Scale *= .55f;
        }

        public void UpdateInventory()
        {
            _itemsArrayInterface.UpdateView();
            _leftMainItemsArrayInterface.UpdateView();
            _rightMainItemsArrayInterface.UpdateView();
        }

        public void ClearInventory()
        {
            _items.Empty();
            _mainItems.Empty();
        }

        public bool AddItem(Item New)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null)
                {
                    this.SetItem(i, New);
                    return true;
                }
            }
            return false;
        }

        public void SetItem(int Index, Item New)
        {
            var array = Index >= InventorySpaces ? _mainItems : _items;
            var index = ToMainItemsSpace(Index);
            array.SetItem(index, New);
            if(Index == WeaponHolder) _player.Model.SetWeapon(New.Weapon);
        }

        public void SetItems(KeyValuePair<int, Item>[] Items)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                this.SetItem(Items[i].Key, Items[i].Value);
            }
        }

        public KeyValuePair<int, Item>[] ToArray()
        {
            var list = new List<KeyValuePair<int, Item>>();
            for (var i = 0; i < _items.Length; i++)
            {
                if(_items[i] != null) list.Add(new KeyValuePair<int, Item>(i, _items[i]));
            }
            for (var i = 0; i < _mainItems.Length; i++)
            {
                if (_mainItems[i] != null) list.Add(new KeyValuePair<int, Item>(i + InventorySpaces, _mainItems[i]));
            }
            return list.ToArray();
        }

        private void SetInventoryState(bool State)
        {        
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.Check = false;
                _player.View.Check = false;
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
                _player.View.Pitch = Mathf.Lerp(_player.View.Pitch, 0f, (float) Time.deltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float) Time.deltaTime * 16f);
                _player.View.Yaw = Mathf.Lerp(_player.View.Yaw, (float) Math.Acos(-_player.Orientation.X),
                    (float) Time.deltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float) Time.deltaTime * 16f);
                _inventoryBackground.UpdateView(_player);
            }
        }

        public bool HasWeaponRestriction(WeaponType Type)
        {
            return _restrictions.HasRestriction(ToMainItemsSpace(WeaponHolder), Type.ToString());
        }

        public void AddWeaponRestriction(WeaponType Type)
        {   
            _restrictions.AddRestriction(ToMainItemsSpace(WeaponHolder), Type.ToString());
        }

        private static int ToMainItemsSpace(int Index)
        {
            return Index >= InventorySpaces ? Index - InventorySpaces : Index;
        }


        public bool HasAvailableSpace => _items.HasAvailableSpace;
        public Item this[int Index] => (Index >= InventorySpaces ? _mainItems : _items)[ToMainItemsSpace(Index)];
        public Item MainWeapon => this[WeaponHolder];
        public Item Food => this[FoodHolder];
        public Item Ring => this[RingHolder];
        public Item Glider => this[GliderHolder];
        public Item Mount => this[PetHolder];
        public Item Helmet => this[HelmetHolder];
        public Item Chest => this[ChestHolder];
        public Item Pants => this[PantsHolder];
        public Item Boots => this[BootsHolder];
        public Item Gold => this[GoldHolder];
        public int Length => _items.Length + _mainItems.Length;

        public bool Show
        {
            get { return _show; }
            set
            {
                if(_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _itemsArrayInterface.Enabled = _show;
                _leftMainItemsArrayInterface.Enabled = _show;
                _rightMainItemsArrayInterface.Enabled = _show;
                _inventoryBackground.Enabled = _show;
                this.UpdateInventory();
                this.SetInventoryState(_show);
            }
        }
    }
}
