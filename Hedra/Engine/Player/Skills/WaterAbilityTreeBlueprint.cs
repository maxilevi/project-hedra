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
	public class WaterAbilityTreeBlueprint : AbilityTreeBlueprint
	{
		
		public WaterAbilityTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(0.000f, 0.396f, 0.725f, 1.000f);
			
			Items[0][0].AbilityType = typeof(Fireball);	
			Items[0][0].Image = Graphics2D.LoadFromAssets("Fireball.png");
			Items[0][0].Enabled = true;
			
			Items[0][1].AbilityType = typeof(TripleFireball);	
			Items[0][1].Image = Graphics2D.LoadFromAssets("TripleFireball.png");
			Items[0][1].Enabled = true;
			
			Items[0][3].AbilityType = typeof(Meteor);	
			Items[0][3].Image = Graphics2D.LoadFromAssets("Meteor.png");
			Items[0][3].Enabled = true;
			
			Items[1][1].AbilityType = typeof(FlameStyle);
			Items[1][1].Image = Graphics2D.LoadFromAssets("FlameStyle.png");
			Items[1][1].Enabled = true;
			
			Items[1][2].AbilityType = typeof(FlameJump);
			Items[1][2].Image = Graphics2D.LoadFromAssets("FlameJump.png");
			Items[1][2].Enabled = true;
			
			Items[1][3].AbilityType = typeof(FlameDisk);
			Items[1][3].Image = Graphics2D.LoadFromAssets("FlameDisk.png");
			Items[1][3].Enabled = true;
			
			Items[2][0].AbilityType = typeof(Conflagaration);
			Items[2][0].Image = Graphics2D.LoadFromAssets("Conflagaration.png");
			Items[2][0].Enabled = true;
			
			Items[2][2].AbilityType = typeof(FireRelease);
			Items[2][2].Image = Graphics2D.LoadFromAssets("FireRelease.png");
			Items[2][2].Enabled = true;
			
			Items[2][3].AbilityType = typeof(FireRelease);
			Items[2][3].Image = Graphics2D.LoadFromAssets("Conflagaration.png");
			Items[2][3].Enabled = true;
		}
	}
}
