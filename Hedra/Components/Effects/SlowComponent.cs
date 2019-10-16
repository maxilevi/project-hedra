/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/12/2016
 * Time: 10:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Components.Effects
{
    /// <inheritdoc cref="ApplyEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class SlowComponent : ApplyEffectComponent
    {
        protected override void DoApply(IEntity Victim, float Amount)
        {
            if (Victim.SearchComponent<SlowingComponent>() == null)
                Victim.AddComponent(new SlowingComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
        }

        public SlowComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity, Chance, Damage, Duration)
        {
        }
    }
}
