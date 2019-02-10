using System;
using System.Linq;
using Hedra.Core;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public class ExplorerAIBehaviour : BanditAIBehaviour
    {
        private readonly IHumanoid[] _crew;
        private IHumanoid Leader => _crew.First(C => !C.Disposed && !C.IsDead);
        private readonly float _angle;
        
        public ExplorerAIBehaviour(IHumanoid Parent, IHumanoid[] Crew, int Seed) : base(Parent)
        {
            _crew = Crew;
            _angle = new Random(Seed).NextFloat() * 360;
            Parent.SearchComponent<CombatAIComponent>().IgnoreEntities = _crew;
        }

        public override Vector3 FindPoint()
        {
            return Leader == Parent
                ? Vector3.TransformPosition(Vector3.UnitZ * 32, Matrix4.CreateRotationY(_angle * Mathf.Radian)) +
                  Parent.Position
                : _crew[Array.IndexOf(_crew, Parent) - 1].BlockPosition;
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

        public override float WaitTime => 0;
    }
}