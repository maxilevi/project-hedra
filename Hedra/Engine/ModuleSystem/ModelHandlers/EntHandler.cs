using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Engine.ModuleSystem.ModelHandlers
{
    public class EntHandler : ModelHandler
    {
        public override void Process(IEntity Mob, AnimatedUpdatableModel Model)
        {
            var region = World.BiomePool.GetRegion(Mob.Position);
            var woodColor = region.Colors.WoodColors[Utils.Rng.Next(0, region.Colors.WoodColors.Length)];
            Model.Paint(new []
            {
                woodColor,
                region.Colors.LeavesColors[Utils.Rng.Next(0, region.Colors.LeavesColors.Length)],
                woodColor * .65f
            });
        }
    }
}