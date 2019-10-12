using System.Drawing;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using Hedra.WorldObjects;
using OpenToolkit.Mathematics;

namespace Hedra.AISystem.Behaviours
{
    public class GiantBeetleAttackBehaviour : BaseBeetleAttackBehaviour
    {
        private const int SpitAnimationIndex = 1;
        private const int BiteAnimationIndex = 0;
        
        public GiantBeetleAttackBehaviour(IEntity Parent) : base(Parent)
        {
        }
        
        protected override Animation GetBiteAnimation(QuadrupedModel Model) => Model.AttackAnimations[BiteAnimationIndex];

        protected override Animation GetSpitAnimation(QuadrupedModel Model) => Model.AttackAnimations[SpitAnimationIndex];

        protected override float SpitCooldown => 5;
        protected override bool HasSpit => true;
    }
}
