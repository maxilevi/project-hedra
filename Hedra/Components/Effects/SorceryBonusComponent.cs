using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class SorceryBonusComponent : BonusComponent
    {
        public SorceryBonusComponent(IHumanoid Parent, float Sorcery) : base(Parent, Sorcery)
        {
        }
        
        protected override void AddValue(float Value)
        {
            Parent.Sorcery += Value;
        }
    }
}