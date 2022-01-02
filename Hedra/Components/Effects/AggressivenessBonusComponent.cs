using Hedra.EntitySystem;

namespace Hedra.Components.Effects;

public class AggressivenessBonusComponent : BonusComponent
{
    public AggressivenessBonusComponent(IHumanoid Parent, float Value) : base(Parent, Value)
    {
    }
        
    protected override void AddValue(float Value)
    {
        Parent.Aggressiveness += Value;
    }
}