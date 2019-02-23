using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class SteadyAim : ChargeablePassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteadyAim.png");

        protected override ChargeableComponent CreateComponent()
        {
            return new SteadyAimComponent(Player, ChargeTime, RangeBonus);
        }    
                
        public override string Description => Translations.Get("steady_aim_desc");
        public override string DisplayName => Translations.Get("steady_aim");
        protected override int MaxLevel => 15;
        private float ChargeTime => 5;//10.5f - Level / 2f;
        private float RangeBonus => 1;
        
        private class SteadyAimComponent : ChargeableComponent
        {
            private readonly float _precisionBonus;
            
            public SteadyAimComponent(IHumanoid Entity, float ChargeTime, float PrecisionBonus) : base(Entity, ChargeTime)
            {
                _precisionBonus = PrecisionBonus;
            }

            protected override bool ShouldCharge()
            {
                return Parent.IsMoving;
            }

            protected override void Apply(AttackOptions Options)
            {
                
            }
        }

        public override string[] Attributes => new[]
        {
            Translations.Get("steady_wait_change", ChargeTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("steady_precision_change", (int)(RangeBonus * 100)),
        };
    }
}