using System;
using System.Linq;
using Hedra.AISystem.Behaviours;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class FeedableCattleAIComponent : CattleAIComponent
    {
        public const int FoodFollowRadius = 48;
        private readonly Timer _timer;

        public FeedableCattleAIComponent(IEntity Parent) : base(Parent)
        {
            _timer = new Timer(0.25f);
            Follow = new FollowBehaviour(Parent);
        }

        protected FollowBehaviour Follow { get; }

        public override void Update()
        {
            if (!Herd.Enabled)
            {
                if (Follow.Enabled)
                    Follow.Update();
                else
                    base.Update();
            }
            else
            {
                base.Update();
            }

            SetFollowTarget();
        }

        private void SetFollowTarget()
        {
            if (!_timer.Tick()) return;
            var nearby = World.InRadius<IHumanoid>(Parent.Position, FoodFollowRadius)
                .FirstOrDefault(H => string.Equals(H.MainWeapon?.Name, CommonItems.AnimalFood.ToString(),
                    StringComparison.InvariantCultureIgnoreCase));
            Follow.Target = nearby;
        }
    }
}