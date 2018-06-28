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
	internal class LearnDoubleSwords : BaseSkill
	{

		public LearnDoubleSwords() : base() {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/DoubleSwords.png");
			base.Passive = true;
		}
		
		public override void Update()
		{
			if(base.Level == 0) return;
			if(base.Level > 1) Player.AbilityTree.SetPoints(this.GetType(), 1);
		    Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.DoubleBlades);
        }
		
		public override string Description {
			get {
				return "Learn to use the double swords.";
			}
		}
		
		public override void Use(){}
	}
}
