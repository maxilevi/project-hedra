using System;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0Design : SimpleCompletableStructureDesign<Dungeon0>
    {
        public override int PlateauRadius => 384;
        public override string DisplayName => Translations.Get("structure_dungeon");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        protected override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;

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
            AddMembersToStructure((Dungeon0)Structure.WorldObject, Structure);
            
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door0.ply", Vector3.One), Dungeon0Cache.Doors[0], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door1.ply", Vector3.One), Dungeon0Cache.Doors[1], Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door2.ply", Vector3.One), Dungeon0Cache.Doors[2], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door3.ply", Vector3.One), Dungeon0Cache.Doors[3], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door4.ply", Vector3.One), Dungeon0Cache.Doors[4], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door5.ply", Vector3.One), Dungeon0Cache.Doors[5], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door6.ply", Vector3.One), Dungeon0Cache.Doors[6], Rotation, Structure, false, true);
        }

        private static void AddMembersToStructure(Dungeon0 Dungeon, CollidableStructure Structure)
        {
            Dungeon.Trigger = (Dungeon0Trigger) Structure.WorldObject.Children.First(T => T is CollisionTrigger);
            //Dungeon.Boss = Structure.WorldObject.n
        }

        protected override string GetDescription(Dungeon0 Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(Dungeon0 Structure) => throw new System.NotImplementedException();

        private static BaseStructure BuildTrigger0(Vector3 Point, VertexData Mesh)
        {
            return new Dungeon0Trigger(Point, Mesh);
        }
        

        private static IEntity PatrolSkeleton(Vector3 Position)
        {
            var skeleton = default(IEntity);
            var spawnKamikazeSkeleton = Utils.Rng.Next(1, 2) == 1;
            skeleton = spawnKamikazeSkeleton
                ? World.SpawnMob(MobType.SkeletonKamikaze, Position, Utils.Rng)
                : NormalPatrolSkeleton(Position);
            skeleton.Position = Position;
            return skeleton;
        }

        private static IEntity NormalPatrolSkeleton(Vector3 Position)
        {
            var skeleton = BaseSkeleton(Position);
            //skeleton.AddComponent(new WizardTowerAIComponent(wizard, Position.Xz(), Vector2.One * 16));
            return skeleton;
        }
        
        private static IEntity StationarySkeleton(Vector3 Position)
        {
            var skeleton = BaseSkeleton(Position);
            //skeleton.AddComponent();
            return skeleton;
        }

        private static IHumanoid BaseSkeleton(Vector3 Position)
        {
            const int level = 17;
            var skeleton = World.WorldBuilding.SpawnBandit(Position, level, false, true);
            skeleton.Physics.CollidesWithEntities = false;
            skeleton.Position = Position;
            skeleton.RemoveComponent<BaseHumanoidAIComponent>();
            return skeleton;
        }
        
        private static SceneSettings Settings { get; } = new SceneSettings
        {
            LightRadius = PointLight.DefaultRadius * 1.5f,
            IsNightLight = false,
            Structure1Creator = BuildTrigger0,
            Structure2Creator = (P, V) => new Lever(P, SceneLoader.GetRadius(V)),
            Npc1Creator = PatrolSkeleton,
            Npc2Creator = StationarySkeleton
        };
    }
}