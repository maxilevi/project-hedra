/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Linq;
using System.Collections.Generic;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class LearnAxe : Skill
	{

		public LearnAxe(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Axe.png");
			base.Passive = true;
			this.Player = Player;
		}
		
		public override void Update()
		{
			if(base.Level == 0)return;
			
			//Manually set the level cap
			if(base.Level > 1)
				Player.SkillSystem.SetPoints(this.GetType(), 1);
			
			for(int i = 0; i < Player.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType].Length; i++){
				if(Player.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType][i] == Item.ItemType.Axe)
					return;
			}
			List<Item.ItemType> Types = new List<Item.ItemType>();
			Types.AddRange(Player.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType]);
			Types.Add(Item.ItemType.Axe);
			Player.Inventory.EquipmentTypes[Inventory.WeaponEquipmentType] = Types.ToArray();
		}
		
		public override string Description {
			get {
				return "Learn to use the axe.";
			}
		}
		
		public override void KeyDown(){}
	}
}
