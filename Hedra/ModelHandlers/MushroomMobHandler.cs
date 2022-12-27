using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.ModelHandlers;

public class MushroomMobHandler : ModelHandler
{
    public override void Process(IEntity Mob, AnimatedUpdatableModel Model)
    {
        var region = World.BiomePool.GetRegion(Mob.Position);
        var woodColor = region.Colors.WoodColors[Utils.Rng.Next(0, region.Colors.WoodColors.Length)];
        var leafColor = region.Colors.LeavesColors[Utils.Rng.Next(0, region.Colors.LeavesColors.Length)];
        
        Model.Paint(leafColor, woodColor, leafColor * .75f, woodColor * .75f);
    }
}