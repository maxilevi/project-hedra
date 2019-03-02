using System;
using System.Drawing;
using System.Globalization;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.Engine.EntitySystem;
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
            _warrior.SearchComponent<DamageComponent>().Ignore(E => E == Player);
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
        
        private class ShadowWarriorComponent : WarriorAIComponent
        {
            private readonly MinionAIComponent _aiComponent;
            
            public ShadowWarriorComponent(IHumanoid Parent, IHumanoid Owner) : base(Parent, default(bool))
            {
                IgnoreEntities = new IEntity[] { Owner };
                _aiComponent = new MinionAIComponent(Parent, Owner);
                _aiComponent.AlterBehaviour<AttackBehaviour>(
                    new ShadowWarriorBehaviour(Parent, (T, M) =>
                    {
                        Parent.RotateTowards(T);
                        base.OnAttack();
                    })
                );
            }
            
            public override void Update()
            {
                _aiComponent.Update();
                base.DoUpdate();
                ShowParticles();
            }           
            
            private void ShowParticles()
            {
                World.Particles.Color = new Vector4(.2f, .2f, .2f, .8f);
                World.Particles.VariateUniformly = true;
                World.Particles.Position =
                    Parent.Position + Vector3.UnitY * Parent.Model.Height * .3f;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = -Parent.Orientation * .05f;
                World.Particles.ParticleLifetime = 1.0f;
                World.Particles.GravityEffect = 0.0f;
                World.Particles.PositionErrorMargin = new Vector3(1.25f, Parent.Model.Height * .3f, 1.25f);
                World.Particles.Emit();
            }

            public override void Dispose()
            {
                base.Dispose();
                _aiComponent.Dispose();
            }
        }

        private class ShadowWarriorBehaviour : AttackBehaviour
        {
            private readonly Action<IEntity, float> _lambda;
            public ShadowWarriorBehaviour(IEntity Parent, Action<IEntity, float> Lambda) : base(Parent)
            {
                _lambda = Lambda;
            }

            protected override void Attack(float RangeModifier)
            {
                _lambda(Target, RangeModifier);
            }
        }

        protected override bool ShouldDisable => !Player.HasWeapon || !(Player.LeftWeapon is MeleeWeapon);
        protected override float Duration => 21 + Level * 2f;
        protected override int MaxLevel => 15;
        public override float ManaCost => 80;
        public override float MaxCooldown => Duration + 82 - Level;
        public override string Description => Translations.Get("shadow_warrior_desc");
        public override string DisplayName => Translations.Get("shadow_warrior_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("shadow_warrior_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}