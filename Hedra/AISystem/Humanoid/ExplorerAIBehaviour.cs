using System;
using System.Linq;
using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class ExplorerAIBehaviour : BanditAIBehaviour
    {
        private readonly IHumanoid[] _crew;
        private readonly Random _rng;
        private float _angle;

        public ExplorerAIBehaviour(IHumanoid Parent, IHumanoid[] Crew, Random Rng) : base(Parent)
        {
            _crew = Crew;
            _rng = Rng;
            _angle = _rng.NextFloat() * 360;
            Parent.SearchComponent<CombatAIComponent>().IgnoreEntities = _crew;
        }

        private IHumanoid Leader => _crew.First(C => !C.Disposed && !C.IsDead);

        public override float WaitTime => 0;

        public override Vector3 FindPoint()
        {
            return Leader == Parent
                ? Vector3.Transform(Vector3.UnitZ * 32, Matrix4x4.CreateRotationY(_angle * Mathf.Radian)) +
                  Parent.Position
                : _crew[Array.IndexOf(_crew, Parent) - 1].Position;
        }

        public override IEntity FindMobTarget(float SearchRadius)
        {
            return base.FindMobTarget(SearchRadius, _crew.Cast<IEntity>().ToArray());
        }

        public override void OnStare(IEntity Entity)
        {
            for (var i = 0; i < _crew.Length; ++i)
            {
                if (_crew[i].IsDead || _crew[i].Disposed) continue;
                if ((Entity.Position - _crew[i].Position).LengthSquared() > Math.Pow(CombatAIComponent.StareRadius, 2))
                {
                    var component = _crew[i].SearchComponent<CombatAIComponent>();
                    component?.WalkTo(Entity.Position);
                }
            }
        }

        public override void SetTarget(IEntity Entity)
        {
            for (var i = 0; i < _crew.Length; ++i)
            {
                if (_crew[i].IsDead || _crew[i].Disposed) continue;
                var component = _crew[i].SearchComponent<CombatAIComponent>();
                if (component != null)
                    if (!component.IsChasing)
                        component.SetTarget(Entity);
            }
        }

        public override void OnStuck()
        {
            _angle = _rng.NextFloat() * 360;
        }
    }
}