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
	public class LearnClaw : LearningSkill
	{
		public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/Claw.png");
		public override void Learn()
		{
		    Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Claw);
        }
		
		public override string Description => "Learn to use the claws.";
	}
}