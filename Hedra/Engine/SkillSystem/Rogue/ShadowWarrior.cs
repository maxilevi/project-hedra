using System;
using System.Drawing;
using System.Globalization;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using System.Numerics;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.SkillSystem.Rogue
{
    public class ShadowWarrior : ActivateDurationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ShadowWarrior.png");
        private IHumanoid _warrior;
        
        protected override void DoEnable()
        {
            _warrior = NPCCreator.SpawnHumanoid(HumanType.Rogue, User.Position + User.Orientation * 12);
            _warrior.Model.Outline = true;
            _warrior.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            _warrior.RemoveComponent(_warrior.SearchComponent<HealthBarComponent>());
            _warrior.AddComponent(new HealthBarComponent(_warrior, Translations.Get("shadow_warrior_name"), HealthBarType.Black, Color.FromArgb(255, 40, 40, 40)));
            _warrior.Speed = User.Speed;
            _warrior.AttackPower = User.AttackPower;
            _warrior.AttackSpeed = User.AttackSpeed;
            _warrior.AttackResistance = User.AttackResistance;
            _warrior.Class = User.Class;
            _warrior.Level = User.Level;
            _warrior.Health = _warrior.MaxHealth;
            _warrior.Ring = User.Ring;
            _warrior.SetWeapon(WeaponFactory.Get(User.MainWeapon));
            _warrior.SetBoots(User.Inventory.Boots?.Boots);
            _warrior.SetPants(User.Inventory.Pants?.Pants);
            _warrior.SetChestplate(User.Inventory.Chest?.Chestplate);
            _warrior.SetHelmet(User.Inventory.Helmet?.Helmet);
            _warrior.SearchComponent<DamageComponent>().Ignore(E => E == User || E == User.Companion.Entity);
            _warrior.Physics.CanBePushed = false;
            _warrior.Physics.CollidesWithEntities = false;
            _warrior.Physics.CollidesWithStructures = false;
            var ai = new ShadowWarriorComponent(_warrior, User);
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

        protected override bool ShouldDisable => !User.HasWeapon || !(User.LeftWeapon is MeleeWeapon);
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