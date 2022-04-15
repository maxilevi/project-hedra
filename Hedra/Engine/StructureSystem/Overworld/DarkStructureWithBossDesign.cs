using System;
using System.Linq;
using System.Numerics;
using Hedra.AISystem.Humanoid;
using Hedra.API;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class DarkStructureWithBossDesign : BaseDungeonDesign<DungeonWithBoss>
    {
        protected abstract string BaseFileName { get; }
        protected abstract string FolderName { get; }

        protected virtual bool HasAmbientHandler => false;

        protected virtual SceneSettings Settings => new SceneSettings
        {
            LightRadius = Torch.DefaultRadius * 2,
            LightColor = WorldLight.DefaultColor * 2,
            IsNightLight = false,
            Structure1Creator = BuildDungeonDoorTrigger,
            Structure2Creator = BuildBossRoomTrigger,
            Structure3Creator = (P, M) => StructureContentHelper.AddRewardChest(P, M, CreateItemForChest(Level)),
            Structure4Creator = (P, _) => new Torch(P),
            Npc1Creator = DungeonSkeleton,
            Npc2Creator = DungeonSkeleton,
            Npc3Creator = BossCreator
        };

        protected override DungeonWithBoss Create(Vector3 Position, float Size)
        {
            return new DungeonWithBoss(Position, Size, HasAmbientHandler);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, $"Assets/Env/Structures/{FolderName}/{BaseFileName}.ply", Vector3.One,
                Rotation * Translation, Settings);
            TaskScheduler.When(
                () => ((DungeonWithBoss)Structure.WorldObject).Boss != null,
                () => MakeChestOpenWithBossDeath(Structure)
            );
            ((DungeonWithBoss)Structure.WorldObject).BuildingTrigger =
                (DungeonDoorTrigger)Structure.WorldObject.Children.FirstOrDefault(T => T is DungeonDoorTrigger);
            Structure.Waypoints = WaypointLoader.Load($"Assets/Env/Structures/{FolderName}/{BaseFileName}-Pathfinding.ply",
                Vector3.One, Rotation * Translation);
        }

        private void MakeChestOpenWithBossDeath(CollidableStructure Structure)
        {
            var children = Structure.WorldObject.Children;
            var boss = ((DungeonWithBoss)Structure.WorldObject).Boss;
            for (var i = 0; i < children.Length; ++i)
                if (children[i] is Chest chest && (chest.Position - boss.Position).LengthFast() < 48)
                    chest.Condition = () =>
                        StructureContentHelper.IsNotNearLookingEnemies(chest.Position) && boss.IsDead;
        }

        protected abstract IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure);

        protected static IEntity BossCreator(Vector3 Position, CollidableStructure Structure)
        {
            var design = (DarkStructureWithBossDesign)Structure.Design;
            var boss = design.CreateDungeonBoss(Position, Structure);
            boss.AddComponent(new IsStructureMemberComponent(boss));
            boss.AddComponent(new DropComponent(boss)
            {
                ItemDrop = Utils.Rng.Next(0, 7) == 1 ? ItemPool.Grab(ItemTier.Unique) :
                    Utils.Rng.Next(0, 5) == 1 ? ItemPool.Grab(ItemTier.Rare) : ItemPool.Grab(ItemTier.Uncommon),
                DropChance = Utils.Rng.NextFloat() * 25f + 75f
            });
            var bossBar = boss.SearchComponent<BossHealthBarComponent>();
            bossBar.ViewRange = 80;
            bossBar.Enabled = false;
            ((DungeonWithBoss)Structure.WorldObject).Boss = boss;
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
            var skeleton = NPCCreator.SpawnBandit(Position, ((DarkStructureWithBossDesign)Structure.Design).Level,
                BanditOptions.Undead);
            ConfigureSkeleton(skeleton, Position);
            return skeleton;
        }
        
        protected static IHumanoid RangedSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var skeleton = NPCCreator.SpawnBandit(Position, ((DarkStructureWithBossDesign)Structure.Design).Level,
                new BanditOptions {
                    Friendly = false,
                    PossibleClasses = Class.Archer | Class.Mage,
                    ModelType = Utils.Rng.Next(0, 7) == 1 ? HumanType.VillagerGhost : HumanType.Skeleton
                });
            ConfigureSkeleton(skeleton, Position);
            AddImmuneTag(skeleton);
            return skeleton;
        }

        private static void ConfigureSkeleton(IHumanoid Skeleton, Vector3 Position)
        {
            Skeleton.Physics.CollidesWithEntities = false;
            Skeleton.SearchComponent<CombatAIComponent>().SetCanExplore(false);
            Skeleton.SearchComponent<CombatAIComponent>().SetGuardSpawnPoint(false);
            Skeleton.Position = Position;
        }

        private static Item CreateItemForChest(int Level)
        {
            return ItemPool.Grab(Utils.Rng.Next(0, 5) == 1 ? ItemTier.Rare : ItemTier.Uncommon);
        }
    }
}