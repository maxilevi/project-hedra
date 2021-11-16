using System;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Sound;

namespace Hedra.AISystem
{
    public abstract class CattleAIComponent : BasicAIComponent
    {
        protected CattleAIComponent(IEntity Parent) : base(Parent)
        {
            if (AlertTime == 0) throw new ArgumentOutOfRangeException($"AlertTime cannot be '{AlertTime}'");
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = AlertTime,
                Sound = Sound,
                Radius = Radius
            };
            Herd = new HerdBehaviour(Parent);
        }

        protected RoamBehaviour Roam { get; }
        protected HerdBehaviour Herd { get; }

        protected abstract float AlertTime { get; }
        protected abstract SoundType Sound { get; }
        protected virtual float Radius => 80;
        public override AIType Type => AIType.Neutral;

        public override void Update()
        {
            if (!Herd.Enabled)
                Roam.Update();
            else
                Herd.Update();
        }

        public override void Draw()
        {
            base.Draw();
            if (GameSettings.DebugAI)
            {
                DrawDebugCollision(Parent);
                if (!Herd.Enabled)
                    Roam.Draw();
                else
                    Herd.Draw();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Roam.Dispose();
            Herd.Dispose();
        }
    }
}