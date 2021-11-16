using System.Numerics;
using Hedra.Engine.Player;

namespace Hedra.Engine.SkillSystem
{
    public interface ISkilledAnimableEntity : ISkilledEntity, IObjectWithAnimation, IObjectWithMovement,
        IObjectWithWeapon
    {
        Vector3 LookingDirection { get; }
    }
}