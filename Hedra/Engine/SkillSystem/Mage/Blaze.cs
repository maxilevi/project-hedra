using System.Collections.Generic;
using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Blaze : DrainSkill<ISkilledAnimableEntity>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Blaze.png");
        private BlazeComponent _component;
        private SpeedBonusComponent _speedComponent;
        
        protected override void Activate()
        {
            User.AddComponent(_component = new BlazeComponent(User, Damage));
            User.AddComponent(_speedComponent = new SpeedBonusComponent(User, User.Speed * SpeedModifier));
        }

        protected override void Deactivate()
        {
            User.RemoveComponent(_component);
            User.RemoveComponent(_speedComponent);
        }

        private class BlazeComponent : Component<IEntity>
        {
            private readonly Timer _gcTimer;
            private readonly List<Vector3> _spots;
            private readonly ParticleSystem _particles;
            private readonly float _damage;
            private Vector3 _lastPosition;
            
            public BlazeComponent(IEntity Entity, float Damage) : base(Entity)
            {
                _gcTimer = new Timer(.1f);
                _spots = new List<Vector3>();
                _particles = new ParticleSystem();
                _damage = Damage;
                Parent.Model.Outline = true;
                Parent.Model.OutlineColor = new Vector4(1, .3f, 0, 1);
            }

            public override void Update()
            {
                UpdateSpots();
                if (Parent.IsGrounded && (_lastPosition - Parent.Position).LengthFast() > 2)
                {
                    MarkSpot();
                    _lastPosition = Parent.Position;
                }
                if (_gcTimer.Tick() && _spots.Count > 0) _spots.RemoveAt(0);
            }

            private void UpdateSpots()
            {
                for (var k = 0; k < _spots.Count; ++k)
                {
                    _particles.Collides = true;
                    _particles.VariateUniformly = false;
                    _particles.Color = new Vector4(1, .3f, 0, 1);
                    _particles.Position = _spots[k];
                    _particles.Scale = Vector3.One * .25f;
                    _particles.ScaleErrorMargin = Vector3.One * .35f;
                    _particles.Direction = Vector3.UnitY;
                    _particles.ParticleLifetime = .75f;
                    _particles.GravityEffect = .25f;
                    _particles.PositionErrorMargin = Vector3.One * 1.5f;
                    _particles.Emit();
                }
            }
            
            private void MarkSpot()
            {
                _spots.Add(Parent.Position);
                for (var k = 0; k < _spots.Count; ++k)
                {
                    SkillUtils.DoNearby(Parent, 8, -1, (E, F) =>
                    {
                        if(E.SearchComponent<BurningComponent>() == null)
                            E.AddComponent(new BurningComponent(E, Parent, 3, _damage));
                    });
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                Parent.Model.Outline = false;
                TaskScheduler.After(_particles.ParticleLifetime + 1, () => _particles.Dispose());
            }
        }

        protected override int MaxLevel => 15;
        protected override float ManaPerSecond => 28;
        private float SpeedModifier => .25f + .25f * (Level / (float) MaxLevel);
        private float Damage => 15f + 40f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("blaze_desc");
        public override string DisplayName => Translations.Get("blaze_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("blaze_speed_change", (int)(SpeedModifier * 100)),
            Translations.Get("blaze_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("blaze_mana_cost", ManaPerSecond.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}