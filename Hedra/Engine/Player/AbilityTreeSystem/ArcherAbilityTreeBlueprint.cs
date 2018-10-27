﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/08/2016
 * Time: 08:12 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using System.IO;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Archer;
using OpenTK;
namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WaterTreeBlueprint.
	/// </summary>
	public class ArcherAbilityTreeBlueprint : AbilityTreeBlueprint
	{
		
		public ArcherAbilityTreeBlueprint()
		{
			Items[0][0].AbilityType = typeof(Kick);	
			Items[0][0].Image = Graphics2D.LoadFromAssets("Skills/Kick.png");
			Items[0][0].Enabled = true;
			
			Items[0][2].AbilityType = typeof(LearnKnife);	
			Items[0][2].Image = Graphics2D.LoadFromAssets("Skills/LearnKnife.png");
			Items[0][2].Enabled = true;
			
			//Slots[0][3].AbilityType = typeof(LearnCrossbow);	
			//Slots[0][3].Image = Graphics2D.LoadFromAssets("LearnCrossbow.png");
			//Slots[0][3].Enabled = true;
			
			Items[1][1].AbilityType = typeof(Agility);	
			Items[1][1].Image = Graphics2D.LoadFromAssets("Skills/Agility.png");
			Items[1][1].Enabled = true;
			
			Items[1][2].AbilityType = typeof(Puncture);	
			Items[1][2].Image = Graphics2D.LoadFromAssets("Skills/PierceArrows.png");
			Items[1][2].Enabled = true;
			
			Items[2][0].AbilityType = typeof(PoisonArrow);	
			Items[2][0].Image = Graphics2D.LoadFromAssets("Skills/PoisonArrow.png");
			Items[2][0].Enabled = true;
			
			Items[2][3].AbilityType = typeof(FlameArrow);	
			Items[2][3].Image = Graphics2D.LoadFromAssets("Skills/FlameArrow.png");
			Items[2][3].Enabled = true;
			
			Items[2][1].AbilityType = typeof(IceArrow);	
			Items[2][1].Image = Graphics2D.LoadFromAssets("Skills/IceArrow.png");
			Items[2][1].Enabled = true;
		}
	}
}
