using System.Collections;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Components.Effects
{
    public class SlowingComponent : EntityComponent
    {
        private readonly float _totalTime;
        private readonly float _slowPercentage;
        private readonly IEntity _damager;
        private float _pTime;

        public SlowingComponent(IEntity Parent, IEntity Damager, float TotalTime, float SlowPercentage) : base(Parent){
            this._totalTime = TotalTime;
            this._slowPercentage = SlowPercentage;
            this._damager = Damager;
            CoroutineManager.StartCoroutine(this.UpdateEffect);
        }

        public override void Update() { }

        private IEnumerator UpdateEffect()
        {
            Parent.Model.BaseTint = new Vector4(-.75f, -.75f, -.75f, 1);
            var newSpeed = Parent.Speed * _slowPercentage / 100f;
            Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, newSpeed - Parent.Speed),
                () => _totalTime > _pTime && !Parent.IsDead && !Disposed);
            Parent.Model.AnimationSpeed = newSpeed;
            while (_totalTime > _pTime && !Parent.IsDead && !Disposed)
            {

                _pTime += Time.DeltaTime;
                yield return null;
            }
            Parent.Model.AnimationSpeed = 1f;
            Parent.Model.BaseTint = Vector4.Zero;
            Parent.RemoveComponent(this);
        }
    }
}
