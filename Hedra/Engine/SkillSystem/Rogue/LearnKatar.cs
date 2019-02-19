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

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class LearnKatar : LearningSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Katar.png");

        protected override int RestrictionIndex => PlayerInventory.WeaponHolder;
        
        protected override EquipmentType Equipment => EquipmentType.Katar;
        
        public override string Description => Translations.Get("learn_katar_desc");
        
        public override string DisplayName => Translations.Get("learn_katar");
    }
}