using System;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.AISystem.Mob;
using Hedra.API;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0Design : DungeonWithBossDesign
    {
        public override int PlateauRadius => 300;
        protected override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;
        protected override Vector3 StructureOffset => Dungeon0Cache.Offset;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        protected override float GroundworkRadius => 180;
        protected override string BaseFileName => "Dungeon0";
        protected override SceneSettings Scene => Settings;
        protected override int Level => 13;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            /* Office */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door0.ply", Vector3.One), Dungeon0Cache.Doors[0], Rotation, Structure, true, false);
            /* Entrance */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door1.ply", Vector3.One), Dungeon0Cache.Doors[1], Rotation, Structure, false, false);
            /* Lever room pathway */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door2.ply", Vector3.One), Dungeon0Cache.Doors[2], Rotation, Structure, true, true);
            /* Lever room */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door3.ply", Vector3.One), Dungeon0Cache.Doors[3], Rotation, Structure, true, true);
            /* Boss room pathway */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door4.ply", Vector3.One), Dungeon0Cache.Doors[4], Rotation, Structure, true, true);
            /* Boss room doors */
            var bossDoor0 = AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door5.ply", Vector3.One), Dungeon0Cache.Doors[5], Rotation, Structure, true, false);
            var bossDoor1 = AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door6.ply", Vector3.One), Dungeon0Cache.Doors[6], Rotation, Structure, false, true);

            bossDoor0.IsLocked = true;
            bossDoor1.IsLocked = true;
            var lever = AddLever(Structure, Dungeon0Cache.Lever0, Rotation);
            lever.OnActivate += _ =>
            {
                bossDoor0.IsLocked = false;
                bossDoor1.IsLocked = false;
                SoundPlayer.PlaySound(SoundType.Door, lever.Position);
            };
        }

        private static IEntity SkeletonBoss(Vector3 Position, CollidableStructure Structure)
        {
            var boss = BossGenerator.Generate(new []{MobType.SkeletonKing}, Position, Utils.Rng);
            boss.Position = Position;
            boss.AddComponent(new IsDungeonMemberComponent(boss));
            boss.AddComponent(new DropComponent(boss)
            {
                ItemDrop = Utils.Rng.Next(0, 3) == 1 ? ItemPool.Grab(ItemTier.Unique) : ItemPool.Grab(ItemTier.Rare),
                DropChance = Utils.Rng.NextFloat() * 25f + 75f
            });
            var bossBar = boss.SearchComponent<BossHealthBarComponent>();
            bossBar.ViewRange = 80;
            bossBar.Enabled = false;
            ((DungeonWithBoss) Structure.WorldObject).Boss = boss;
            Structure.WorldObject.Search<DungeonBossRoomTrigger>().Boss = boss;
            return boss;
        }

        private static SceneSettings Settings { get; } = new SceneSettings
        {
            LightRadius = Torch.DefaultRadius * 2,
            LightColor = WorldLight.DefaultColor * 2,
            IsNightLight = false,
            Structure1Creator = BuildDungeonDoorTrigger,
            Structure2Creator = BuildBossRoomTrigger,
            Structure3Creator = AddRewardChest,
            Structure4Creator = (P, _) => new Torch(P),
            Npc1Creator = DungeonSkeleton,
            Npc2Creator = DungeonSkeleton,
            Npc3Creator = SkeletonBoss,
        };
    }
}