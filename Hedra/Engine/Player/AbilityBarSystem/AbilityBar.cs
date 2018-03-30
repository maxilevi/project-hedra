/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:15 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.AbilityBar;
using Hedra.Engine.Player.Inventory;
using OpenTK;

namespace Hedra.Engine.Player.AbilityBarSystem
{
	/// <summary>
	/// Description of SkillsBar.
	/// </summary>
	public class AbilityBar
	{
	    private const int BarItems = 7;
	    private readonly Skill[] _skills = new Skill[0];
		private readonly LocalPlayer _player;
	    private readonly InventoryArray _barItems;
	    private readonly InventoryArray _bagItems;
	    private readonly InventoryArrayInterface _barItemsInterface;
	    private readonly InventoryArrayInterface _bagItemsInterface;
	    private readonly InventoryArrayInterfaceManager _manager;

        public AbilityBar(LocalPlayer Player){
			_player = Player;
            _barItems = new InventoryArray(BarItems);
            _bagItems = new InventoryArray(1);
            _barItemsInterface = new AbilityBarInventoryInterface(_barItems, 0, _barItems.Length, BarItems, Vector2.One);
            _bagItemsInterface = new AbilityBarInventoryInterface(_bagItems, 0, _bagItems.Length, 1, Vector2.One);
            _manager = new InventoryArrayInterfaceManager(null, _barItemsInterface, _bagItemsInterface);
        }

	    public void SetAttackType(Weapon CurrentWeapon)
	    {
	        
	    }

	    public Skill[] Skills => _skills;
		
		public byte[] ToArray()
		{
		    return null;
		}
		
		public void FromInformation(PlayerInformation Information)
		{

		}

        public bool DisableAttack { get; set; }
	}
}