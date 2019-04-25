using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public interface ISkilledAnimableEntity : ISkilledEntity, IObjectWithAnimation, IObjectWithMovement, IObjectWithWeapon
    {
        Vector3 LookingDirection { get; }
    }
}