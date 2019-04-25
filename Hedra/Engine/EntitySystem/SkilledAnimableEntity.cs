using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.SkillSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public class SkilledAnimableEntity : SkilledEntity, ISkilledAnimableEntity
    {
        public Animation AnimationBlending => default(Animation);
        
        public void ResetModel()
        {
        }

        public void BlendAnimation(Animation Animation)
        {
        }

        public bool CaptureMovement { get; set; }
        
        public void Orientate()
        {
        }

        public bool IsAttacking { get; set; }
        public bool WasAttacking { get; set; }
        public bool InAttackStance { get; set; }
        public Vector3 LookingDirection => Orientation;
    }
}