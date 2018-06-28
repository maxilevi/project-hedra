/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/09/2016
 * Time: 10:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using System.Collections;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of PoisonousComponent.
    /// </summary>
    internal class PoisonousComponent : EntityComponent, IEffectComponent
    {
		public int Chance { get; set; } = 10;
		public float Damage { get; set; } = 30;
		public float Duration { get; set; } = 5;

		public PoisonousComponent(Entity Parent) : base(Parent){
			Parent.OnAttacking += this.Apply;
		}
		
		public override void Update(){}

        public void Apply(Entity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                if (Victim.SearchComponent<PoisonComponent>() == null)
                    Victim.AddComponent(new PoisonComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
            }
        }

        public override void Dispose()
        {
            Parent.OnAttacking -= this.Apply;
        }
    }
}
