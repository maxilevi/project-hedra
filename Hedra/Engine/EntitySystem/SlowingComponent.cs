using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public class SlowingComponent : EntityComponent
    {
        private readonly float _totalTime;
        private readonly float _slowPercentage;
        private readonly Entity _damager;
        private float _pTime;

        public SlowingComponent(Entity Parent, Entity Damager, float TotalTime, float SlowPercentage) : base(Parent){
            this._totalTime = TotalTime;
            this._slowPercentage = SlowPercentage;
            this._damager = Damager;
            CoroutineManager.StartCoroutine(this.UpdateEffect);
        }

        public override void Update() { }

        public IEnumerator UpdateEffect()
        {
            Parent.Model.BaseTint = new Vector4(-.75f, -.75f, -.75f, 1);
            Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, -Parent.Speed + Parent.Speed * _slowPercentage / 100),
                () => _totalTime > _pTime && !Parent.IsDead && !Disposed);
            while (_totalTime > _pTime && !Parent.IsDead && !Disposed)
            {

                _pTime += Time.ScaledFrameTimeSeconds;
                yield return null;
            }
            Parent.Model.BaseTint = Vector4.Zero;
            Parent.RemoveComponent(this);
        }
    }
}
