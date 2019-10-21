using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class BanditAIBehaviour : CombatAIBehaviour
    {
        public BanditAIBehaviour(IHumanoid Parent) : base(Parent)
        {
        }

        public override Vector3 FindPoint()
        {
            return new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) +
                   Parent.Position;
        }

        public override IEntity FindPlayerTarget(float SearchRadius)
        {
            var player = GameManager.Player;
            return !player.IsDead && (player.Position.Xz() - Parent.Position.Xz()).LengthSquared() < SearchRadius * player.Attributes.MobAggroModifier * SearchRadius * player.Attributes.MobAggroModifier
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
                if (entities[i] == this.Parent || !((entities[i].Position.Xz() - Parent.Position.Xz()).LengthSquared() < SearchRadius * SearchRadius)) continue;
                if (entities[i].IsStatic 
                    || Array.IndexOf(Ignore, entities[i]) != -1
                    || entities[i] is IPlayer                       
                    || entities[i].IsImmune 
                    || entities[i].IsFriendly 
                    || entities[i].IsInvisible) continue;

                return entities[i];
            }
            return null;
        }

        public override float WaitTime => Utils.Rng.NextFloat() * 4 + 6.0f;
    }
}