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
	internal class LearnKatar : LearningSkill
	{
		public override uint TexId => Graphics2D.LoadFromAssets("Assets/Skills/Katar.png");
		public override void Learn()
		{
		    Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Katar);
        }
		
		public override string Description {
			get {
				return "Learn to use the katar.";
			}
		}
		
		public override void Use(){}
	}
}