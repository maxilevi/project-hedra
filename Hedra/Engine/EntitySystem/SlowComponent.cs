/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/12/2016
 * Time: 10:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using OpenTK;
using System.Collections;
 using Hedra.Core;
 using Hedra.Engine.Management;
 using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class SlowComponent : EntityComponent, IEffectComponent
    {
        private readonly IEntity _parent;
        public int Chance { get; set; } = 15;
        public float Damage { get; set; } = 60;
        public float Duration { get; set; } = 5;

        private float _cooldown;
        private bool _canSlow = true;
        
        public SlowComponent(IEntity Parent) : base(Parent)
        {
            _parent = Parent;
            Parent.AfterAttacking += this.Apply;
        }

        public override void Update() { }

        public void Apply(IEntity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                if (Victim.SearchComponent<SlowingComponent>() == null)
                    Victim.AddComponent(new SlowingComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
            }
        }

        public override void Dispose()
        {
            Parent.AfterAttacking -= this.Apply;
        }
    }
}
