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
        protected override Vector3 StructureScale => Vector3.One * 1.0f;

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

        private static IHumanoid CreateBaseWizard(Vector3 Position, HealthBarType BehaviourType)
        {
            var type = Utils.Rng.Next(0, 2) == 1 ? HumanType.Witch : HumanType.Scholar;
            var wizard = World.WorldBuilding.SpawnHumanoid(type, Position);
            wizard.Physics.CollidesWithEntities = false;
            wizard.Physics.GravityDirection = Vector3.Zero;
            wizard.SearchComponent<DamageComponent>().Immune = true;
            var healthBar = wizard.SearchComponent<HealthBarComponent>();
            wizard.RemoveComponent(healthBar);
            wizard.AddComponent(new HealthBarComponent(wizard, healthBar.Name, BehaviourType));
            return wizard;
        }
        
        private static IHumanoid CreateWizard(Vector3 Position)
        {
            if (Utils.Rng.Next(0, 2) == 1) return null;
            var wizard = CreateBaseWizard(Position, HealthBarType.Friendly);
            if (Utils.Rng.Next(0, 4) == 1)
            {
                var questComponent = new QuestGiverComponent(wizard,
                    MissionPool.Random(Position, QuestTier.Medium, QuestHint.Magic));
                wizard.AddComponent(questComponent);
            }
            else
            {
                wizard.AddComponent(new WizardThoughtsComponent(wizard));
                wizard.AddComponent(new TalkComponent(wizard));
            }
            return wizard;
        }
        
        private static IHumanoid CreateDarkWizard(Vector3 Position)
        {
            if (Utils.Rng.Next(0, 4) == 1) return null;
            
            var wizard = CreateBaseWizard(Position, HealthBarType.Black);
            if (Utils.Rng.Next(0, 6) == 1)
            {
                //var questComponent = new QuestGiverComponent(wizard, MissionPool.Grab());
                //wizard.AddComponent(questComponent);
            }
            else
            {
                wizard.AddComponent(new DarkWizardThoughtsComponent(wizard));
                wizard.AddComponent(new TalkComponent(wizard));
            }
            return wizard;
        }

        public override string DisplayName => Translations.Get("structure_wizard_tower");

        protected override string GetDescription(WizardTower Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(WizardTower Structure) => throw new System.NotImplementedException();

        private static SceneSettings WizardTowerSettings { get; } = new SceneSettings
        {
            IsNightLight = false,
            LightRadius = PointLight.DefaultRadius * 1.5f,
            Npc1Creator = CreateWizard,
            Npc2Creator = CreateDarkWizard
        };
    }
}