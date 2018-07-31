using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player
{
    public interface IPlayer : ISearchable
    {
        Vector3 Position { get; set; }
    }
}
