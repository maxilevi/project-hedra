using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave6Design : CaveWithBossDesign
{
    public override int StructureChance => StructureGrid.Cave6Chance;
    protected override CacheItem? Cache => CacheItem.Cave6;
    protected override Vector3 StructureOffset => Cave6Cache.Offset;
    protected override string BaseFileName => "Cave6";
    protected override int Level => 28;

}