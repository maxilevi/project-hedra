/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Rogue
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class Venom : PassiveSkill
    {
        private PoisonousComponent _component;

        protected override void OnChange()
        {
            if (Player.SearchComponent<PoisonousComponent>() == null)
            {
                _component = new PoisonousComponent(Player);
                Player.AddComponent(_component);
            }
            var poison = Player.SearchComponent<PoisonousComponent>();
            poison.Chance = (int) (100 * (Math.Min(.4f, base.Level * .05f) + .2f));
            poison.Damage = Level * 7.5f + 20f;
            poison.Duration = 8f - Math.Min(4f, base.Level * .5f);        
        }

        protected override void Remove()
        {
            Player.RemoveComponent(_component);
        }

        protected override int MaxLevel => 100;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Venom.png");
        public override string Description => "Your attacks have a chance to apply poison.";
        public override string DisplayName => "Venom";
    }
}
