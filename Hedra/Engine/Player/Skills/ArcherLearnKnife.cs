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

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class LearnKnife : Skill
	{

		public LearnKnife(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/LearnKnife.png");
			base.Passive = true;
			this.Player = Player;
		}
		
		public override void Update()
		{
			if(base.Level == 0) return;
			if(base.Level > 1) Player.SkillSystem.SetPoints(this.GetType(), 1);	
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Knife);
		}
		
		public override string Description => "Learn to use the knife.";

	    public override void KeyDown(){}
	}
}