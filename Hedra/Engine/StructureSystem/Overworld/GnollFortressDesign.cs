using System;
using System.Numerics;
using Hedra.AISystem.Humanoid;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GnollFortressDesign : SimpleCompletableStructureDesign<GnollFortress>
    {
        public const int Level = 27;
        public override int PlateauRadius => 480;
        public override string DisplayName => Translations.Get("structure_gnoll_fortress");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GnollFortressIcon);
        public override bool CanSpawnInside => false;
        protected override int StructureChance => StructureGrid.GnollFortressChance;
        protected override CacheItem? Cache => CacheItem.GnollFortress;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            for (var i = 0; i < GnollFortressCache.DoorSettings.Length; ++i)
            {
                var settings = GnollFortressCache.DoorSettings[i];
                AddDoor(
                    AssetManager.PLYLoader($"Assets/Env/Structures/GnollFortress/GnollFortress0-Door{i}.ply", settings.Scale),
                    settings.Position + Vector3.UnitY * 0.05f * GnollFortressCache.Scale,
                    Rotation,
                    Structure,
                    settings.InvertedRotation,
                    settings.InvertedPivot
                );
            }
            Structure.Waypoints = WaypointLoader.Load(GnollFortressCache.PathfindingFile, Vector3.One * GnollFortressCache.Scale, Rotation * Translation);
            SceneLoader.LoadIfExists(Structure, GnollFortressCache.SceneFile, GnollFortressCache.Scale, Rotation * Translation, GnollFortressSettings);
        }

        protected override GnollFortress Create(Vector3 Position, float Size)
        {
            return new GnollFortress(Position);
        }

        protected override string GetDescription(GnollFortress Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(GnollFortress Structure) => throw new System.NotImplementedException();

        protected static IHumanoid CreateMeleeGnoll(Vector3 Position, CollidableStructure Structure)
        {
            var options = BanditOptions.Default;
            options.ModelType = HumanType.GnollWarrior;
            var bandit = NPCCreator.SpawnBandit(Position, Level, options);
            bandit.Physics.CollidesWithEntities = false;
            bandit.SearchComponent<CombatAIComponent>().SetCanExplore(Value: false);
            bandit.SearchComponent<CombatAIComponent>().SetGuardSpawnPoint(Value: false);
            bandit.Position = Position;
            AddImmuneTag(bandit);
            return bandit;
        }
        
        protected static IHumanoid CreateRangedGnoll(Vector3 Position, CollidableStructure Structure)
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
            return ItemPool.Grab(ItemTier.Rare);
        }

        private SceneSettings GnollFortressSettings { get; } = new SceneSettings
        {
            Structure4Creator = (P, _) => new Torch(P),
            Structure2Creator = SceneLoader.SleepingPadPlacer,
            Structure3Creator = (P, M) => StructureContentHelper.AddRewardChest(P, M, CreateItemForRewardChest()),
            Structure1Creator = SceneLoader.WellPlacer,
            Npc1Creator = CreateRangedGnoll,
            Npc2Creator = CreateMeleeGnoll,
        };
    }
}