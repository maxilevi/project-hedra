using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public abstract class QuestComponent : SingularComponent<QuestComponent, IHumanoid>
    {
        protected QuestComponent(IHumanoid Entity) : base(Entity)
        {
        }
    }
}