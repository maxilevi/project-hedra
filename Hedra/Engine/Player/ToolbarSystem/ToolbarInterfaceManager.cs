using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class ToolbarInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly ToolbarInventoryInterface _toolbarInferface;
        private readonly AbilityBagInventoryInterface _bagInterface;
        private readonly PassiveEffectsInterface _passiveInterface;
        private readonly IPlayer _player;
        private bool _hasInitialized;

        public ToolbarInterfaceManager(IPlayer Player,
            ToolbarInventoryInterface ToolbarInferface, AbilityBagInventoryInterface BagInterface, PassiveEffectsInterface PassiveInterface)
            : base(null, ToolbarInferface, BagInterface)
        {
            _toolbarInferface = ToolbarInferface;
            _bagInterface = BagInterface;
            _passiveInterface = PassiveInterface;
            _player = Player;
        }

        public void Empty()
        {
            for (var i = 0; i < this._bagInterface.Buttons.Length; i++)
            {
                this._bagInterface.Array[i].SetAttribute("ImageId", 0);
                this._bagInterface.Array[i].SetAttribute("AbilityType", null);
            }
            for (var i = 0; i < this._toolbarInferface.Buttons.Length; i++)
            {
                if (this._toolbarInferface.Array[i].HasAttribute("AbilityType"))
                    this._toolbarInferface.Array[i].SetAttribute("AbilityType", null);
            }
            var filteredSkills = this.GetActiveFilteredSkills();
            for (var i = 0; i < filteredSkills.Length; i++)
            {
                var firstButton = this._bagInterface.Buttons.First(
                    B => _bagInterface.Array[Array.IndexOf(_bagInterface.Buttons, B)]
                             .GetAttribute<Type>("AbilityType") == null);
                var index = Array.IndexOf(_bagInterface.Buttons, firstButton);
                filteredSkills[i].Scale = _toolbarInferface.Textures[0].Scale;
                filteredSkills[i].Position = firstButton.Position;

                this._bagInterface.Array[index].SetAttribute("ImageId", filteredSkills[i].TextureId);
                this._bagInterface.Array[index].SetAttribute("AbilityType", filteredSkills[i].GetType());
            }
        }

        public override void UpdateView()
        {
            var usedTypes = new List<Type>();
            for (var i = 0; i < this._bagInterface.Buttons.Length; i++)
            {
                this._bagInterface.Array[i].SetAttribute("ImageId", 0);
                this._bagInterface.Array[i].SetAttribute("AbilityType", null);
            }
            for (var i = 0; i < this._toolbarInferface.Buttons.Length; i++)
            {
                if (this._toolbarInferface.Array[i].HasAttribute("AbilityType"))
                    usedTypes.Add(this._toolbarInferface.Array[i].GetAttribute<Type>("AbilityType"));
            }
            var allSkills = _player.Toolbar.Skills ?? new BaseSkill[0];
            allSkills.ToList().ForEach( S => S.Active = false);

            var filteredSkills = GetActiveFilteredSkills();
            for (var i = 0; i < filteredSkills.Length; i++)
            {
                filteredSkills[i].Active = this.Enabled && _player.AbilityTree.Show;
                var type = filteredSkills[i].GetType();
                if (usedTypes.Contains(type))
                {
                    filteredSkills[i].Active = true;
                    var button = this._toolbarInferface.Buttons.First(
                        B => _toolbarInferface.Array[Array.IndexOf(_toolbarInferface.Buttons, B)]
                                 .GetAttribute<Type>("AbilityType") == type
                    );
                    filteredSkills[i].Scale = ToolbarScale;
                    filteredSkills[i].Position = button.Position;
                }
                else
                {
                    var index = Array.IndexOf(_bagInterface.Buttons, this._bagInterface.Buttons.First(
                        B => _bagInterface.Array[Array.IndexOf(_bagInterface.Buttons, B)].GetAttribute<Type>("AbilityType") == null));
                    var firstButton = _bagInterface.Buttons[index];
                    filteredSkills[i].Scale = ToolbarScale;
                    filteredSkills[i].Position = firstButton.Position;

                    this._bagInterface.Array[index].SetAttribute("ImageId", filteredSkills[i].TextureId);
                    this._bagInterface.Array[index].SetAttribute("AbilityType", filteredSkills[i].GetType());
                }
            }
            _passiveInterface.UpdateView(GetPassiveAndEnabledFilteredSkills());
            base.UpdateView();
        }

        private Vector2 ToolbarScale => _toolbarInferface.Textures[0].Scale;

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs) {}

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if(!_player.AbilityTree.Show) return;

            var button = (Button) Sender;
            if (_toolbarInferface.Buttons.Contains(button))
            {
                if (!this.AddTo(button, _bagInterface)) SoundPlayer.PlayUISound(SoundType.ButtonHover);
            }
            else
            {
                if (!this.AddTo(button, _toolbarInferface)) SoundPlayer.PlayUISound(SoundType.ButtonHover);
            }
            this.UpdateView();
        }

        private bool AddTo(Button Sender, InventoryArrayInterface Interface)
        {
            var item = this.ItemByButton(Sender);
            var abilityType = item.HasAttribute("AbilityType") 
                ? item.GetAttribute<Type>("AbilityType")
                : null;
            if (abilityType == null) return false;
            var success = false;
            for (var i = 0; i < Interface.Buttons.Length; i++)
            {
                if (Interface.Array[i].HasAttribute("AbilityType") &&
                    Interface.Array[i].GetAttribute<Type>("AbilityType") == null)
                {
                    this.Move(item, abilityType, Interface, i);
                    success = true;
                    break;
                }
            }
            return success;
        }

        private void Set(Type AbilityType, InventoryArrayInterface Interface, int Index)
        {
            Interface.Array[Index].SetAttribute("AbilityType", AbilityType);
        }

        public void Move(Item From, Type AbilityType, InventoryArrayInterface Interface, int Index)
        {
            this.Set(AbilityType, Interface, Index);
            From.SetAttribute("AbilityType", null);
        }

        private BaseSkill[] GetFilteredSkills()
        {
            var items = _player.AbilityTree.TreeItems;
            return _player.Toolbar.Skills?.Where(S =>
            {
                if(!CanLearnSkill(S)) return false;
                var item = items.FirstOrDefault(I => I != null && I.HasAttribute("AbilityType") && I.GetAttribute<Type>("AbilityType") == S.GetType());
                return item != null && item.GetAttribute<bool>("Enabled");
            }).ToArray() ?? new BaseSkill[0];
        }

        private bool CanLearnSkill(BaseSkill S)
        {
            return _player.AbilityTree != null
                   && (!_player.Class.MainTree?.Has(S.GetType()) ?? false)
                   && (!_player.AbilityTree.Specialization?.Has(S.GetType()) ?? false);
        }
        
        public BaseSkill[] GetActiveFilteredSkills()
        {
            return GetFilteredSkills().Where(S => !S.Passive).ToArray();
        }

        private BaseSkill[] GetPassiveAndEnabledFilteredSkills()
        {
            return GetFilteredSkills().Where(S =>S.Level > 0 && S.IsAffecting).ToArray();
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if(Enabled)
                    _passiveInterface.Enable();
                else
                    _passiveInterface.Disable();
            }
        }
    }
}
