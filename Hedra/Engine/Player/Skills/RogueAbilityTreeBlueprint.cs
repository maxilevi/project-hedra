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
using Hedra.Engine.Player.Skills;
using OpenTK;
namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WaterTreeBlueprint.
	/// </summary>
	public class RogueAbilityTreeBlueprint : AbilityTreeBlueprint
	{
		
		public RogueAbilityTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1.000f);
			
			Items[1][0].AbilityType = typeof(Shuriken);	
			Items[1][0].Image = Graphics2D.LoadFromAssets("Skills/Shuriken.png");
			Items[1][0].Enabled = true;
			
			Items[1][3].AbilityType = typeof(TripleShuriken);	
			Items[1][3].Image = Graphics2D.LoadFromAssets("Skills/TripleShuriken.png");
			Items[1][3].Enabled = true;
			
			Items[0][1].AbilityType = typeof(LearnKatar);
			Items[0][1].Image = Graphics2D.LoadFromAssets("Skills/Katar.png");
			Items[0][1].Enabled = true;
			
			Items[0][2].AbilityType = typeof(LearnClaw);
			Items[0][2].Image = Graphics2D.LoadFromAssets("Skills/Claw.png");
			Items[0][2].Enabled = true;
			
			Items[2][1].AbilityType = typeof(Fade);
			Items[2][1].Image = Graphics2D.LoadFromAssets("Skills/Fade.png");
			Items[2][1].Enabled = true;
			
			Items[2][2].AbilityType = typeof(Venom);
			Items[2][2].Image = Graphics2D.LoadFromAssets("Skills/Venom.png");
			Items[2][2].Enabled = true;
			
			Items[2][0].AbilityType = typeof(BurstOfSpeed);
			Items[2][0].Image = Graphics2D.LoadFromAssets("Skills/BurstOfSpeed.png");
			Items[2][0].Enabled = true;
			
			Items[1][2].AbilityType = typeof(RoundSlash);
			Items[1][2].Image = Graphics2D.LoadFromAssets("Skills/RoundSlash.png");
			Items[1][2].Enabled = true;
			
			//Slots[2][3].AbilityType = typeof(Venom);
			//Slots[2][3].Image = Graphics2D.LoadFromAssets("Fade.png");
			//Slots[2][3].Enabled = true;
		}
	}
}
