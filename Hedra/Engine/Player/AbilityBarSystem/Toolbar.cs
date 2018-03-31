/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:15 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using System.Text;
using Hedra.Engine.Events;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.Skills;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.AbilityBarSystem
{
	/// <summary>
	/// Description of SkillsBar.
	/// </summary>
	public class Toolbar
	{
        public const int InteractableItems = 4;
	    public const int BarItems = 7;
	    private const char Marker = '!';
		private readonly LocalPlayer _player;
	    private readonly InventoryArray _barItems;
	    private readonly InventoryArray _bagItems;
	    private readonly ToolbarInventoryInterface _barItemsInterface;
	    private readonly AbilityBagInventoryInterface _bagItemsInterface;
	    private readonly ToolbarInterfaceManager _manager;
	    private readonly ToolbarInputHandler _inputHandler;
	    private BaseSkill[] _skills;
	    private WeaponAttack _w1;
	    private WeaponAttack _w2;
        private bool _show;

        public Toolbar(LocalPlayer Player){
			_player = Player;
            _barItems = new InventoryArray(BarItems);
            _bagItems = new InventoryArray(AbilityTree.AbilityCount-1);
            _barItemsInterface = new ToolbarInventoryInterface(_player, _barItems, 0, _barItems.Length, BarItems, Vector2.One)
            {
                Position = Vector2.UnitY * -.875f,
                IndividualScale = Vector2.One * 1.0f
            };
            _bagItemsInterface = new AbilityBagInventoryInterface(_player, _bagItems, 0, _bagItems.Length, AbilityTree.AbilityCount / 2, Vector2.One)
            {
                Position = Vector2.UnitY * -.6f,
                IndividualScale = Vector2.One * 0.85f
            };
            _manager = new ToolbarInterfaceManager(_player, _barItemsInterface, _bagItemsInterface);
            _inputHandler = new ToolbarInputHandler(_player);
            this.LoadSkills();
        }

	    private void LoadSkills()
	    {
	        Type[] skillsTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(Assembly => Assembly.GetLoadableTypes())
                .Where(Type => Type.IsSubclassOf(typeof(BaseSkill))).Where(Type => Type != typeof(WeaponAttack)).ToArray();
	        _skills = new BaseSkill[skillsTypes.Length];
	        for (var i = 0; i < Skills.Length; i++)
	        {
	            _skills[i] = (BaseSkill) Activator.CreateInstance(skillsTypes[i], Vector2.Zero, InventoryArrayInterface.DefaultSize, _player.UI.GamePanel, _player);
	            _skills[i].MaskId = InventoryArrayInterface.DefaultId;
                _skills[i].Active = false;
	        }
            _w1 = new WeaponAttack(_barItemsInterface.Textures[4].Position, _barItemsInterface.Textures[4].Scale, _player.UI.GamePanel, _player);
            _w2 = new WeaponAttack(_barItemsInterface.Textures[5].Position, _barItemsInterface.Textures[5].Scale, _player.UI.GamePanel, _player);

	        _w1.MaskId = InventoryArrayInterface.DefaultId;
	        _w2.MaskId = InventoryArrayInterface.DefaultId;
            EventDispatcher.RegisterMouseDown(this, this.MouseDown);
	        EventDispatcher.RegisterMouseUp(this, this.MouseUp);
        }

        private void MouseDown(object Sender, MouseButtonEventArgs EventArgs)
        {
            if(!_player.CanInteract || _player.Knocked || _player.IsDead || _player.IsUnderwater || _player.IsSwimming ||
                _player.IsGliding || _player.Inventory.Show || _player.AbilityTree.Show || GameSettings.Paused || _player.Trade.Show) return;

            switch (EventArgs.Button)
            {
                case MouseButton.Left:
                    if (!_w1.MeetsRequirements(this, 0)) return;
                    _w1.Cooldown = _w1.MaxCooldown;
                    _w1.KeyDown();
                    break;
                case MouseButton.Right:
                    if (!_w2.MeetsRequirements(this, 0)) return;
                    _w2.Cooldown = _w2.MaxCooldown;
                    _w2.KeyDown();
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

	    public BaseSkill SkillAt(int Index)
	    {
	        return _skills.FirstOrDefault(S => _barItems[Index].HasAttribute("AbilityType") 
                && S.GetType() == _barItems[Index].GetAttribute<Type>("AbilityType"));
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

	    public BaseSkill[] Skills => _skills;
		
		public byte[] ToArray()
		{
		    var saveData = string.Empty;
		    for (var i = 0; i < InteractableItems; i++)
		    {
		        var skill = this.SkillAt(i);
                saveData += skill == null ? string.Empty : skill.GetType().Name + Toolbar.Marker;
		    }
		    return Encoding.ASCII.GetBytes(saveData);
		}
		
		public void FromInformation(PlayerInformation Information)
		{
            var saveData = Encoding.ASCII.GetString(Information.ToolbarArray);
		    var splits = saveData.Split(Toolbar.Marker);
            for (var i = 0; i < _skills.Length; i++)
		    {
		        for (var k = 0; k < splits.Length; k++)
		        {
		            var type = _skills[i].GetType();
                    if (type.Name == splits[k])
		            {
                        _manager.Switch( _bagItemsInterface.Buttons.First(
                            B => _bagItemsInterface.Array[Array.IndexOf(_bagItemsInterface.Buttons, B)]
                            .GetAttribute<Type>("AbilityType") == type) );
		            }
		        }
		    }
		}

        public bool DisableAttack { get; set; }

	    public bool BagEnabled
	    {
	        get { return _bagItemsInterface.Enabled; }
	        set
	        {
	            _bagItemsInterface.Enabled = value;
	        }
	    }

        public bool Show
	    {
	        get { return _show; }
	        set
	        {
	            _show = value;
	            _manager.Enabled = value;
	            _bagItemsInterface.Enabled = _player.AbilityTree.Show;
                this.UpdateView();
	        }
	    }
	}
}