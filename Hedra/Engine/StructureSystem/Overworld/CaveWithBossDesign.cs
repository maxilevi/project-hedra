using System;
using System.Numerics;
using Hedra.API;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld;

public abstract class CaveWithBossDesign : DarkStructureWithBossDesign
{
    protected override string FolderName => "Caves";
    protected override BlockType PathType => BlockType.Grass;
    protected override bool NoPlantsZone => true;
    protected override bool NoTreesZone => true;
    public override int PlateauRadius => 480;
    protected override float GroundworkRadius => 256;
    
    protected override Vector3 StructureScale => Vector3.One;
    public override VertexData Icon => CacheManager.GetModel(CacheItem.CaveIcon);

    protected override SceneSettings Settings
    {
        get
        {
            var @base = base.Settings;
            @base.Npc2Creator = RangedSkeleton;
            return @base;
        }
    }
    
    protected override void ApplyColors(VertexData Model, RegionColor Colors)
    {
        Model.Color(AssetManager.ColorCode0, Colors.StoneColor * 0.6f);
        Model.Color(AssetManager.ColorCode1, Colors.GrassColor * 0.8f);
        Model.Color(AssetManager.ColorCode2, Colors.StoneColor * 0.3f);
        Model.Color(AssetManager.ColorCode3, Colors.StoneColor * 0.8f);
        Model.GraduateColor(Vector3.UnitY, 0.25f);
    }

    protected override IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure)
    {
        var rng = BuildRng(Structure);
        var level = ((CaveWithBossDesign)Structure.Design).Level;
        var funcs = new Func<Vector3, int, Random, IEntity>[]
        {
            BossGenerator.CreateBeasthunterBoss,
            BossGenerator.CreateSkeletonKingBoss,
            BossGenerator.CreateGolemBoss,
            //BossGenerator.CreateGhostBoss
        };
        return funcs[rng.Next(0, funcs.Length)](Position, level, rng);
    }
}