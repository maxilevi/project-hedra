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

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of ArcherLearnKnife.
    /// </summary>
    public class LearnKnife : LearningSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/LearnKnife.png");
        
        protected override int RestrictionIndex => PlayerInventory.WeaponHolder;
        
        protected override EquipmentType Equipment => EquipmentType.Knife;
        
        public override string Description => Translations.Get("learn_knife_desc");
        
        public override string DisplayName => Translations.Get("learn_knife");

    }
}