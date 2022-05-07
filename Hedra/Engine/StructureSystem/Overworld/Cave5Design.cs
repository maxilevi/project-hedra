using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave5Design : CaveWithBossDesign
{
    public override int StructureChance => StructureGrid.Cave5Chance;
    protected override CacheItem? Cache => CacheItem.Cave5;
    protected override Vector3 StructureOffset => Cave5Cache.Offset;
    protected override string BaseFileName => "Cave5";
    protected override int Level => 17;

}