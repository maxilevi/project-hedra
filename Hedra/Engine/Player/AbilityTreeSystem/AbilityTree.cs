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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Input;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Silk.NET.Input;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public delegate void OnSkillUpdated(AbstractBaseSkill Skill);

    public delegate void OnSpecializationLearned(AbilityTreeBlueprint SpecializationTree);

    public class AbilityTree : PlayerInterface, IAbilityTree
    {
        public const int Rows = 4;
        public const int Columns = 3;
        public const int AbilityCount = Columns * Rows;
        public const int SpecializationLevelRequirement = 5;
        private const char SaveMarker = '!';
        private const char NumberMarker = '|';
        private const string HeaderMarker = "<>";
        private readonly AbilityInventoryBackground _background;
        private readonly AbilityTreeInterface _interface;
        private readonly AbilityTreeInterfaceManager _manager;
        private readonly IPlayer _player;
        private readonly InventoryStateManager _stateManager;
        private readonly Vector2 _targetResolution;
        private InventoryArray _abilities;
        private bool _show;
        private readonly Dictionary<int, int> _skillChanges;

        public AbilityTree(IPlayer Player)
        {
            _player = Player;
            _targetResolution = new Vector2(1366, 705);
            FirstTree = BuildArray();
            SecondTree = BuildArray();
            _abilities = MainTree = BuildArray();
            _skillChanges = new Dictionary<int, int>();
            _interface = new AbilityTreeInterface(_player, _abilities, 0, _abilities.Length, Columns)
            {
                Position = Vector2.UnitX * -.65f + Vector2.UnitY * -.1f,
                SpecializationInfo =
                {
                    Position = Vector2.UnitX * .6f + Vector2.UnitY * .1f
                }
            };
            var itemInfo = new AbilityTreeInterfaceItemInfo
            {
                Position = _interface.SpecializationInfo.Position
            };
            _manager = new AbilityTreeInterfaceManager(_player, itemInfo, _interface, MainTree, FirstTree,
                SecondTree);
            _stateManager = new InventoryStateManager(_player);
            _background = new AbilityInventoryBackground(Vector2.UnitY * .65f);
            _stateManager.OnStateChange += State => { Invoke(State); };
        }

        public int SpecializationTreeIndex { get; set; }

        public override Key OpeningKey => Controls.Skilltree;


        private byte[] MainTreeSave => BuildSaveData(MainTree);

        private byte[] FirstTreeSave => BuildSaveData(FirstTree);

        private byte[] SecondTreeSave => BuildSaveData(SecondTree);

        protected override bool HasExitAnimation => true;
        public event OnSkillUpdated SkillUpdated;
        public event OnSpecializationLearned SpecializationLearned;

        public void Update()
        {
            if (_show)
            {
                _player.View.TargetPitch = Mathf.Lerp(_player.View.TargetPitch, 0f, Time.DeltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, Time.DeltaTime * 16f);
                _player.View.TargetYaw = Mathf.Lerp(_player.View.TargetYaw,
                    (float)Math.Atan2(-_player.Orientation.Z, -_player.Orientation.X),
                    Time.DeltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    Time.DeltaTime * 16f);
                _background.UpdateView(_player);
            }
        }

        public void SetPoints(Type AbilityType, int Count)
        {
            var item = _abilities.Search(I => I.GetAttribute<Type>("AbilityType") == AbilityType);
            if (item != null) SetPoints(_abilities.IndexOf(item), Count);
        }

        public void SetPoints(int Index, int Count)
        {
            if (!_skillChanges.ContainsKey(Index))
                _skillChanges.Add(Index, _abilities[Index].GetAttribute<int>("Level"));

            _abilities[Index].SetAttribute("Level", Count);
            for (var k = 0; k < _player.Toolbar.Skills.Length; k++)
                if (_player.Toolbar.Skills[k].GetType() == _abilities[Index].GetAttribute<Type>("AbilityType"))
                {
                    _player.Toolbar.Skills[k].Level = Count;
                    SkillUpdated?.Invoke(_player.Toolbar.Skills[k]);
                }

            UpdateView();
        }

        public void ConfirmPoints()
        {
            _skillChanges.Clear();
            UpdateView();
        }

        public bool IsConfirmed(int Index)
        {
            return !_skillChanges.ContainsKey(Index);
        }

        public void Reset()
        {
            SetBlueprint(_player.Class.FirstSpecializationTree, FirstTree);
            for (var i = 0; i < AbilityCount; i++) SetPoints(i, 0);
            SetBlueprint(_player.Class.SecondSpecializationTree, SecondTree);
            for (var i = 0; i < AbilityCount; i++) SetPoints(i, 0);
            SetBlueprint(_player.Class.MainTree, MainTree);
            for (var i = 0; i < AbilityCount; i++) SetPoints(i, 0);
            SpecializationTreeIndex = 0;
        }

        public void ShowBlueprint(AbilityTreeBlueprint Blueprint, InventoryArray Array, byte[] AbilityTreeArray)
        {
            SetBlueprint(Blueprint, Array);
            if (AbilityTreeArray != null)
            {
                if (AbilityTreeArray.Length > 0)
                {
                    var saveData = Encoding.ASCII.GetString(AbilityTreeArray);
                    if (!saveData.StartsWith(HeaderMarker)) return;

                    var splits = saveData.Substring(HeaderMarker.Length, saveData.Length - HeaderMarker.Length)
                        .Split(SaveMarker);
                    for (var i = 0; i < splits.Length; i++)
                    {
                        var subSplits = splits[i].Split(NumberMarker);
                        var firstSplit = subSplits[0];
                        if (firstSplit == string.Empty) continue;
                        var secondSplit = subSplits[1];
                        for (var k = 0; k < _abilities.Length; k++)
                            if (firstSplit == _abilities[k].GetAttribute<Type>("AbilityType")?.Name)
                                SetPoints(k, int.Parse(secondSplit));
                    }
                }
                else
                {
                    Reset();
                }
            }

            ConfirmPoints();
        }

        public AbilityTreeBlueprint Specialization { get; private set; }

        public void LearnSpecialization(AbilityTreeBlueprint Blueprint)
        {
            if (_player.Class.FirstSpecializationTree.Identifier == Blueprint.Identifier)
                SpecializationTreeIndex = 1;
            else if (_player.Class.SecondSpecializationTree.Identifier == Blueprint.Identifier)
                SpecializationTreeIndex = 2;

            SoundPlayer.PlayUISound(SoundType.NotificationSound);
            UpdateView();
            SpecializationLearned?.Invoke(Blueprint);
        }

        public bool IsTreeEnabled(AbilityTreeBlueprint Blueprint)
        {
            if (_player.Class.FirstSpecializationTree.Identifier == Blueprint.Identifier)
                return HasFirstSpecialization;
            if (_player.Class.SecondSpecializationTree.Identifier == Blueprint.Identifier)
                return HasSecondSpecialization;
            /* Is the default tree, always true */
            return true;
        }

        public bool IsCurrentTreeEnabled => IsTreeEnabled(Specialization);

        public bool HasSpecialization => SpecializationTreeIndex != 0;

        public bool HasFirstSpecialization => SpecializationTreeIndex == 1;

        public bool HasSecondSpecialization => SpecializationTreeIndex == 2;

        public int AvailablePoints => _manager.AvailablePoints;
        public int UsedPoints => _manager.UsedPoints;
        public Item[] TreeItems => MainTree.Items.Concat(FirstTree.Items).Concat(SecondTree.Items).ToArray();
        public InventoryArray MainTree { get; }

        public InventoryArray FirstTree { get; }

        public InventoryArray SecondTree { get; }

        public override bool Show
        {
            get => _show;
            set
            {
                if (_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                if (!_show)
                    ResetChanges();
                _player.Toolbar.BagEnabled = _show;
                _player.Toolbar.PassiveEffectsEnabled = !_show;
                _interface.Enabled = _show;
                _background.Enabled = _show;
                _manager.Enabled = _show;
                if (_show)
                {
                    ShowBlueprint(_player.Class.MainTree, MainTree, null);
                    _skillChanges.Clear();
                }

                SetInventoryState(_show);
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
            }
        }

        public int ExtraSkillPoints
        {
            get => _manager.ExtraSkillPoints;
            set => _manager.ExtraSkillPoints = value;
        }

        public void Dump(BinaryWriter Writer)
        {
            var main = MainTreeSave;
            var first = FirstTreeSave;
            var second = SecondTreeSave;

            Writer.Write(main.Length);
            Writer.Write(main);
            Writer.Write(first.Length);
            Writer.Write(first);
            Writer.Write(second.Length);
            Writer.Write(second);
            Writer.Write(SpecializationTreeIndex);
            Writer.Write(ExtraSkillPoints);
        }

        public void Load(BinaryReader Reader)
        {
            var mainArray = Reader.ReadBytes(Reader.ReadInt32());
            var firstArray = Reader.ReadBytes(Reader.ReadInt32());
            var secondArray = Reader.ReadBytes(Reader.ReadInt32());
            SpecializationTreeIndex = Reader.ReadInt32();
            ExtraSkillPoints = Reader.ReadInt32();

            ResetArray(FirstTree);
            ResetArray(SecondTree);
            ResetArray(MainTree);

            ShowBlueprint(_player.Class.FirstSpecializationTree, FirstTree, firstArray);
            ShowBlueprint(_player.Class.SecondSpecializationTree, SecondTree, secondArray);
            ShowBlueprint(_player.Class.MainTree, MainTree, mainArray);
        }

        private static InventoryArray BuildArray()
        {
            var array = new InventoryArray(AbilityCount);
            ResetArray(array);
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

        private void ResetChanges()
        {
            foreach (var change in _skillChanges) SetPoints(change.Key, change.Value);
            ConfirmPoints();
        }

        private static byte[] BuildSaveData(InventoryArray Abilities)
        {
            var saveData = HeaderMarker;
            for (var i = 0; i < Abilities.Length; i++)
            {
                var skill = Abilities[i];
                saveData += skill == null
                    ? string.Empty + SaveMarker
                    : (skill.GetAttribute<Type>("AbilityType")?.Name ?? string.Empty) + NumberMarker +
                      skill.GetAttribute<int>("Level") + SaveMarker;
            }

            return Encoding.ASCII.GetBytes(saveData);
        }

        private void SetBlueprint(AbilityTreeBlueprint Blueprint, InventoryArray Array)
        {
            _abilities = Array;
            for (var i = 0; i < Blueprint.Items.Length; i++)
            for (var j = 0; j < Blueprint.Items[i].Length; j++)
            {
                var index = (Blueprint.Items[i].Length - 1 - j) * Columns + i;
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

            _interface.SetArray(Array);
            _interface.SetBlueprint(Specialization = Blueprint);
        }

        private static void ResetArray(InventoryArray Array)
        {
            Array.Empty();
            for (var i = 0; i < Array.Length; i++)
            {
                Array[i] = new Item
                {
                    Model = new VertexData()
                };
                Array[i].SetAttribute("Level", 0);
            }
        }
    }
}