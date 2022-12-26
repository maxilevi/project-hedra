using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PlantSystem;

public class MushroomGrassDesign : WeedDesign
{
    public override CacheItem Type => CacheItem.MushroomGrass;

    protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
    {
        Data.Color(AssetManager.ColorCode0, Region.Colors.WoodColor);
        Data.Color(AssetManager.ColorCode1, Region.Colors.LeavesColor);
    }
}