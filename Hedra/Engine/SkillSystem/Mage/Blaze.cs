using System.Collections.Generic;
using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Blaze : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Blaze.png");
        private BlazeComponent _component;
        
        protected override void DoEnable()
        {
            Player.AddComponent(_component = new BlazeComponent(Player));
        }

        protected override void DoDisable()
        {
            Player.RemoveComponent(_component);
        }

        private class BlazeComponent : EntityComponent
        {
            private readonly Timer _spotTimer;
            private readonly List<Vector3> _spots;
            
            public BlazeComponent(IEntity Entity) : base(Entity)
            {
                _spotTimer = new Timer(.75f);
                _spots = new List<Vector3>();
            }

            public override void Update()
            {
                if (!Parent.IsGrounded) return;
                World.Particles.VariateUniformly = false;
                World.Particles.Color = new Vector4(1, .3f, 0, 1);
                World.Particles.Position = Parent.Position;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.ScaleErrorMargin = Vector3.One * .35f;
                World.Particles.Direction = (-Parent.Orientation + Vector3.UnitY * 2f) * .25f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = Vector3.One * 2;
                for(var i = 0; i < 5; ++i)
                    World.Particles.Emit();
                if (_spotTimer.Tick())
                    MarkSpot();
            }

            private void MarkSpot()
            {
                if (_spots.Count > 0) _spots.RemoveAt(0);
                World.HighlightArea(Parent.Position, new Vector4(1, .3f, 0, 1), 6, _spotTimer.AlertTime);
                _spots.Add(Parent.Position);
            }
        }

        protected override int MaxLevel => 15;
        protected override float Duration => 5 + 9 * (Level / (float) MaxLevel);
        protected override float CooldownDuration => 34 - 6 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("blaze_desc");
        public override string DisplayName => Translations.Get("blaze_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("blaze_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}