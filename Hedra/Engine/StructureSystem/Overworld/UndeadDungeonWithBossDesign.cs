using System;
using System.Linq;
using System.Numerics;
using Hedra.AISystem.Humanoid;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class UndeadDungeonWithBossDesign : BaseDungeonDesign<DungeonWithBoss>
    {
        protected override DungeonWithBoss Create(Vector3 Position, float Size)
        {
            return new DungeonWithBoss(Position);
        }
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, $"Assets/Env/Structures/Dungeon/{BaseFileName}.ply", Vector3.One, Rotation * Translation, Settings);
            TaskScheduler.When(
                () => ((DungeonWithBoss) Structure.WorldObject).Boss != null,
                () => MakeChestOpenWithBossDeath(Structure)
            );
            ((DungeonWithBoss)Structure.WorldObject).BuildingTrigger = (DungeonDoorTrigger) Structure.WorldObject.Children.FirstOrDefault(T => T is DungeonDoorTrigger);
            Structure.Waypoints = WaypointLoader.Load($"Assets/Env/Structures/Dungeon/{BaseFileName}-Pathfinding.ply", Vector3.One, Rotation * Translation);
        }

        private void MakeChestOpenWithBossDeath(CollidableStructure Structure)
        {
            var children = Structure.WorldObject.Children;
            var boss = ((DungeonWithBoss) Structure.WorldObject).Boss;
            for (var i = 0; i < children.Length; ++i)
            {
                if (children[i] is Chest chest && (chest.Position - boss.Position).LengthFast() < 48)
                {
                    chest.Condition = () => StructureContentHelper.IsNotNearLookingEnemies(chest.Position) && boss.IsDead;
                }
            }
        }

        protected abstract string BaseFileName { get; }

        protected abstract IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure);

        protected static IEntity BossCreator(Vector3 Position, CollidableStructure Structure)
        {
            var design = (UndeadDungeonWithBossDesign) Structure.Design;
            var boss = design.CreateDungeonBoss(Position, Structure);
            boss.AddComponent(new IsStructureMemberComponent(boss));
            boss.AddComponent(new DropComponent(boss)
            {
                ItemDrop = Utils.Rng.Next(0, 7) == 1 ? ItemPool.Grab(ItemTier.Unique) : Utils.Rng.Next(0, 5) == 1 ? ItemPool.Grab(ItemTier.Rare) : ItemPool.Grab(ItemTier.Uncommon),
                DropChance = Utils.Rng.NextFloat() * 25f + 75f
            });
            var bossBar = boss.SearchComponent<BossHealthBarComponent>();
            bossBar.ViewRange = 80;
            bossBar.Enabled = false;
            ((DungeonWithBoss) Structure.WorldObject).Boss = boss;
            Structure.WorldObject.SearchFirst<DungeonBossRoomTrigger>().Boss = boss;
            AddImmuneTag(boss);
            return boss;
        }

        protected static IEntity DungeonSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var skeleton = default(IEntity);
            var spawnKamikazeSkeleton = Utils.Rng.Next(1, 7) == 1;
            var spawnGladiatorSkeleton = !spawnKamikazeSkeleton && Utils.Rng.Next(1, 7) == 1;
            skeleton = spawnKamikazeSkeleton
                ? SpawnKamikazeSkeleton(Position, Structure)
                : spawnGladiatorSkeleton
                    ? SpawnGladiatorSkeleton(Position, Structure)
                    : NormalSkeleton(Position, Structure);
            skeleton.Position = Position;
            AddImmuneTag(skeleton);
            return skeleton;
        }

        protected static IEntity SpawnKamikazeSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var mob = World.SpawnMob(MobType.SkeletonKamikaze, Position, Utils.Rng);
            mob.Position = Position;
            return mob;
        }
        
        protected static IEntity SpawnGladiatorSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var mob = World.SpawnMob(MobType.Skeleton, Position, Utils.Rng);
            mob.Position = Position;
            return mob;
        }

        protected static IHumanoid NormalSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var skeleton = NPCCreator.SpawnBandit(Position, ((UndeadDungeonWithBossDesign)Structure.Design).Level, BanditOptions.Undead);
            skeleton.Physics.CollidesWithEntities = false;
            skeleton.SearchComponent<CombatAIComponent>().SetCanExplore(Value: false);
            skeleton.SearchComponent<CombatAIComponent>().SetGuardSpawnPoint(Value: false);
            skeleton.Position = Position;
            return skeleton;
        }

        private static Item CreateItemForChest()
        {
            return ItemPool.Grab(Utils.Rng.Next(0, 5) == 1 ? ItemTier.Rare : ItemTier.Uncommon);
        }

        private static SceneSettings Settings { get; } = new SceneSettings
        {
            LightRadius = Torch.DefaultRadius * 2,
            LightColor = WorldLight.DefaultColor * 2,
            IsNightLight = false,
            Structure1Creator = BuildDungeonDoorTrigger,
            Structure2Creator = BuildBossRoomTrigger,
            Structure3Creator = (P, M) => StructureContentHelper.AddRewardChest(P, M, CreateItemForChest()),
            Structure4Creator = (P, _) => new Torch(P),
            Npc1Creator = DungeonSkeleton,
            Npc2Creator = DungeonSkeleton,
            Npc3Creator = BossCreator,
        };
    }
}