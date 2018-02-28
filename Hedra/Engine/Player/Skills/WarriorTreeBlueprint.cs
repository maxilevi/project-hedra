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
	public class WarriorTreeBlueprint : TreeBlueprint
	{
		
		public WarriorTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(1.000f, 0.420f, 0.204f, 1.000f);
			
			Slots[0][0].AbilityType = typeof(WeaponThrow);	
			Slots[0][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Throw.png");
			Slots[0][0].Enabled = true;
			
			Slots[0][1].AbilityType = typeof(Whirlwind);	
			Slots[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
			Slots[0][1].Enabled = true;
			
			//Slots[0][2].AbilityType = typeof(AttackLeap);	
			//Slots[0][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/AttackLeap.png");
			//Slots[0][2].Enabled = true;
			
			Slots[1][0].AbilityType = typeof(Bash);	
			Slots[1][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Bash.png");
			Slots[1][0].Enabled = true;
			
			Slots[1][3].AbilityType = typeof(LearnAxe);	
			Slots[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Axe.png");
			Slots[1][3].Enabled = true;
			
			Slots[1][1].AbilityType = typeof(LearnHammer);	
			Slots[1][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Hammer.png");
			Slots[1][1].Enabled = true;
			
			//Slots[1][3].AbilityType = typeof(LearnDoubleSwords);	
			//Slots[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/DoubleSwords.png");
			//Slots[1][3].Enabled = true;
			
			Slots[2][1].AbilityType = typeof(Resistance);	
			Slots[2][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
			Slots[2][1].Enabled = true;
		}
	}
}
