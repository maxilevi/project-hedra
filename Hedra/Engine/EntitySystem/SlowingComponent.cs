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
        private readonly float _totalDamage;
        private readonly Entity _damager;
        private float _oldSpeed;
        private float _pTime;

        public SlowingComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent){
            this._totalTime = TotalTime;
            this._totalDamage = TotalDamage;
            this._damager = Damager;
            CoroutineManager.StartCoroutine(this.UpdateEffect);
        }

        public override void Update() { }

        public IEnumerator UpdateEffect()
        {
            _oldSpeed = Parent.Speed;
            Parent.Model.BaseTint = new Vector4(-.5f, -.5f, -.5f, 1);
            Parent.Speed *= _totalDamage / 100;
            while (_totalTime > _pTime && !Parent.IsDead && !Disposed)
            {

                _pTime += Time.ScaledFrameTimeSeconds;
                yield return null;
            }
            Parent.Speed = _oldSpeed;
            Parent.Model.BaseTint = Vector4.Zero;
            Parent.RemoveComponent(this);
        }
    }
}
