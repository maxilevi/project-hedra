/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Warrior
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class Resistance : PassiveSkill
    {
        private float _addonHealth;

        private float HealthFormula(bool Clamp = false)
        {
            return Clamp ? 12 * Math.Max(Level, 1) : 12 * Level;
        }
        
        protected override void OnChange()
        {
            Player.AddonHealth -= _addonHealth;
            _addonHealth = HealthFormula() * Level;
            Player.AddonHealth += _addonHealth;
            if(Player.Health > Player.MaxHealth) Player.Health = Player.MaxHealth;
        }

        protected override void Remove()
        {
            /* Health will be resetted when changing the character */
        }

        public override void Load()
        {
            Player.AddonHealth += _addonHealth;
        }
        
        public override void Unload()
        {
            Player.AddonHealth -= _addonHealth;
        }

        protected override int MaxLevel => 10;
        
        public override string Description => $"Grants +{HealthFormula(true):0.0} HP.";
        public override string DisplayName => "Resistance";
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
    }
}
