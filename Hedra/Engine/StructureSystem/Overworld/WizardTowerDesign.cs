using System;
using System.IO;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using BulletSharp;
using Hedra.AISystem.Humanoid;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Numerics;
using Hedra.WeaponSystem;
using TaskScheduler = Hedra.Core.TaskScheduler;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTowerDesign : SimpleFindableStructureDesign<WizardTower>
    {
        public override int PlateauRadius => 384;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WizardTowerIcon);
        protected override int StructureChance => StructureGrid.WizardTower;
        protected override CacheItem? Cache => CacheItem.WizardTower;
        protected override bool NoPlantsZone => true;
        protected override Vector3 StructureOffset => Vector3.UnitY * -.5f;
        protected override Vector3 StructureScale => Vector3.One * 1.0f;
        protected override BlockType PathType => BlockType.Dirt;
        protected override float GroundworkRadius => 128 + 64;
        public override bool CanSpawnInside => true;

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
            AddDoor(model, WizardTowerCache.Door0, Transformation, Structure, false, false);
        }

        private static IHumanoid CreateBaseWizard(Vector3 Position, HealthBarType BehaviourType)
        {
            var type = Utils.Rng.Next(0, 2) == 1 ? HumanType.Witch : HumanType.Scholar;
            var wizard = NPCCreator.SpawnHumanoid(type, Position);
            wizard.Physics.CollidesWithEntities = false;
            wizard.Position = Position;
            wizard.SearchComponent<DamageComponent>().Immune = true;
            var healthBar = wizard.SearchComponent<HealthBarComponent>();
            wizard.RemoveComponent(healthBar);
            wizard.SetWeapon(new Hands());
            wizard.AddComponent(new HealthBarComponent(wizard, healthBar.Name, BehaviourType));
            wizard.AddComponent(new WizardTowerAIComponent(wizard, Position.Xz(), Vector2.One * 16));
            return wizard;
        }
        
        private static IHumanoid CreateWizard(Vector3 Position, CollidableStructure Structure)
        {
            if (Utils.Rng.Next(0, 3) == 1) return null;
            var wizard = CreateBaseWizard(Position, HealthBarType.Friendly);
            if (Utils.Rng.Next(0, 4) == 1)
            {
                var quest = MissionPool.Random(Position, QuestTier.Medium, QuestHint.Magic);
                var questComponent = new QuestGiverComponent(wizard, quest);
                wizard.AddComponent(questComponent);
            }
            else
            {
                wizard.AddComponent(new WizardThoughtsComponent(wizard));
                wizard.AddComponent(new TalkComponent(wizard));
            }
            return wizard;
        }
        
        private static IHumanoid CreateDarkWizard(Vector3 Position, CollidableStructure Structure)
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

        private static SceneSettings WizardTowerSettings { get; } = new SceneSettings
        {
            IsNightLight = false,
            LightRadius = PointLight.DefaultRadius * 1.5f,
            Npc1Creator = CreateWizard,
            Npc2Creator = CreateDarkWizard,
            Structure1Creator = (V, _) => new SleepingPad(V),
            Structure3Creator = SceneLoader.WellPlacer,
            Structure4Creator = SceneLoader.FireplacePlacer
        };

        private class WizardTowerAIComponent : BaseVillagerAIComponent
        {
            private readonly Vector2 _position;
            private readonly Vector2 _size;
            public WizardTowerAIComponent(IHumanoid Parent, Vector2 Position, Vector2 Size) : base(Parent, true)
            {
                _position = Position;
                _size = Size;
            }

            protected override void OnMovementStuck()
            {
                base.OnMovementStuck();
                base.CancelMovement();
            }

            protected override Vector3 NewPoint
            {
                get
                {
                    /* Try 10 times */
                    for(var i = 0; i < 10; ++i)
                    {
                        var newPoint = new Vector3(
                                           Utils.Rng.NextFloat() * _size.X - _size.X * .5f,
                                           0,
                                           Utils.Rng.NextFloat() * _size.Y - _size.Y * .5f
                                       ) + _position.ToVector3();
                        return newPoint;
                    }
                    return Parent.Position;
                }
            }

            protected override bool ShouldSit => true;
        }
    }
}