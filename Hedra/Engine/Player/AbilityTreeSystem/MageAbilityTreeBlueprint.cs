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
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Mage;
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
	public class MageAbilityTreeBlueprint : AbilityTreeBlueprint
	{
		
		public MageAbilityTreeBlueprint()
		{

			Items[0][0].AbilityType = typeof(Fireball);	
			Items[0][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Fireball.png");
			Items[0][0].Enabled = true;
			
			Items[0][1].AbilityType = typeof(TripleFireball);	
			Items[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/TripleFireball.png");
			Items[0][1].Enabled = true;
			
			Items[0][3].AbilityType = typeof(Meteor);	
			Items[0][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Meteor.png");
			Items[0][3].Enabled = true;
			
			Items[1][1].AbilityType = typeof(FlameStyle);
			Items[1][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameStyle.png");
			Items[1][1].Enabled = true;
			/*
			Items[1][2].AbilityType = typeof(FlameJump);
			Items[1][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameJump.png");
			Items[1][2].Enabled = true;
			
			Items[1][3].AbilityType = typeof(FlameDisk);
			Items[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/FlameDisk.png");
			Items[1][3].Enabled = true;*/
			
			Items[2][0].AbilityType = typeof(Conflagaration);
			Items[2][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Conflagaration.png");
			Items[2][0].Enabled = true;
			
			Items[2][2].AbilityType = typeof(FireRelease);
			Items[2][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/FireRelease.png");
			Items[2][2].Enabled = true;
			
			//Slots[2][3].AbilityType = typeof(FireRelease);
			//Slots[2][3].Image = Graphics2D.LoadFromAssets("Conflagaration.png");
			//Slots[2][3].Enabled = true;
		}
	}
}
