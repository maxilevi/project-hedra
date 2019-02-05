/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of ArcherLearnKnife.
    /// </summary>
    public class LearnKnife : LearningSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/LearnKnife.png");
        
        protected override void Learn()
        {
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Knife);
        }

        public override string Description => "Learn to use the knife.";
        public override string DisplayName => "Learn Knife";

    }
}