/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Rogue
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnClaw : LearningSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Claw.png");

        protected override void Learn()
        {
            Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentType.Claw);
        }
        
        public override string Description => "Learn to use the claws.";
        public override string DisplayName => "Learn Claw";
    }
}