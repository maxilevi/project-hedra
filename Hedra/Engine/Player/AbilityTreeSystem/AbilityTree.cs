/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/08/2016
 * Time: 07:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;
using Cursor = Hedra.Engine.Input.Cursor;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    /// <summary>
    /// Description of SkillSystem.
    /// </summary>
    public class AbilityTree : PlayerInterface, IAbilityTree
    {
        public const int Rows = 4;
        public const int Columns = 3;
        public const int AbilityCount = Columns * Rows;
        public int SpecializationTreeIndex { get; set; }
        private const char SaveMarker = '!';
        private const char NumberMarker = '|';
        private const string HeaderMarker = "<>";
        private readonly Vector2 _targetResolution;
        private readonly IPlayer _player;
        private readonly InventoryArray _mainTree;
        private readonly InventoryArray _firstTree;
        private readonly InventoryArray _secondTree;
        private readonly AbilityTreeInterface _interface;
        private readonly AbilityTreeInterfaceManager _manager;
        private readonly InventoryStateManager _stateManager;
        private readonly AbilityInventoryBackground _background;
        private InventoryArray _abilities;
        private bool _show;

        public AbilityTree(IPlayer Player)
        {
            _player = Player;
            _targetResolution = new Vector2(1366, 705);
            _firstTree = BuildArray();
            _secondTree = BuildArray();
            _abilities = _mainTree = BuildArray();
            _interface = new AbilityTreeInterface(_player, _abilities, 0, _abilities.Length, Columns)
            {
                Position = Mathf.ScaleGui(_targetResolution, Vector2.UnitX * -.65f + Vector2.UnitY * -.1f),
                SpecializationInfo =
                {
                    Position = Mathf.ScaleGui(_targetResolution, Vector2.UnitX * .6f + Vector2.UnitY * .25f)
                }
            };
            var itemInfo = new AbilityTreeInterfaceItemInfo(_interface.Renderer)
            {
                Position = _interface.SpecializationInfo.Position
            };
            _manager = new AbilityTreeInterfaceManager(_player, itemInfo, _interface);
            _stateManager = new InventoryStateManager(_player);
            _background = new AbilityInventoryBackground(Vector2.UnitY * .65f);

            _stateManager.OnStateChange += State =>
            {
                base.Invoke(State);
            };
        }

        private static InventoryArray BuildArray()
        {
            var array = new InventoryArray(AbilityCount);
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Item
                {
                    Model = new VertexData()
                };
                array[i].SetAttribute("Level", 0);             
            }

            return array;
        }

        private void UpdateView()
        {
            _manager.UpdateView();
        }

        private void SetInventoryState(bool State)
        {
            if (State)
            {
                _stateManager.CaptureState();
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
                Cursor.Show = true;
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
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, (float)Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float)Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw, (float)Math.Acos(-_player.Orientation.X),
                    Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    Time.DeltaTime * 16f);
                _background.UpdateView(_player);
            }
        }

        public void SetPoints(Type AbilityType, int Count)
        {
            var item = _abilities.Search(I => I.GetAttribute<Type>("AbilityType") == AbilityType);
            if(item != null) this.SetPoints(_abilities.IndexOf(item), Count);
        }

        public void SetPoints(int Index, int Count)
        {
            _abilities[Index].SetAttribute("Level", Count);
            for (var k = 0; k < _player.Toolbar.Skills.Length; k++)
            {
                if (_player.Toolbar.Skills[k].GetType() == _abilities[Index].GetAttribute<Type>("AbilityType"))
                {
                    _player.Toolbar.Skills[k].Level = Count;
                }
            }
            this.UpdateView();
        }

        public void Reset()
        {
            for (var i = 0; i < AbilityCount; i++)
            {
                this.SetPoints(i, 0);
            }
        }

        public byte[] MainTreeSave => BuildSaveData(_mainTree);
        
        public byte[] FirstTreeSave => BuildSaveData(_firstTree);
        
        public byte[] SecondTreeSave => BuildSaveData(_secondTree);
        
        private static byte[] BuildSaveData(InventoryArray Abilities)
        {
            var saveData = HeaderMarker;
            for (var i = 0; i < Abilities.Length; i++)
            {
                var skill = Abilities[i];
                saveData += skill == null
                    ? string.Empty + AbilityTree.SaveMarker 
                    : (skill.GetAttribute<Type>("AbilityType")?.Name ?? string.Empty) + AbilityTree.NumberMarker + skill.GetAttribute<int>("Level") + AbilityTree.SaveMarker;
            }
            return Encoding.ASCII.GetBytes(saveData);
        }

        public void ShowBlueprint(AbilityTreeBlueprint Blueprint, byte[] AbilityTreeArray)
        {
            this.SetBlueprint(Blueprint);
            if (AbilityTreeArray != null)
            {
                if (AbilityTreeArray.Length > 0)
                {
                    var saveData = Encoding.ASCII.GetString(AbilityTreeArray);
                    if (!saveData.StartsWith(HeaderMarker)) return;

                    var splits = saveData.Substring(HeaderMarker.Length, saveData.Length - HeaderMarker.Length)
                        .Split(AbilityTree.SaveMarker);
                    for (var i = 0; i < splits.Length; i++)
                    {
                        var subSplits = splits[i].Split(AbilityTree.NumberMarker);
                        var firstSplit = subSplits[0];
                        if (firstSplit == string.Empty) continue;
                        var secondSplit = subSplits[1];
                        for (var k = 0; k < _abilities.Length; k++)
                        {
                            if (firstSplit == _abilities[k].GetAttribute<Type>("AbilityType")?.Name)
                            {
                                this.SetPoints(k, int.Parse(secondSplit));
                            }
                        }
                    }
                }
                else
                {
                    this.Reset();
                }
            }

            this.UpdateView();
        }

        public void FromInformation(PlayerInformation Information)
        {
            _abilities = _firstTree;
            ShowBlueprint(Information.Class.FirstSpecializationTree, Information.FirstSpecializationTreeArray);
            _abilities = _secondTree;
            ShowBlueprint(Information.Class.SecondSpecializationTree, Information.SecondSpecializationTreeArray);
            _abilities = _mainTree;
            ShowBlueprint(Information.Class.MainTree, Information.MainTreeArray);
        }

        public AbilityTreeBlueprint Specialization
        {
            get
            {
                switch (SpecializationTreeIndex)
                {
                    case 0:
                        return null;
                    case 1:
                        return _player.Class.FirstSpecializationTree;
                    case 2:
                        return _player.Class.SecondSpecializationTree;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid tree index");
                }
            }
        }

        public bool HasSpecialization => SpecializationTreeIndex != 0;

        public bool HasFirstSpecialization => SpecializationTreeIndex == 1;
        
        public bool HasSecondSpecialization => SpecializationTreeIndex == 2;
        
        private void SetBlueprint(AbilityTreeBlueprint Blueprint)
        {
            for (var i = 0; i < Blueprint.Items.Length; i++)
            {
                for (var j = 0; j < Blueprint.Items[i].Length; j++)
                {
                    var index = (Blueprint.Items[i].Length-1 - j) * Columns + i;
                    var button = _interface.Buttons[index];
                    var slot = Blueprint.Items[i][j];
                    var ability = _abilities[index];

                    ability.SetAttribute("ImageId", slot.Image);
                    ability.SetAttribute("AbilityType", slot.AbilityType);
                    ability.SetAttribute("Enabled", slot.Enabled);
                    if (slot.Enabled)
                    {
                        var realSkill = _player.Toolbar.Skills.First(S => slot.AbilityType == S.GetType());
                        ability.SetAttribute("Skill", realSkill);
                    }

                    button.Texture.Opacity = 0.75f;
                    button.Texture.TextureId = slot.Image;
                    button.Texture.MaskId = _interface.Textures[index].TextureElement.Id;
                }
            }
            _interface.SetBlueprint(Blueprint);
        }

        public int AvailablePoints => _manager.AvailablePoints;
        public InventoryArray TreeItems => _abilities;

        public override Key OpeningKey => Controls.Skilltree;
        public override bool Show
        {
            get => _show;
            set
            {
                if (_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _player.Toolbar.BagEnabled = _show;
                _interface.Enabled = _show;              
                _background.Enabled = _show;
                _manager.Enabled = _show;
                if(_show)
                    ShowBlueprint(_player.Class.MainTree, null);
                SetInventoryState(_show);
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
            }
        }
    }
}
