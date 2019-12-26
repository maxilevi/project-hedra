/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Components.Effects
{
    /// <summary>
    /// Description of BurningComponent.
    /// </summary>
    public class PoisonComponent : DamagingEffectComponent
    {
        private float _time;
        private int _pTime;
        public override DamageType DamageType => DamageType.Poison;
        
        public PoisonComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent, TotalTime, TotalDamage, Damager)
        {
            RoutineManager.StartRoutine(UpdatePoison);
        }

        public override void Update()
        {
        }

        private IEnumerator UpdatePoison()
        {
            Parent.Model.BaseTint = Colors.PoisonGreen *new Vector4(1,3,1,1);
            while(TotalTime > _pTime && !Parent.IsDead && !Disposed){
                
                _time += Core.Time.DeltaTime;
                if(_time >= 1){
                    _pTime++;
                    _time = 0;
                    Damage();
                }
                
                yield return null;
            }
            Parent.Model.BaseTint = Vector4.Zero;
            this.Dispose();
            Parent.RemoveComponent(this);
        }
    }
}
