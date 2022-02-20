using System;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem;

public class LandformPlacer
{
    
    public static Landform Sample(Vector2 Offset, RandomDistribution Rng)
    {
        if(!CanPlace(Offset, Rng)) return null;
        var landform = LandformLoader.New(Offset, (LandformType) Rng.Next(0, (int)LandformType.MaxLandforms));
        World.WorldBuilding.AddLandform(landform);
        return landform;
    }

    private static bool CanPlace(Vector2 Offset, RandomDistribution Rng)
    {
        Rng.Seed = GetSeed(Offset) + 1;
        return (int)Math.Abs(Offset.X % 11) == 4 && Math.Abs((int)Offset.Y % 7) == 6 && Rng.Next(0, 4) == 1;
    }
    
    private static int GetSeed(Vector2 Position)
    {
        unchecked
        {
            var seed = 17;
            seed = seed * 31 + Position.X.GetHashCode();
            seed = seed * 31 + Position.Y.GetHashCode();
            seed = seed * 31 + World.Seed.GetHashCode();
            return seed;
        }
    }
}