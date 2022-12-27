using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.ShroomDimension;
using Hedra.Numerics;

namespace Hedra.Structures;

public class SpawnShroomDimensionPortalDesign : ShroomDimensionPortalDesign
{
    public override int PlateauRadius => 180;
    protected override bool SpawnNpc => false;
    public static bool Spawned { get; set; }
    public override bool IsFixed => true;

    public static Vector3 Position =>
        World.SpawnPoint + Vector3.One.Xz().ToVector3() * 1024f;

    protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
    {
        Spawned = true;
        return base.Setup(TargetPosition, Rng);
    }

    public override bool ShouldSetup(Vector2 ChunkOffset, ref Vector3 TargetPosition, CollidableStructure[] Items,
        Region Biome, IRandom Rng)
    {
        return ChunkOffset == World.ToChunkSpace(Position) && !Spawned;
    }

    protected override ShroomDimensionPortal Create(Vector3 TargetPosition, float Size)
    {
        return new SpawnShroomDimensionPortal(Position+ Vector3.UnitY * 8f, StructureScale * 10f);
    }
}

public class SpawnShroomDimensionPortal : ShroomDimensionPortal
{
    public SpawnShroomDimensionPortal(Vector3 Position, Vector3 Scale) : base(Position,
        Scale, RealmHandler.Overworld)
    {
    }
    
    public override void Dispose()
    {
        base.Dispose();
        SpawnShroomDimensionPortalDesign.Spawned = false;
    }
}