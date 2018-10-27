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
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class FreezeComponent : EntityComponent, IEffectComponent
    {
        public int Chance { get; set; } = 10;
        public float Damage { get; set; } = 30;
        public float Duration { get; set; } = 5;
        
        public FreezeComponent(IEntity Parent) : base(Parent) {
            Parent.AfterAttacking += this.Apply;
        }
        
        public override void Update(){}

        public void Apply(IEntity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                if (Victim.SearchComponent<FreezingComponent>() == null)
                    Victim.AddComponent(new FreezingComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
            }
        }

        public override void Dispose()
        {
            Parent.AfterAttacking -= this.Apply;
        }
    }
}
