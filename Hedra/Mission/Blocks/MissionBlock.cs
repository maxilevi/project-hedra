using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Mission.Blocks
{
    public abstract class MissionBlock
    {
        public IPlayer Owner { get; set; }
        public IHumanoid Giver { get; set; }
        public abstract bool IsCompleted { get; }
        public abstract void Setup();
        public abstract void End();
        public abstract QuestView BuildView();
        public abstract bool HasLocation { get; }
        public virtual Vector3 Location { get; }
        public abstract string ShortDescription { get; }
        public abstract string Description { get; }
        public virtual void Dispose()
        {
            
        }
    }
}