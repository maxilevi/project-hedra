/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Components.Effects
{
    /// <summary>
    /// Description of BurningComponent.
    /// </summary>
    public class FreezingComponent : EntityComponent
    {
        private readonly float _totalTime;
        private float _time;
        private readonly float _totalDamage;
        private int _pTime;
        private readonly IEntity _damager;
        
        public FreezingComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent)
        {
            _totalTime = TotalTime;
            _totalDamage = TotalDamage;
            _damager = Damager;
            this.Start();
        }

        private void Start()
        {
            var burning = Parent.SearchComponent<BurningComponent>();
            if (burning != null)
            {
                Parent.RemoveComponent(burning);
                Parent.RemoveComponent(this);
            }
            else
            {
                Parent.Model.BaseTint = Colors.LightBlue * new Vector4(1, 1, 3, 1) * .7f;
                Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, -Parent.Speed),
                    () => _totalTime > _pTime && !Parent.IsDead && !Disposed);
                Parent.Model.Pause = true;
            }
        }
        
        public override void Update()
        {
            _time += Time.DeltaTime;
            if (!(_time >= 1)) return;
            _pTime++;
            _time = 0;
            Parent.Damage( (_totalDamage / _totalTime), _damager, out var exp);
            if(_damager != null && _damager is IHumanoid humanoid)
                humanoid.XP += exp;

            if (!(_totalTime > _pTime && !Parent.IsDead && !Disposed))
            {
                Parent.RemoveComponent(this);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Parent.Model.BaseTint = Vector4.Zero;
            Parent.Model.Pause = false;
        }
    }
}
