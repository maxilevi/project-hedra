using System;
using System.Drawing;
using System.Globalization;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class Leech : RadiusEffectSkill<ISkilledAnimableEntity>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Leech.png");

        protected override void Apply(IEntity Entity)
        {
            if (Entity.SearchComponent<DamageComponent>().HasIgnoreFor(User)) return;
            Entity.AddComponentForSeconds(new LeechComponent(Entity, User, Damage, Health), Duration);
        }

        protected override Vector4 HighlightColor => Colors.Red * .5f;

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("leech_desc");
        public override string DisplayName => Translations.Get("leech_skill");
        protected override float Radius => 48 + 48 * (Level / (float) MaxLevel);
        private float Health => Damage;
        private float Damage => Math.Min(10, 5 + 5 * (Level / 10));
        private float Duration => 7 + 10 * (Level / (float)MaxLevel);
        public override float ManaCost => 60;
        public override float MaxCooldown => 27 - 5 * (MaxLevel / (float) MaxLevel) + Duration;
        public override string[] Attributes => new[]
        {
            Translations.Get("leech_damage_and_health_change", Damage.ToString("0.0", CultureInfo.InvariantCulture), Health.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("leech_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("leech_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture))
        };

        private class LeechComponent : EntityComponent
        {
            private readonly ISkilledAnimableEntity _caster;
            private readonly Timer _timer;
            private readonly float _damagePerSecond;
            private readonly float _healPerSecond;
            
            public LeechComponent(IEntity Entity, ISkilledAnimableEntity Caster, float DamagePerSecond, float HealthPerSecond) : base(Entity)
            {
                _caster = Caster;
                _timer = new Timer(1 + Utils.Rng.NextFloat());
                _damagePerSecond = DamagePerSecond;
                _healPerSecond = HealthPerSecond;
                Parent.Model.Outline = true;
                Parent.Model.OutlineColor = new Vector4(Colors.Red.Xyz * .75f, 1);
            }

            public override void Update()
            {
                if (_timer.Tick())
                {
                    Parent.Damage(_damagePerSecond, _caster, out var xp);
                    if(_caster is IHumanoid humanoid)
                        humanoid.XP += xp;
                    BloodSkill.LaunchParticle(_caster, Parent, _caster, (_, __) =>
                    {
                        _caster.ShowText($"+{_healPerSecond} HP", Color.LawnGreen, 14);
                        _caster.Health += _healPerSecond;
                        _caster.Model.Outline = true;
                        _caster.Model.OutlineColor = Colors.GreenYellow;
                        TaskScheduler.After(.5f, () =>
                        {
                            _caster.Model.Outline = false;
                        });
                    });
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                Parent.Model.Outline = false;
            }
        }
    }
}