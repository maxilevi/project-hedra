using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class ArmorBonusComponent : BonusComponent
    {
        public ArmorBonusComponent(IHumanoid Parent, float Armor) : base(Parent, Armor)
        {
        }
        
        protected override void AddValue(float Value)
        {
            Parent.Armor += Value;
        }
    }
}