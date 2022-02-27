using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave0Design : CaveWithBossDesign
{
    public override int PlateauRadius => 480;
    public override int StructureChance => 1000;//StructureGrid.Dungeon1Chance;
    protected override CacheItem? Cache => CacheItem.Cave0;
    protected override Vector3 StructureOffset => Cave0Cache.Offset;
    protected override float GroundworkRadius => 256;
    protected override string BaseFileName => "Cave0";
    protected override int Level => 17;  
}