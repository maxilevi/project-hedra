using System;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
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
    public class Dungeon0Design : BaseDungeonDesign<Dungeon0>
    {
        public override int PlateauRadius => 300;
        protected override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;
        public override bool CanSpawnInside => false;
        protected override Vector3 StructureOffset => Dungeon0Cache.Offset;
        protected override BlockType PathType => BlockType.StonePath;
        protected override float GroundworkRadius => 180;
        
        protected override Dungeon0 Create(Vector3 Position, float Size)
        {
            return new Dungeon0(Position);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, "Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One, Rotation * Translation, Settings);
            AddMembersToStructure((Dungeon0)Structure.WorldObject, Structure, Rotation, Translation);
            
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

        private static void AddMembersToStructure(Dungeon0 Dungeon, CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation)
        {
            Dungeon.BuildingTrigger = (DungeonDoorTrigger) Structure.WorldObject.Children.First(T => T is DungeonDoorTrigger);
            if (Dungeon.BuildingTrigger == null)
            {
                int a = 0;
            }
            Structure.Waypoints = WaypointLoader.Load("Assets/Env/Structures/Dungeon/Dungeon0-Pathfinding.ply", Vector3.One, Rotation * Translation);
        }

        private static BaseStructure BuildTrigger0(Vector3 Point, VertexData Mesh)
        {
            return new DungeonDoorTrigger(Point, Mesh);
        }
        
        private static BaseStructure BuildTrigger1(Vector3 Point, VertexData Mesh)
        {
            return new DungeonBossRoomTrigger(Point, Mesh);
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
            ((Dungeon0) Structure.WorldObject).Boss = boss;
            Structure.WorldObject.Search<DungeonBossRoomTrigger>().Boss = boss;
            return boss;
        }

        private static void AddImmuneTag(IEntity Skeleton)
        {
            Skeleton.AddComponent(new IsDungeonMemberComponent(Skeleton));
            Skeleton.SearchComponent<DamageComponent>().Ignore(E => E.SearchComponent<IsDungeonMemberComponent>() != null);
        }
        
        private static IEntity PatrolSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var skeleton = default(IEntity);
            var spawnKamikazeSkeleton = Utils.Rng.Next(1, 7) == 1;
            skeleton = spawnKamikazeSkeleton
                ? SpawnKamikazeSkeleton(Position, Structure)
                : NormalSkeleton(Position, Structure);
            skeleton.Position = Position;
            AddImmuneTag(skeleton);
            return skeleton;
        }

        private static IEntity SpawnKamikazeSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            var mob = World.SpawnMob(MobType.SkeletonKamikaze, Position, Utils.Rng);
            mob.Position = Position;
            var previousAI = mob.SearchComponent<BasicAIComponent>();
            mob.RemoveComponent(previousAI, false);
            mob.AddComponent(new DungeonDualAIComponent(mob, new DungeonSkeletonKamikazeAIComponent(mob), previousAI, Structure));
            return mob;
        }

        private static IHumanoid NormalSkeleton(Vector3 Position, CollidableStructure Structure)
        {
            const int level = 17;
            var skeleton = World.WorldBuilding.SpawnBandit(Position, level, false, true, Class.Warrior | Class.Rogue | Class.Mage);
            skeleton.Physics.CollidesWithEntities = false;
            skeleton.Position = Position;
            var previousAI = skeleton.SearchComponent<BaseHumanoidAIComponent>();
            skeleton.RemoveComponent(previousAI, false);
            var dungeonAI = default(IComponent<IEntity>);
            if(previousAI is MeleeAIComponent)
                dungeonAI = new DungeonMeleeAIComponent(skeleton, false);
            else if(previousAI is MageAIComponent)
                dungeonAI = new DungeonMageAIComponent(skeleton, false);
            else if(previousAI is RangedAIComponent)
                dungeonAI = new DungeonRangedAIComponent(skeleton, false);
            else
                throw new ArgumentOutOfRangeException();
            skeleton.AddComponent(new DungeonDualAIComponent(skeleton, dungeonAI, previousAI, Structure));
            return skeleton;
        }
        
        private static SceneSettings Settings { get; } = new SceneSettings
        {
            LightRadius = Torch.DefaultRadius * 2,
            LightColor = WorldLight.DefaultColor * 2,
            IsNightLight = false,
            Structure1Creator = BuildTrigger0,
            Structure2Creator = BuildTrigger1,
            Npc1Creator = PatrolSkeleton,
            Npc2Creator = PatrolSkeleton,
            Npc3Creator = SkeletonBoss,
            Structure4Creator = (P, _) => new Torch(P),
        };
    }
}