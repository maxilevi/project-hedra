using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave1Design : CaveWithBossDesign
{
    public override int StructureChance => StructureGrid.Cave1Chance;
    protected override CacheItem? Cache => CacheItem.Cave1;
    protected override Vector3 StructureOffset => Cave1Cache.Offset;
    protected override string BaseFileName => "Cave1";
    protected override int Level => 12;

}