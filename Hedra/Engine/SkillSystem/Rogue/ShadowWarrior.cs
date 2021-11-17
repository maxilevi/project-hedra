using System.Globalization;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using SixLabors.ImageSharp;

namespace Hedra.Engine.SkillSystem.Rogue
{
    public class ShadowWarrior : ActivateDurationSkill<IPlayer>
    {
        private IHumanoid _warrior;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ShadowWarrior.png");

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

        protected override void DoEnable()
        {
            _warrior = NPCCreator.SpawnHumanoid(HumanType.Rogue, User.Position + User.Orientation * 12);
            _warrior.Model.Outline = true;
            _warrior.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            _warrior.RemoveComponent(_warrior.SearchComponent<HealthBarComponent>());
            _warrior.AddComponent(new HealthBarComponent(_warrior, Translations.Get("shadow_warrior_name"),
                HealthBarType.Black, Color.FromRgb(40, 40, 40)));
            _warrior.Speed = User.Speed;
            _warrior.AttackPower = User.AttackPower;
            _warrior.AttackSpeed = User.AttackSpeed;
            _warrior.AttackResistance = User.AttackResistance;
            _warrior.Class = User.Class;
            _warrior.Level = User.Level;
            _warrior.Health = _warrior.MaxHealth;
            _warrior.Ring = User.Ring?.Clone();
            _warrior.SetWeapon(WeaponFactory.Get(User.MainWeapon?.Clone()));
            _warrior.SetBoots(User.Inventory.Boots?.Clone());
            _warrior.SetPants(User.Inventory.Pants?.Clone());
            _warrior.SetChestplate(User.Inventory.Chest?.Clone());
            _warrior.SetHelmet(User.Inventory.Helmet.Clone());
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
    }
}