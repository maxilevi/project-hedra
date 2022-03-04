using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave2Design : CaveWithBossDesign
{
    public override int PlateauRadius => 480;
    public override int StructureChance => 1000;//StructureGrid.Dungeon1Chance;
    protected override CacheItem? Cache => CacheItem.Cave2;
    protected override Vector3 StructureOffset => Cave2Cache.Offset;
    protected override float GroundworkRadius => 256;
    protected override string BaseFileName => "Cave2";
    protected override int Level => 17;

}