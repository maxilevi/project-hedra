/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Linq;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class LearnKatar : BaseSkill
	{

		public LearnKatar(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Katar.png");
			base.Passive = true;
		}
		
		public override void Update()
		{
			if(base.Level == 0) return;
			if(base.Level > 1) Player.AbilityTree.SetPoints(this.GetType(), 1);
		    Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Katar);
        }
		
		public override string Description {
			get {
				return "Learn to use the katar.";
			}
		}
		
		public override void KeyDown(){}
	}
}