/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnHammer : LearningSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Hammer.png");
        
        protected override void Learn()
        {
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Hammer);
        }
        
        public override string Description => "Learn to use the hammer.";
        public override string DisplayName => "Learn Hammer";
    }
}
