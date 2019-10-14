﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:15 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Loader;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.WeaponSystem;
using System.Numerics;
using MouseButton = Silk.NET.Input.Common.MouseButton;
using TaskScheduler = Hedra.Core.TaskScheduler;

namespace Hedra.Engine.Player.ToolbarSystem
{
    /// <summary>
    /// Description of SkillsBar.
    /// </summary>
    public class Toolbar : IToolbar, IDisposable
    {
        public const int InteractableItems = 4;
        public const int BarItems = 7;
        private const char Marker = '!';
        private const string HeaderMarker = "<>";
        private readonly IPlayer _player;
        private readonly InventoryArray _barItems;
        private readonly InventoryArray _bagItems;
        private readonly ToolbarInventoryInterface _toolbarItemsInterface;
        private readonly AbilityBagInventoryInterface _bagItemsInterface;
        private readonly PassiveEffectsInterface _passiveEffectsInterface;
        private readonly ToolbarInterfaceManager _manager;
        private readonly ToolbarInputHandler _inputHandler;
        private AbstractBaseSkill[] _skills;
        private WeaponAttack _w1;
        private WeaponAttack _w2;
        private bool _show;

        public Toolbar(IPlayer Player)
        {
            _player = Player;
            _barItems = new InventoryArray(BarItems);
            _bagItems = new InventoryArray((AbilityTree.AbilityCount-1) * 3);
            _toolbarItemsInterface = new ToolbarInventoryInterface(_player, _barItems, 0, _barItems.Length, BarItems, Vector2.One)
            {
                Position = Vector2.UnitY * -.825f,
                IndividualScale = Vector2.One * 1.0f
            };
            _bagItemsInterface = new AbilityBagInventoryInterface(_bagItems, 0, _bagItems.Length, _bagItems.Length / 2, Vector2.One)
            {
                Position = Vector2.UnitY * -.6f,
                IndividualScale = Vector2.One * 0.85f
            };
            _passiveEffectsInterface = new PassiveEffectsInterface(_player)
            {
                Position = Vector2.UnitY * -.775f
            };
            _manager = new ToolbarInterfaceManager(_player, _toolbarItemsInterface, _bagItemsInterface, _passiveEffectsInterface)
            {
                HasCancelButton = false
            };
            _inputHandler = new ToolbarInputHandler(_player);
            TaskScheduler.When(
                () => _player.AbilityTree != null,
                () => _player.AbilityTree.SkillUpdated += S => UpdateView()
            );
            this.LoadSkills();
        }

        private void LoadSkills()
        {
            var types = SkillFactory.Instance.GetAll();
            _skills = new AbstractBaseSkill[types.Length];
            for (var i = 0; i < _skills.Length; i++)
            {
                _skills[i] = (AbstractBaseSkill) Activator.CreateInstance(types[i]);
                _skills[i].InitializeUI(Vector2.Zero, InventoryArrayInterface.DefaultSize, _player.UI.GamePanel);
                _skills[i].Initialize(_player);
                _skills[i].StateUpdated += SkillStateUpdated;
                _skills[i].Active = false;
            }
            _w1 = new WeaponAttack();
            _w1.InitializeUI(_toolbarItemsInterface.Textures[4].Position, _toolbarItemsInterface.Textures[4].Scale, _player.UI.GamePanel);
            _w1.Initialize(_player);
            _w2 = new WeaponAttack();
            _w2.InitializeUI(_toolbarItemsInterface.Textures[5].Position, _toolbarItemsInterface.Textures[5].Scale, _player.UI.GamePanel);
            _w2.Initialize(_player);

            EventDispatcher.RegisterMouseDown(this, this.MouseDown);
            EventDispatcher.RegisterMouseUp(this, this.MouseUp);
        }

        private void SkillStateUpdated()
        {
            UpdateView();
        }

        private void MouseDown(object Sender, MouseButtonEventArgs EventArgs)
        {
            if(!Listen || !_player.CanInteract || _player.IsKnocked || _player.IsDead || _player.IsSwimming ||
                _player.IsTravelling || _player.InterfaceOpened || GameManager.InMenu) return;

            switch (EventArgs.Button)
            {
                case MouseButton.Left:
                    if (!_w1.MeetsRequirements() || _w2.IsCharging) return;
                    _w1.Use();
                    break;
                case MouseButton.Right:
                    if (!_w2.MeetsRequirements()) return;
                    _w2.Use();
                    break;
            }
        }

