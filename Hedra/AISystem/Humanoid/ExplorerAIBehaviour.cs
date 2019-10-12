using System;
using System.Linq;
using Hedra.Core;
using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.AISystem.Humanoid
{
    public class ExplorerAIBehaviour : BanditAIBehaviour
    {
        private readonly IHumanoid[] _crew;
        private IHumanoid Leader => _crew.First(C => !C.Disposed && !C.IsDead);
        private float _angle;
        private readonly Random _rng;
        
        public ExplorerAIBehaviour(IHumanoid Parent, IHumanoid[] Crew, Random Rng) : base(Parent)
        {
            _rng = Rng;
            _crew = Crew;
            _angle = _rng.NextFloat() * 360;
            Parent.SearchComponent<CombatAIComponent>().IgnoreEntities = _crew;
        }

        public override Vector3 FindPoint()
        {
            return Leader == Parent
                ? Vector3.TransformPosition(Vector3.UnitZ * 32, Matrix4.CreateRotationY(_angle * Mathf.Radian)) +
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
                if(_crew[i].IsDead || _crew[i].Disposed) continue;
                if ((Entity.Position - _crew[i].Position).LengthSquared > Math.Pow(CombatAIComponent.StareRadius, 2))
                    _crew[i].SearchComponent<CombatAIComponent>().WalkTo(Entity.Position);
            }
        }
        
        public override void SetTarget(IEntity Entity)
        {
            for (var i = 0; i < _crew.Length; ++i)
            {
                if(_crew[i].IsDead || _crew[i].Disposed) continue;
                if (!_crew[i].SearchComponent<CombatAIComponent>().IsChasing)
                    _crew[i].SearchComponent<CombatAIComponent>().SetTarget(Entity);
            }
        }

        public override void OnStuck()
        {
            _angle = _rng.NextFloat() * 360;
        }

        public override float WaitTime => 0;
    }
}