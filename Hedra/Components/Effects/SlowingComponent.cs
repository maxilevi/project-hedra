using System.Collections;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class SlowingComponent : EntityComponent
    {
        private readonly IEntity _damager;
        private readonly float _slowPercentage;
        private readonly float _totalTime;
        private float _pTime;

        public SlowingComponent(IEntity Parent, IEntity Damager, float TotalTime, float SlowPercentage) : base(Parent)
        {
            _totalTime = TotalTime;
            _slowPercentage = SlowPercentage;
            _damager = Damager;
            RoutineManager.StartRoutine(UpdateEffect);
        }

        public override void Update()
        {
        }

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