using System;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects;

public abstract class BonusComponent : Component<IHumanoid>
{
    private readonly float _value;

    protected BonusComponent(IHumanoid Parent, float Value) : base(Parent)
    {
        _value = Value;
        AddValue(_value);
    }

    public override void Update()
    {
    }

    protected abstract void AddValue(float Value);

    public override void Dispose()
    {
        if (Disposed) return;
        AddValue(-_value);
        base.Dispose();
    }
}