/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:11 p.m.
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
	/*
	 *    0   1   2
	 * 0  -   -   -
	 * 
	 * 1  -   -   -
	 * 
	 * 2  -   -   -
	 * 
	 * 3  -   -   -
	 * */
	public class FireTreeBlueprint : TreeBlueprint
	{
		
		public FireTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(1.000f, 0.420f, 0.204f, 1.000f);
			
			Slots[0][0].AbilityType = typeof(Fireball);	
			Slots[0][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Fireball.png");
			Slots[0][0].Enabled = true;
			
			Slots[0][1].AbilityType = typeof(TripleFireball);	
			Slots[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/TripleFireball.png");
			Slots[0][1].Enabled = true;
			
			Slots[0][3].AbilityType = typeof(Meteor);	
			Slots[0][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Meteor.png");
			Slots[0][3].Enabled = true;
			
			Slots[1][1].AbilityType = typeof(FlameStyle);
			Slots[1][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameStyle.png");
			Slots[1][1].Enabled = true;
			
			Slots[1][2].AbilityType = typeof(FlameJump);
			Slots[1][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameJump.png");
			Slots[1][2].Enabled = true;
			
			Slots[1][3].AbilityType = typeof(FlameDisk);
			Slots[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameDisk.png");
			Slots[1][3].Enabled = true;
			
			Slots[2][0].AbilityType = typeof(Conflagaration);
			Slots[2][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Conflagaration.png");
			Slots[2][0].Enabled = true;
			
			Slots[2][2].AbilityType = typeof(FireRelease);
			Slots[2][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/FireRelease.png");
			Slots[2][2].Enabled = true;
			
			//Slots[2][3].AbilityType = typeof(FireRelease);
			//Slots[2][3].Image = Graphics2D.LoadFromAssets("Conflagaration.png");
			//Slots[2][3].Enabled = true;
		}
	}
}
