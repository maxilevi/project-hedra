/*
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
using OpenTK;
namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WaterTreeBlueprint.
	/// </summary>
	public class ArcherTreeBlueprint : TreeBlueprint
	{
		
		public ArcherTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(0.5625f, 0.25f, 0.296875f, 1.000f);
			
			Slots[0][0].AbilityType = typeof(Kick);	
			Slots[0][0].Image = Graphics2D.LoadFromAssets("Skills/Kick.png");
			Slots[0][0].Enabled = true;
			
			Slots[0][2].AbilityType = typeof(LearnKnife);	
			Slots[0][2].Image = Graphics2D.LoadFromAssets("Skills/LearnKnife.png");
			Slots[0][2].Enabled = true;
			
			//Slots[0][3].AbilityType = typeof(LearnCrossbow);	
			//Slots[0][3].Image = Graphics2D.LoadFromAssets("LearnCrossbow.png");
			//Slots[0][3].Enabled = true;
			
			Slots[1][1].AbilityType = typeof(Agility);	
			Slots[1][1].Image = Graphics2D.LoadFromAssets("Skills/Agility.png");
			Slots[1][1].Enabled = true;
			
			Slots[1][2].AbilityType = typeof(Puncture);	
			Slots[1][2].Image = Graphics2D.LoadFromAssets("Skills/PierceArrows.png");
			Slots[1][2].Enabled = true;
			
			Slots[2][0].AbilityType = typeof(PoisonArrow);	
			Slots[2][0].Image = Graphics2D.LoadFromAssets("Skills/PoisonArrow.png");
			Slots[2][0].Enabled = true;
			
			Slots[2][3].AbilityType = typeof(FlameArrow);	
			Slots[2][3].Image = Graphics2D.LoadFromAssets("Skills/FlameArrow.png");
			Slots[2][3].Enabled = true;
			
			Slots[2][1].AbilityType = typeof(IceArrow);	
			Slots[2][1].Image = Graphics2D.LoadFromAssets("Skills/IceArrow.png");
			Slots[2][1].Enabled = true;
		}
	}
}
