using System.Numerics;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.SkillSystem.Mage.Necromancer;
using Hedra.EntitySystem;
using Hedra.Localization;
using SixLabors.ImageSharp;

namespace Hedra.AnimationEvents;

public abstract class SpawnMinion : AnimationEvent
{
    public SpawnMinion(ISkilledAnimableEntity Parent) : base(Parent)
    {
    }

    public override void Build()
    {
        var targetPosition = Parent.Position + Parent.Orientation * 16;
        Parent.Physics.StaticRaycast(targetPosition, out var hitPosition);
        var minion = CreateMinion(hitPosition);
        minion.Position = hitPosition;
        //minion.AddComponent(new HostileAIComponent(minion));
        minion.Level = Parent.Level / 2;
        minion.SearchComponent<DamageComponent>()
            .Ignore(E => E == Parent || E.SearchComponent<DamageComponent>().HasIgnoreFor(Parent));
        minion.RemoveComponent(minion.SearchComponent<HealthBarComponent>());
        minion.AddComponent(new HealthBarComponent(minion, minion.Name, HealthBarType.Black, Color.FromRgb(40, 40, 40)));
        minion.AddComponent(new RaiseSkeleton.SkeletonEffectComponent(minion));
        minion.Physics.CollidesWithEntities = false;
        minion.RemoveComponentsOfType<DropComponent>();
        minion.AddComponent(new SelfDestructComponent(minion, () => Parent.IsDead));
        minion.SearchComponent<DamageComponent>().Ignore(E => E == minion);
    }

    protected abstract IEntity CreateMinion(Vector3 Position);
}