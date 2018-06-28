/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	internal class LearnAxe : LearningSkill
	{
	    public override uint TexId => Graphics2D.LoadFromAssets("Assets/Skills/Axe.png");
		public override void Learn()
		{
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Axe);
		}
		
		public override string Description => "Learn to use the axe.";
	}
}
