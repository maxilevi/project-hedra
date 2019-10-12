using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Thorns : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Thorns.png");
        private ThornsComponent _component;
        
        protected override void Add()
        {
            User.AddComponent(_component = new ThornsComponent(User, ReturnPercentage));
        }

        protected override void Remove()
        {
            if(_component != null) User.RemoveComponent(_component);
            _component = null;
        }

        public override string Description => Translations.Get("thorns_desc");
        public override string DisplayName => Translations.Get("thorns_skill");
        private float ReturnPercentage => .05f + .15f * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override string[] Attributes => new []
        {
            Translations.Get("thorns_damage_change", (int)(ReturnPercentage * 100))  
        };

        private class ThornsComponent : Component<IHumanoid>
        {
            private readonly float _return;
            
            public ThornsComponent(IHumanoid Entity, float ReturnPercentage) : base(Entity)
            {
                _return = ReturnPercentage;
                Parent.SearchComponent<DamageComponent>().OnDamageEvent += OnDamaged;
            }

            public override void Update()
            {
            }

            private void OnDamaged(DamageEventArgs Args)
            {
                if(Args.Damager == null) return;
                Args.Damager.Damage(Args.Amount * _return, Parent, out var xp);
                Parent.XP += xp;
            }
            
            public override void Dispose()
            {
                base.Dispose();
                Parent.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamaged;
            }
        }
    }
}