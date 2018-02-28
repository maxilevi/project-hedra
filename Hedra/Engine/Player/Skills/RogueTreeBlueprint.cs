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
	public class RogueTreeBlueprint : TreeBlueprint
	{
		
		public RogueTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1.000f);
			
			Slots[1][0].AbilityType = typeof(Shuriken);	
			Slots[1][0].Image = Graphics2D.LoadFromAssets("Skills/Shuriken.png");
			Slots[1][0].Enabled = true;
			
			Slots[1][3].AbilityType = typeof(TripleShuriken);	
			Slots[1][3].Image = Graphics2D.LoadFromAssets("Skills/TripleShuriken.png");
			Slots[1][3].Enabled = true;
			
			Slots[0][1].AbilityType = typeof(LearnKatar);
			Slots[0][1].Image = Graphics2D.LoadFromAssets("Skills/Katar.png");
			Slots[0][1].Enabled = true;
			
			Slots[0][2].AbilityType = typeof(LearnClaw);
			Slots[0][2].Image = Graphics2D.LoadFromAssets("Skills/Claw.png");
			Slots[0][2].Enabled = true;
			
			Slots[2][1].AbilityType = typeof(Fade);
			Slots[2][1].Image = Graphics2D.LoadFromAssets("Skills/Fade.png");
			Slots[2][1].Enabled = true;
			
			Slots[2][2].AbilityType = typeof(Venom);
			Slots[2][2].Image = Graphics2D.LoadFromAssets("Skills/Venom.png");
			Slots[2][2].Enabled = true;
			
			Slots[2][0].AbilityType = typeof(BurstOfSpeed);
			Slots[2][0].Image = Graphics2D.LoadFromAssets("Skills/BurstOfSpeed.png");
			Slots[2][0].Enabled = true;
			
			Slots[1][2].AbilityType = typeof(RoundSlash);
			Slots[1][2].Image = Graphics2D.LoadFromAssets("Skills/RoundSlash.png");
			Slots[1][2].Enabled = true;
			
			//Slots[2][3].AbilityType = typeof(Venom);
			//Slots[2][3].Image = Graphics2D.LoadFromAssets("Fade.png");
			//Slots[2][3].Enabled = true;
		}
	}
}
