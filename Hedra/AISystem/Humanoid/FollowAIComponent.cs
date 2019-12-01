/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 07:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.AISystem.Behaviours;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    /// <inheritdoc />
    /// <summary>
    /// Description of OldManAIComponent.
    /// </summary>
    public class FollowAIComponent : BasicAIComponent
    {
        private readonly FollowBehaviour _follow;
        public FollowAIComponent(IEntity Parent, IEntity ToFollow) : base(Parent)
        {
            _follow = new FollowBehaviour(Parent)
            {
                Target = ToFollow
            };
        }
        
        public override void Update()
        {
            _follow.Update();
        }

        public override AIType Type => AIType.Neutral;
        
        public override void Dispose()
        {
            base.Dispose();
            _follow.Dispose();
        }
    }
}
