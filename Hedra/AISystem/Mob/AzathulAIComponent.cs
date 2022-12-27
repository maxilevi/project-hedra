using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob;

public class AzathulAIComponent : BasicAIComponent, IGuardAIComponent
{
    public AzathulAIComponent(IEntity Entity) : base(Entity)
    {
        Retaliate = new RetaliateBehaviour(Parent);
        Hostile = new HostileBehaviour(Parent);
        AlterBehaviour<AttackBehaviour>(new AzathulAttackBehaviour(Entity));
    }

    protected RetaliateBehaviour Retaliate { get; }
    protected HostileBehaviour Hostile { get; }

    public override AIType Type => AIType.Hostile;

    public override void Update()
    {
        if (Retaliate.Enabled)
            Retaliate.Update();
        else
            Hostile.Update();
    }

    public override void Dispose()
    {
        base.Dispose();
        Hostile.Dispose();
        Retaliate.Dispose();
    }

    public Vector3 GuardPosition { get; set; }
}