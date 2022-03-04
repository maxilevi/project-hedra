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
    
    protected override Vector3 StructureScale => Vector3.One;
    public override VertexData Icon => CacheManager.GetModel(CacheItem.CaveIcon);
    
    protected override void ApplyColors(VertexData Model, RegionColor Colors)
    {
        Model.Color(AssetManager.ColorCode0, Colors.StoneColor * 0.6f);
        Model.Color(AssetManager.ColorCode1, Colors.GrassColor * 0.8f);
        Model.Color(AssetManager.ColorCode2, Colors.StoneColor * 0.3f);
        Model.Color(AssetManager.ColorCode3, Colors.StoneColor * 0.8f);
        Model.GraduateColor(Vector3.UnitY, 0.15f);
    }
    
    protected override IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure)
    {
        const HumanType type = HumanType.BeasthunterSpirit;
        var boss = NPCCreator.SpawnBandit(Position, ((CaveWithBossDesign)Structure.Design).Level,
            new BanditOptions
            {
                ModelType = type,
                Friendly = false,
                PossibleClasses = Class.Warrior | Class.Rogue | Class.Mage
            });
        boss.Position = Position;
        var template = HumanoidLoader.HumanoidTemplater[type];
        BossGenerator.MakeBoss(boss, Position, template.XP);
        boss.BonusHealth = boss.MaxHealth * (1.5f + Utils.Rng.NextFloat());
        boss.Health = boss.MaxHealth;
        var currentWeapon = boss.MainWeapon;
        boss.MainWeapon = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, currentWeapon.EquipmentType)
        {
            RandomizeTier = false
        });
        return boss;
    }
}