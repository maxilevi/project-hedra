using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class BanditAIBehaviour : CombatAIBehaviour
    {
        public BanditAIBehaviour(IHumanoid Parent) : base(Parent)
        {
        }

        public override float WaitTime => Utils.Rng.NextFloat() * 4 + 6.0f;

        public override Vector3 FindPoint()
        {
            return new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) +
                   Parent.Position;
        }

        public override IEntity FindPlayerTarget(float SearchRadius)
        {
            var player = GameManager.Player;
            return !player.IsDead
                   && (player.Position.Xz() - Parent.Position.Xz()).LengthSquared() < SearchRadius *
                   player.Attributes.MobAggroModifier * SearchRadius * player.Attributes.MobAggroModifier
                   && !Parent.Physics.StaticRaycast(player.Position + Vector3.UnitY * player.Model.Height)
                ? player
                : null;
        }

        public override IEntity FindMobTarget(float SearchRadius)
        {
            return FindMobTarget(SearchRadius);
        }

        protected IEntity FindMobTarget(float SearchRadius, params IEntity[] Ignore)
        {
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
            {
                if (entities[i] == Parent || !((entities[i].Position.Xz() - Parent.Position.Xz()).LengthSquared() <
                                               SearchRadius * SearchRadius)) continue;
                if (entities[i].IsStatic
                    || Array.IndexOf(Ignore, entities[i]) != -1
                    || entities[i] is IPlayer
                    || entities[i] == LocalPlayer.Instance.Companion.Entity
                    || entities[i].IsImmune
                    || entities[i].IsInvisible) continue;

                return entities[i];
            }

            return null;
        }
    }
}