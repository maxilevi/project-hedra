/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.EntitySystem
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
                this.End();
                return;
            }
                    
            Parent.Model.BaseTint = Colors.LightBlue * new Vector4(1,1,2,1) * .7f;
            Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, -Parent.Speed),
                () => _totalTime > _pTime && !Parent.IsDead && !Disposed);
            Parent.Model.Pause = true;
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
                this.End();
            }
        }

        private void End()
        {
            Parent.Model.BaseTint = Vector4.Zero;
            Parent.Model.Pause = false;
            this.Dispose();
            Parent.RemoveComponent(this);
        }
    }
}
