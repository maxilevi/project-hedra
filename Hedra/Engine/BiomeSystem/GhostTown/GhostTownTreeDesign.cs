using Hedra.BiomeSystem;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownTreeDesign : BiomeTreeDesign
    {
        public override TreeDesign[] AvailableTypes { get; } =
        {
            new DeadTreeDesign()
        };
    }
}