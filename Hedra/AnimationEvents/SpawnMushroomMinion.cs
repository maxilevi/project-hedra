using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;

namespace Hedra.AnimationEvents;

public class SpawnMushroomMinion : SpawnMinion
{
    protected override IEntity CreateMinion(Vector3 Position)
    {
        var mobSeed = Utils.Rng.Next(int.MaxValue);
        var type = Utils.Rng.NextBool() ? MobType.MushroomHound : MobType.MushroomHoundBiped;
        return World.SpawnMob(type.ToString(), Position, mobSeed);
    }

    public SpawnMushroomMinion(ISkilledAnimableEntity Parent) : base(Parent)
    {
    }
}