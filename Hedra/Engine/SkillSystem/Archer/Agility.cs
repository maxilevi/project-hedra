/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer
{
    public class Agility : PassiveSkill
    {
        private float StaminaFormula(bool Clamp = false)
        {
            if(Clamp)
                return -1f * Math.Max(Level,1);
            return -1f * Level;
        }

        protected override void OnChange()
        {
            Player.DodgeCost = Humanoid.DefaultDodgeCost + StaminaFormula();
        }

        protected override void Remove()
        {
            Player.DodgeCost = Humanoid.DefaultDodgeCost;
        }

        protected override int MaxLevel => 10;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Agility.png");
        public override string Description => Translations.Get("agility_skill_desc", -StaminaFormula(true));
        public override string DisplayName => Translations.Get("agility_skill");
    }
}
