using System;
using System.IO;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTowerDesign : SimpleCompletableStructureDesign<WizardTower>
    {
        public override int PlateauRadius => 256;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WizardTowerIcon);
        protected override int StructureChance => StructureGrid.WizardTower;
        protected override CacheItem? Cache => CacheItem.WizardTower;
        protected override bool NoPlantsZone => true;
        protected override Vector3 StructureOffset => Vector3.UnitY * -.5f;
        protected override Vector3 StructureScale => Vector3.One * 1.25f;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            AddDoors(Structure, Rotation);
            SceneLoader.LoadIfExists(Structure, "Assets/Env/Structures/WizardTower/WizardTower0.ply", StructureScale, Rotation * Translation, WizardTowerSettings);
        }
        
        protected override void ApplyColors(VertexData Model, RegionColor Region)
        {
            Model.Color(AssetManager.ColorCode0, Region.StoneColor);
            Model.Color(AssetManager.ColorCode1, Region.DirtColor);
            Model.GraduateColor(Vector3.UnitY);
        }

        protected override WizardTower Create(Vector3 Position, float Size)
        {
            return new WizardTower(Position);
        }

        private void AddDoors(CollidableStructure Structure, Matrix4x4 Transformation)
        {
            var model = AssetManager.PLYLoader("Assets/Env/Structures/WizardTower/WizardTower0-Door0.ply", Vector3.One);
            AddDoor(model, WizardTowerCache.Door0, Transformation, Structure, true, false);
        }

        private static IHumanoid CreateWizard(Vector3 Position)
        {
            var wizard = World.WorldBuilding.SpawnHumanoid(HumanType.Witch, Position);
            wizard.AddComponent(new QuestGiverComponent(wizard, MissionPool.Random(Position, QuestTier.Medium, QuestHint.Magic)));
            wizard.Physics.CollidesWithEntities = false;
            wizard.SearchComponent<DamageComponent>().Immune = true;
            return wizard;
        }

        public override string DisplayName => Translations.Get("structure_wizard_tower");

        protected override string GetDescription(WizardTower Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(WizardTower Structure) => throw new System.NotImplementedException();

        private static SceneSettings WizardTowerSettings { get; } = new SceneSettings
        {
            IsNightLight = false,
            LightRadius = PointLight.DefaultRadius * 1.5f,
            Npc1Creator = CreateWizard
        };
    }
}