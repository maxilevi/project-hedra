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
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class LearnHammer : BaseSkill
	{

		public LearnHammer() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/Hammer.png");
			base.Passive = true;
		}
		
		public override void Update()
		{
			if(base.Level == 0) return;	
			if(base.Level > 1) Player.AbilityTree.SetPoints(this.GetType(), 1);

		    Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Hammer);
        }
		
		public override string Description => "Learn to use the hammer.";

	    public override void Use(){}
	}
}
