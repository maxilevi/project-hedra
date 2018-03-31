/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/08/2016
 * Time: 07:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    /// <summary>
    /// Description of SkillSystem.
    /// </summary>
    public class AbilityTree
    {
        public const int AbilityCount = 15;
        public const int Layers = 3;
        private readonly LocalPlayer _player;
        private readonly InventoryArray _abilities;
        private readonly AbilityTreeInterface _interface;
        private readonly AbilityTreeInterfaceManager _manager;
        private readonly InventoryStateManager _stateManager;
        private readonly AbilityInventoryBackground _background;
        private AbilityTreeBlueprint _blueprint;
        private bool _show;

        public AbilityTree(LocalPlayer Player)
        {
            _player = Player;
            _abilities = new InventoryArray(AbilityCount);
            for (var i = 0; i < _abilities.Length; i++)
            {
                _abilities[i] = new Item();
                _abilities[i].Model = new VertexData();
                _abilities[i].SetAttribute("Level", 0);
                
            }
            _interface = new AbilityTreeInterface(_player, _abilities, 0, _abilities.Length, Layers, new Vector2(1.5f, 1.5f))
            {
                Position = Vector2.UnitX * -.65f + Vector2.UnitY * -.25f
            };
            _interface.IndividualScale = Vector2.One * 1.1f;
            var itemInfo = new AbilityTreeInterfaceItemInfo(_interface.Renderer)
            {
                Position = Vector2.UnitX * .6f + Vector2.UnitY * .1f
            };
            _manager = new AbilityTreeInterfaceManager(_player, itemInfo, _interface);
            _stateManager = new InventoryStateManager(_player);
            _background = new AbilityInventoryBackground(Vector2.UnitY * .65f);
        }

        public void UpdateView()
        {
            _manager.UpdateView();
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
                _player.View.Pitch = Mathf.Lerp(_player.View.Pitch, 0f, (float)Time.deltaTime * 16f);
                _player.View.TargetDistance =
                    Mathf.Lerp(_player.View.TargetDistance, 10f, (float)Time.deltaTime * 16f);
                _player.View.Yaw = Mathf.Lerp(_player.View.Yaw, (float)Math.Acos(-_player.Orientation.X),
                    (float)Time.deltaTime * 16f);
                _player.View.CameraHeight = Mathf.Lerp(_player.View.CameraHeight, Vector3.UnitY * 4,
                    (float)Time.deltaTime * 16f);
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
                this.SetPoints(i,0);
            }
        }

        public void FromInformation(PlayerInformation Information)
        {           
            this._blueprint = BlueprintBuilder.Build(Information.ClassType);
            var bytes = Information.AbilityTreeArray;
            this.SetBlueprint(_blueprint);
            this.UpdateView();
        }

        public byte[] ToArray()
        {
            return new byte[0];
        }

        private void SetBlueprint(AbilityTreeBlueprint Blueprint)
        {
            for (var i = 0; i < Blueprint.Items.Length; i++)
            {
                for (var j = 0; j < Blueprint.Items[i].Length; j++)
                {
                    var index = (Blueprint.Items[i].Length-1 - j) * Layers + i;
                    var button = _interface.Buttons[index];
                    var slot = Blueprint.Items[i][j];
                    var ability = _abilities[index];

                    ability.SetAttribute("ImageId", slot.Image);
                    ability.SetAttribute("AbilityType", slot.AbilityType);
                    ability.SetAttribute("Enabled", slot.Enabled);
                    if (slot.Enabled)
                    {
                        ability.DisplayName = slot.AbilityType.Name.AddSpacesToSentence();
                        ability.Description = string.Empty;
                    }

                    button.Texture.Opacity = 0.75f;
                    button.Texture.TextureId = slot.Image;
                    button.Texture.MaskId = _interface.Textures[index].TextureElement.Id;
                }
            }
        }

        public int AvailablePoints => _manager.AvailablePoints;
        public InventoryArray TreeItems => _abilities;
        public bool Show
        {
            get { return _show; }
            set
            {
                if (_show == value || _stateManager.GetState() != _show) return;
                _show = value;
                _interface.Enabled = _show;              
                _background.Enabled = _show;
                _manager.Enabled = _show;
                _player.Toolbar.BagEnabled = _show;
                if(_show)
                    this.SetBlueprint(_blueprint);
                this.UpdateView();
                this.SetInventoryState(_show);
            }
        }
    }
}
