using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player
{
    public interface IPlayer : ISearchable, IEntity
    {
        IMessageDispatcher MessageDispatcher { get; }
        Vector3 Position { get; set; }
        float Health { get; set; }
        float Mana { get; set; }
        float XP { get; set; }
        int Level { get; }
    }
}
