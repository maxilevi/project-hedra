using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave3Design : CaveWithBossDesign
{
    public override int StructureChance => StructureGrid.Cave3Chance;
    protected override CacheItem? Cache => CacheItem.Cave3;
    protected override Vector3 StructureOffset => Cave3Cache.Offset;
    protected override string BaseFileName => "Cave3";
    protected override int Level => 21;

}