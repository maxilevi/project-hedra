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
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class SlowComponent : EntityComponent, IEffectComponent
    {
        public int Chance { get; set; } = 15;
        public float TotalStrength { get; set; } = 60;
        public float BaseTime { get; set; } = 5;

	    private float _cooldown;
		private bool _canSlow = true;
		
		public SlowComponent(Entity Parent) : base(Parent)
		{
            Parent.OnAttacking += this.Apply;
		}

        public override void Update() { }

        public void Apply(Entity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                if (Victim.SearchComponent<SlowingComponent>() == null)
                    Victim.AddComponent(new SlowingComponent(Victim, Parent, BaseTime + Utils.Rng.NextFloat() * 4 - 2f, TotalStrength));
            }
        }

        public override void Dispose()
        {
            Parent.OnAttacking -= this.Apply;
        }
    }
}
