using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.WorldBuilding
{
    public sealed class BearTrap : WorldObject
    {
        private static readonly VertexData TrapModel;
        private readonly float _damage;
        private readonly List<Predicate<IEntity>> _ignore;
        private readonly bool _stun;
        private readonly Timer _timer;

        static BearTrap()
        {
            TrapModel = AssetManager.PLYLoader("Assets/Env/BearTrap.ply", Vector3.One * 2.5f);
        }

        public BearTrap(IEntity Parent, Vector3 Position, float Duration, float Damage, bool Stun) : base(Parent)
        {
            Model = ObjectMesh.FromVertexData(TrapModel.Clone());
            _damage = Damage;
            _stun = Stun;
            _timer = new Timer(Duration);
            _ignore = new List<Predicate<IEntity>>();
            this.Position = Position;
        }

        public override void Update()
        {
            base.Update();
            /*var heightAtPosition = Physics.HeightAtPosition(Position.X, Position.Z);
            Position = new Vector3(
                Position.X,
                heightAtPosition,
                Position.Z
            );*/
            HandleCollision();
            if (_timer.Tick()) Dispose();
        }

        private void HandleCollision()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count - 1; ++i)
            {
                if (entities[i] == Parent) continue;
                if (_ignore.Any(X => X(entities[i]))) continue;
                if ((Position - entities[i].Position).LengthSquared() < 6 * 6)
                {
                    Activate(entities[i]);
                    break;
                }
            }
        }

        public void Ignore(Predicate<IEntity> Predicate)
        {
            _ignore.Add(Predicate);
        }

        private void Activate(IEntity Target)
        {
            if (_stun) Target.KnockForSeconds(5);
            SoundPlayer.PlaySound(SoundType.BearTrap, Target.Position);
            Target.Damage(_damage, Parent, out var exp);
            if (Parent is IHumanoid human) human.XP += exp;
            Dispose();
        }
    }
}