/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Components.Effects
{
    /// <summary>
    ///     Description of BurningComponent.
    /// </summary>
    public class PoisonComponent : DamagingEffectComponent
    {
        private int _pTime;
        private float _time;

        public PoisonComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent,
            TotalTime, TotalDamage, Damager)
        {
            RoutineManager.StartRoutine(UpdatePoison);
        }

        public override DamageType DamageType => DamageType.Poison;

        public override void Update()
        {
        }

        private IEnumerator UpdatePoison()
        {
            Parent.Model.BaseTint = Colors.PoisonGreen * new Vector4(1, 3, 1, 1);
            while (TotalTime > _pTime && !Parent.IsDead && !Disposed)
            {
                _time += Time.DeltaTime;
                if (_time >= 1)
                {
                    _pTime++;
                    _time = 0;
                    Damage();
                }

                yield return null;
            }

            Parent.Model.BaseTint = Vector4.Zero;
            Dispose();
            Parent.RemoveComponent(this);
        }
    }
}