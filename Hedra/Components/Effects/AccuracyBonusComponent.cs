using Hedra.EntitySystem;

namespace Hedra.Components.Effects;

public class AccuracyBonusComponent : BonusComponent
{
    public AccuracyBonusComponent(IHumanoid Parent, float Value) : base(Parent, Value)
    {
    }
        
    protected override void AddValue(float Value)
    {
        Parent.Accuracy += Value;
    }
}