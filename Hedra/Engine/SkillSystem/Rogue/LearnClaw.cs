/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnClaw : LearningSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Claw.png");

        protected override EquipmentType Equipment => EquipmentType.Claw;
        
        protected override int RestrictionIndex => PlayerInventory.WeaponHolder;
        
        public override string Description => Translations.Get("learn_claw_desc");
        
        public override string DisplayName => Translations.Get("learn_claw");
    }
}