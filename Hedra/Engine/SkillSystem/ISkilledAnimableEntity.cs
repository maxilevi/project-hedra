using Hedra.Engine.Player;
using System.Numerics;

namespace Hedra.Engine.SkillSystem
{
    public interface ISkilledAnimableEntity : ISkilledEntity, IObjectWithAnimation, IObjectWithMovement, IObjectWithWeapon
    {
        Vector3 LookingDirection { get; }
    }
}