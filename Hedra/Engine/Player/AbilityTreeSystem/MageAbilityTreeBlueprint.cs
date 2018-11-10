/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Player.Skills.Mage;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.AbilityTreeSystem
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
            Items[0][1].AbilityType = typeof(FireRelease);
            Items[0][1].Image = Graphics2D.LoadFromAssets("Assets/Skills/FireRelease.png");
            Items[0][1].Enabled = true;
            
            Items[2][0].AbilityType = typeof(Meditation);
            Items[2][0].Image = Graphics2D.LoadFromAssets("Assets/Skills/Meditation.png");
            Items[2][0].Enabled = true;
        }
    }
}
