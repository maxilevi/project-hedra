using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player
{
    internal interface IPlayer : ISearchable
    {
        Vector3 Position { get; set; }
    }
}
