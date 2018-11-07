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
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Warrior;
using OpenTK;
namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of WaterTreeBlueprint.
    /// </summary>
    public class WarriorAbilityTreeBlueprint : AbilityTreeBlueprint
    {
        
        public WarriorAbilityTreeBlueprint()
        {
            Items[0][1].AbilityType = typeof(Whirlwind);    
            Items[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
            Items[0][1].Enabled = true;

            Items[0][2].AbilityType = typeof(Intercept);
            Items[0][2].Image = Graphics2D.LoadFromAssets("Assets/Skills/Intercept.png");
            Items[0][2].Enabled = true;

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
            
            Items[2][1].AbilityType = typeof(Resistance);    
            Items[2][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[2][1].Enabled = true;
            
            Items[2][1].AbilityType = typeof(Resistance);    
            Items[2][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[2][1].Enabled = true;
            
            Items[2][3].AbilityType = typeof(Resistance);    
            Items[2][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[2][3].Enabled = true;
            
            Items[2][3].AbilityType = typeof(Resistance);    
            Items[2][3].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[2][3].Enabled = true;
            
            Items[2][4].AbilityType = typeof(Resistance);    
            Items[2][4].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[2][4].Enabled = true;
            
/*            
            Items[3][5].AbilityType = typeof(Resistance);    
            Items[3][5].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[3][5].Enabled = true;
            
            Items[4][5].AbilityType = typeof(Resistance);    
            Items[4][5].Image = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
            Items[4][5].Enabled = true;*/
        }
    }
}
