using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Warrior
{
    public class Intercept : BaseSkill
    {
        private bool _isMoving;
        private readonly Timer _timer;
        private Animation _interceptStance;

        public Intercept()
        {
            _timer = new Timer(0);
            _interceptStance = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIntercept.dae");
        }

        public override void Update()
        {
            if (!_isMoving) return;

            this.EmitParticles();

            if (_timer.Tick()) _isMoving = false;
        }

        private void EmitParticles()
        {
            var underChunk = World.GetChunkAt(Player.Position);
            if (underChunk != null)
            {
                World.Particles.Color = World.GetHighestBlockAt((int)Player.Position.X, (int)Player.Position.Z).GetColor(underChunk.Biome.Colors);
                World.Particles.VariateUniformly = true;
                World.Particles.Position = Player.Position - Vector3.UnitY + Player.Orientation * 5f;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1f;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(Player.Model.Dimensions.Size.X, 2, Player.Model.Dimensions.Size.Z) * .75f;

                if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                    World.Particles.Color = new Vector4(underChunk.Biome.Colors.GrassColor.Xyz, 1);
            }
            for (var i = 0; i < 5; i++)
                World.Particles.Emit();
        }

        public override bool MeetsRequirements(IToolbar Bar, int CastingAbilityCount)
        {
            return base.MeetsRequirements(Bar, CastingAbilityCount) && !_isMoving;
        }

        public override void Use()
        {
            if(_isMoving) return;
            Player.Movement.Orientate();
            Player.Model.Blend(_interceptStance);
            _isMoving = true;
            _timer.AlertTime = Duration;
            _timer.Reset();
            Player.Movement.Move(Player.Movement.MoveFormula(Player.Orientation) * 1.5f, Duration, false);
        }

        public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/Intercept.png");
        public float Duration => 0.75f;
        public override string Description => "Charge and knock down you enemies.";
    }
}
