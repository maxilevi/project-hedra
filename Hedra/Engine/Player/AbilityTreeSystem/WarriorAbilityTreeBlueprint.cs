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
	public class WarriorAbilityTreeBlueprint : AbilityTreeBlueprint
	{
		
		public WarriorAbilityTreeBlueprint() : base(){
			base.ActiveColor = new Vector4(1.000f, 0.420f, 0.204f, 1.000f);
			
			Items[0][0].AbilityType = typeof(WeaponThrow);	
			Items[0][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Throw.png");
			Items[0][0].Enabled = true;
			
			Items[0][1].AbilityType = typeof(Whirlwind);	
			Items[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
			Items[0][1].Enabled = true;
			
			//Slots[0][2].AbilityType = typeof(AttackLeap);	
			//Slots[0][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/AttackLeap.png");
			//Slots[0][2].Enabled = true;
			
			Items[1][0].AbilityType = typeof(Bash);	
			Items[1][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Bash.png");
			Items[1][0].Enabled = true;
			
			Items[1][3].AbilityType = typeof(LearnAxe);	
			Items[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Axe.png");
			Items[1][3].Enabled = true;
			
			Items[1][1].AbilityType = typeof(LearnHammer);	
			Items[1][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Hammer.png");
			Items[1][1].Enabled = true;
			
			//Slots[1][3].AbilityType = typeof(LearnDoubleSwords);	
			//Slots[1][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/DoubleSwords.png");
			//Slots[1][3].Enabled = true;
			
			Items[2][1].AbilityType = typeof(WarriorResistance);	
			Items[2][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
			Items[2][1].Enabled = true;
		}
	}
}
