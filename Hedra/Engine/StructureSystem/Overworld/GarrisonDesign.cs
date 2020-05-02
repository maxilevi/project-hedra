using System;
using System.Numerics;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GarrisonDesign : SimpleStructureDesign<Garrison>
    {
        public override int PlateauRadius => 384;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GarrisonIcon);
        public override bool CanSpawnInside => false;
        protected override int StructureChance => StructureGrid.GarrisonChance;
        protected override CacheItem? Cache => CacheItem.Garrison;
        protected override Vector3 StructureScale => GarrisonCache.Scale;
        private static int Level => 12;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            /* Office */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door0.ply", Vector3.One),
                GarrisonCache.Doors[0], Rotation, Structure, false, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door1.ply", Vector3.One),
                GarrisonCache.Doors[1], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door2.ply", Vector3.One),
                GarrisonCache.Doors[2], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door3.ply", Vector3.One),
                GarrisonCache.Doors[3], Rotation, Structure, false, false);
            Structure.Waypoints = WaypointLoader.Load($"Assets/Env/Structures/Garrison/Garrison0-Pathfinding.ply",
                Vector3.One, Rotation * Translation);
            SceneLoader.LoadIfExists(Structure, $"Assets/Env/Structures/Garrison/Garrison0.ply", GarrisonCache.Scale, Rotation * Translation, GarrisonSettings);
        }

        protected override Garrison Create(Vector3 Position, float Size)
        {
            return new Garrison(Position);
        }
        

        protected static IHumanoid CreateBandit(Vector3 Position, CollidableStructure Structure)
        {
            var bandit = NPCCreator.SpawnBandit(Position, Level, BanditOptions.Default);
            bandit.Physics.CollidesWithEntities = false;
            bandit.SearchComponent<CombatAIComponent>().SetCanExplore(Value: false);
            bandit.SearchComponent<CombatAIComponent>().SetGuardSpawnPoint(Value: false);
            bandit.Position = Position;
            AddImmuneTag(bandit);
            return bandit;
        }

        private static Item CreateItemForRewardChest()
        {
            return ItemPool.Grab(Utils.Rng.Next(0, 7) == 1 ? ItemTier.Rare : ItemTier.Uncommon);
        }

        private SceneSettings GarrisonSettings { get; } = new SceneSettings
        {
            Structure4Creator = (P, _) => new Torch(P),
            Structure2Creator = SceneLoader.WellPlacer,
            Structure3Creator = (P, M) => StructureContentHelper.AddRewardChest(P, M, CreateItemForRewardChest()),
            Structure1Creator = SceneLoader.SleepingPadPlacer,
            Npc1Creator = CreateBandit,
            Npc2Creator = CreateBandit
        };
    }
}