        private void MouseUp(object Sender, MouseButtonEventArgs EventArgs)
        {
            switch (EventArgs.Button)
            {
                case MouseButton.Left:
                    _w1.KeyUp();
                    break;
                case MouseButton.Right:
                    _w2.KeyUp();
                    break;
            }
        }

        public AbstractBaseSkill SkillAt(int Index)
        {
            return _skills.FirstOrDefault(S => _barItems[Index].HasAttribute("AbilityType") 
                && S.GetType() == _barItems[Index].GetAttribute<Type>("AbilityType"));
        }

        public void Update()
        {
            _toolbarItemsInterface.Update();
            _passiveEffectsInterface.Update();
        }

        public void UpdateView()
        {
            _manager.UpdateView();
        }

        public void SetAttackType(Weapon CurrentWeapon)
        {
            _w1.SetType(CurrentWeapon, AttackType.Primary);
            _w2.SetType(CurrentWeapon, AttackType.Secondary);
        }

        public AbstractBaseSkill[] Skills => _skills;
        
        public void Dump(BinaryWriter Writer)
        {
            var saveData = HeaderMarker;
            for (var i = 0; i < InteractableItems; i++)
            {
                var skill = this.SkillAt(i);
                saveData += skill == null ? string.Empty + Toolbar.Marker : skill.GetType().Name + Toolbar.Marker;
            }
            Writer.Write(saveData);
        }
        
        public void Load(BinaryReader Reader)
        {
            _manager.Empty();
            var saveData = Reader.ReadString();
            if(!saveData.StartsWith(HeaderMarker)) return;
            var splits = saveData.Substring(HeaderMarker.Length, saveData.Length-HeaderMarker.Length).Split(Toolbar.Marker);
            for (var k = 0; k < splits.Length; k++)
            {
                for (var i = 0; i < _skills.Length; i++)
                {
                    var type = _skills[i].GetType();
                    if (type.Name == splits[k])
                    {
                        var matchingButton = _bagItemsInterface.Buttons.FirstOrDefault(B =>
                            _bagItemsInterface.Array[Array.IndexOf(_bagItemsInterface.Buttons, B)]
                                .GetAttribute<Type>("AbilityType") == type);

                        if(matchingButton == null) return;
                        var itemIndex = Array.IndexOf(_bagItemsInterface.Buttons, matchingButton);

                        _manager.Move(_bagItemsInterface.Array[itemIndex], type, _toolbarItemsInterface, k);
                    }
                }
            }
        }

        public void ResetSkills()
        {
            for (var i = 0; i < Skills.Length; ++i) Skills[i].Reset();
        }
        
        public void ResetCooldowns()
        {
            for (var i = 0; i < _skills.Length; ++i)
            {
                _skills[i].ResetCooldown();
            }
        }

        public bool DisableAttack
        {
            get => _w1.DisableWeapon && _w2.DisableWeapon;
            set
            {
                _w1.DisableWeapon = value;
                _w2.DisableWeapon = value;
            }
        }

        public bool BagEnabled
        {
            get => _bagItemsInterface.Enabled;
            set
            {
                _bagItemsInterface.Enabled = value;
                var filtered = _manager.GetActiveFilteredSkills();
                var inToolbar = new[] {this.SkillAt(0), this.SkillAt(1), this.SkillAt(2), this.SkillAt(3)};
                for (var i = 0; i < _skills.Length; i++)
                {
                    if (!inToolbar.Contains(_skills[i])){
                        _skills[i].Active = false;
                        if (filtered.Contains(_skills[i])) _skills[i].Active = value;
                    }
                }
            }
        }

        public bool PassiveEffectsEnabled
        {
            get => _passiveEffectsInterface.Enabled;
            set
            {
                if(value)
                    _passiveEffectsInterface.Enable();
                else
                    _passiveEffectsInterface.Disable();
            }
        }

        public bool Listen { get; set; } = true;

        public bool Show
        {
            get => _show;
            set
            {
                _show = value;
                _manager.Enabled = _show;
                this.UpdateView();
            }
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterMouseDown(this);
            EventDispatcher.UnregisterMouseUp(this);
        }
    }
}