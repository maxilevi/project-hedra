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

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnHammer : LearningSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Hammer.png");
        
        protected override EquipmentType Equipment => EquipmentType.Hammer;
        
        protected override int RestrictionIndex => PlayerInventory.WeaponHolder;
        
        public override string Description => Translations.Get("learn_hammer_desc");
        
        public override string DisplayName => Translations.Get("learn_hammer");
    }
}
