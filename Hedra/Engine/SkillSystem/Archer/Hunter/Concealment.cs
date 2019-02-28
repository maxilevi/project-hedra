using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Concealment : SingleAnimationSkill
    {
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealment.dae");
        private Animation StanceAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealmentStance.dae");
        protected override bool CanMoveWhileCasting => false;
        protected override bool ShouldDisable => Player.Toolbar.DisableAttack || !(Player.LeftWeapon is Bow) || _isActive;
        private bool _isActive;
        private Bow _bow;
        
        protected override void OnAnimationEnd()
        {
            Start();
        }

        private void Start()
        {
            if(!(Player.LeftWeapon is Bow bow)) return;
            _isActive = true;
            _bow = bow;
            Player.Model.Outline = true;
            Player.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            Player.LeftWeapon.SecondaryAttackEnabled = false;
            Player.BeforeAttack += BeforeAttack;
            _bow.BowModifiers += AddModifiers;
            Player.SearchComponent<DamageComponent>().OnDamageEvent += OnDamaged;
            Cooldown = 0;
            InvokeStateUpdated();
        }

        public override void Update()
        {
            base.Update();
            if (_isActive)
            {
                Player.Model.PlayAnimation(StanceAnimation);
                Player.LeftWeapon.InAttackStance = true;
                if (Player.IsMoving || !(Player.LeftWeapon is Bow))
                    End();
            }
        }

        private void End()
        {
            _isActive = false;
            Player.Model.BaseTint = Vector4.Zero;
            Player.LeftWeapon.SecondaryAttackEnabled = true;
            Player.Model.Reset();
            Player.BeforeAttack -= BeforeAttack;
            Player.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamaged;
            _bow.BowModifiers -= AddModifiers;
            Player.Model.Outline = false;
            Cooldown = MaxCooldown;
        }

        private void OnDamaged(DamageEventArgs Args)
        {
            if (!(Args.Amount > 0) || Args.Damager == null || Args.Damager == Player) return;
            End();
            Player.KnockForSeconds(5);
        }
        
        private static void AddModifiers(Projectile Proj)
        {
            Proj.Mesh.Outline = true;
            Proj.Mesh.OutlineColor = Colors.Violet;
            Proj.Mesh.Scale *= 1.15f;
            Proj.Speed *= 1.25f;
            Proj.Falloff = 0f;
            Proj.UsePhysics = false;
        }
        
        private void BeforeAttack(AttackOptions Options)
        {
            Options.DamageModifier *= 1 + DamageChange;
        }

        public override float MaxCooldown => 18;
        public override float IsAffectingModifier => _isActive ? 1 : 0;
        protected override int MaxLevel => 15;
        private float DamageChange => 1 + Level / 5f;
        public override string Description => Translations.Get("concealment_desc");
        public override string DisplayName => Translations.Get("concealment_skill");
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Concealment.png");
        public override string[] Attributes => new[]
        {
            Translations.Get("concealment_damage_change", (int)(DamageChange * 100))
        };
    }
}