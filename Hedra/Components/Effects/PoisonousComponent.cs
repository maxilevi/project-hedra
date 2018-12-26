/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/09/2016
 * Time: 10:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    /// <inheritdoc cref="ApplyEffectComponent" />
    /// <summary>
    /// Description of PoisonousComponent.
    /// </summary>
    public class PoisonousComponent : ApplyEffectComponent
    {
        protected override void DoApply(IEntity Victim, float Amount)
        {
            if (Victim.SearchComponent<PoisonComponent>() == null)
                Victim.AddComponent(new PoisonComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
        }

        public PoisonousComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity, Chance, Damage, Duration)
        {
        }
    }
}
