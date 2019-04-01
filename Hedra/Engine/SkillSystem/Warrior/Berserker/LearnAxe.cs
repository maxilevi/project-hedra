/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnAxe : LearningSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Axe.png");
        
        protected override EquipmentType Equipment => EquipmentType.Axe;
        
        protected override int RestrictionIndex => PlayerInventory.WeaponHolder;
        
        public override string Description => Translations.Get("learn_axe_desc");
        
        public override string DisplayName => Translations.Get("learn_axe");
    }
}
