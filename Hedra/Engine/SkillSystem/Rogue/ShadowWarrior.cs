using System;
using System.Drawing;
using System.Globalization;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue
{
    public class ShadowWarrior : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ShadowWarrior.png");
        private IHumanoid _warrior;
        
        protected override void DoEnable()
        {
            _warrior = World.WorldBuilding.SpawnHumanoid(HumanType.Rogue, Player.Position + Player.Orientation * 12);
            _warrior.Model.Outline = true;
            _warrior.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            _warrior.RemoveComponent(_warrior.SearchComponent<HealthBarComponent>());
            _warrior.AddComponent(new HealthBarComponent(_warrior, Translations.Get("shadow_warrior_name"), HealthBarType.Black, Color.FromArgb(255, 40, 40, 40)));
            _warrior.Speed = Player.Speed;
            _warrior.AttackPower = Player.AttackPower;
            _warrior.AttackSpeed = Player.AttackSpeed;
            _warrior.AttackResistance = Player.AttackResistance;
            _warrior.Class = Player.Class;
            _warrior.Level = Player.Level;
            _warrior.Health = _warrior.MaxHealth;
            _warrior.Ring = Player.Ring;
            _warrior.SetWeapon(WeaponFactory.Get(Player.MainWeapon));
            _warrior.SetBoots(Player.Inventory.Boots?.Boots);
            _warrior.SetPants(Player.Inventory.Pants?.Pants);
            _warrior.SetChestplate(Player.Inventory.Chest?.Chestplate);
            _warrior.SetHelmet(Player.Inventory.Helmet?.Helmet);
            _warrior.SearchComponent<DamageComponent>().Ignore(E => E == Player || E == Player.Pet.Pet);
            _warrior.Physics.CanBePushed = false;
            _warrior.Physics.CollidesWithEntities = false;
            _warrior.Physics.CollidesWithStructures = false;
            var ai = new ShadowWarriorComponent(_warrior, Player);
            _warrior.AddComponent(ai);
        }

        protected override void DoDisable()
        {
            _warrior.Dispose();
            _warrior = null;
        }
        
        private class ShadowWarriorComponent : MeleeMinionComponent
        {
            public ShadowWarriorComponent(IHumanoid Parent, IHumanoid Owner) : base(Parent, Owner)
            {
            }
            
            public override void Update()
            {
                base.Update();
                ShowParticles();
            }           
            
            private void ShowParticles()
            {
                SkillUtils.DarkContinuousParticles(Parent);
            }
        }

        protected override bool ShouldDisable => !Player.HasWeapon || !(Player.LeftWeapon is MeleeWeapon);
        protected override float Duration => 28 + Level * 2f;
        protected override int MaxLevel => 15;
        public override float ManaCost => 80;
        protected override float CooldownDuration => 82 - Level;
        public override string Description => Translations.Get("shadow_warrior_desc");
        public override string DisplayName => Translations.Get("shadow_warrior_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("shadow_warrior_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}