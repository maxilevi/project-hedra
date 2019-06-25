using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class HotPursuit : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HotPursuit.png");
        protected override int MaxLevel => 15;
        private readonly Timer _combatTimer;
        private bool _inCombat;
        private SpeedBonusComponent _component;

        public HotPursuit()
        {
            _combatTimer = new Timer(5f);
        }
        
        protected override void Add()
        {
            User.SearchComponent<DamageComponent>().OnDamageEvent += OnDamageEvent;
        }
        
        protected override void Remove()
        {
            User.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamageEvent;
            DisableCombat();
        }

        public override void Update()
        {
            base.Update();
            if(!_inCombat) return;
            if (User.IsDead)
                DisableCombat();
            if (_combatTimer.Tick())
                DisableCombat();
        }

        private void OnDamageEvent(DamageEventArgs Args)
        {
            EnableCombat();
        }

        private void DisableCombat()
        {
            _inCombat = false;
            RemoveComponentIfNecessary();
        }

        private void EnableCombat()
        {
            var previousValue = _inCombat;
            _inCombat = true;
            if (!previousValue) InvokeStateUpdated();
            _combatTimer.Reset();
            RemoveComponentIfNecessary();
            User.AddComponent(_component = new SpeedBonusComponent(User, User.Speed * SpeedChange));
        }

        private void RemoveComponentIfNecessary()
        {
            if (_component != null) User.RemoveComponent(_component);
            _component = null;
        }

        private float SpeedChange => (float) Level / MaxLevel / 4f;
        public override float IsAffectingModifier => _inCombat ? 1 : 0;
        public override string Description => Translations.Get("hot_pursuit_desc");
        public override string DisplayName => Translations.Get("hot_pursuit_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("hot_pursuit_speed_change", (int) (SpeedChange * 100))
        };
    }
}