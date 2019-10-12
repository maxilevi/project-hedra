using Hedra.Engine.Player;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem
{
    public interface ISkilledAnimableEntity : ISkilledEntity, IObjectWithAnimation, IObjectWithMovement, IObjectWithWeapon
    {
        Vector3 LookingDirection { get; }
    }
}