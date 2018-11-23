using System;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class SpeedComponent : EntityComponent
    {
        public int Chance { get; set; } = 15;
        public float SpeedBonus { get; set; } = .5f;
        private float _speedTime;

        public SpeedComponent(IEntity Parent) : base(Parent) {
            Parent.AfterAttacking += this.Apply;
        }

        public override void Update()
        {
            _speedTime -= Time.IndependantDeltaTime;
        }

        public void Apply(IEntity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                _speedTime = 4 + Utils.Rng.NextFloat() * 2 - 1f;
                if(Parent.SearchComponent<SpeedBonusComponent>() == null)
                    Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, SpeedBonus), () => _speedTime > 0);
            }
        }

        public override void Dispose()
        {
            Parent.AfterAttacking -= this.Apply;
        }
    }
}
