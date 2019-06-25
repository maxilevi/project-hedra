/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/08/2016
 * Time: 07:50 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of MageAIComponent.
    /// </summary>
    public class MageAIComponent : RangedAIComponent
    {
        private const int MinDistance = 24;
        private bool _hasNearTargets;
        private Timer _updateTimer;
        
        public MageAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
            _updateTimer = new Timer(.25f);
        }

        public override void Update()
        {
            base.Update();
            if(_updateTimer.Tick())
                _hasNearTargets = ChasingTarget != null && (ChasingTarget.Position - Parent.Position).LengthSquared < MinDistance * MinDistance;
        }

        protected override bool CanUseSecondAttack => _hasNearTargets;
    }
}
