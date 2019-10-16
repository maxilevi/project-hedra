/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 07:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    /// <inheritdoc />
    /// <summary>
    /// Description of OldManAIComponent.
    /// </summary>
    public class FollowAIComponent : EntityComponent
    {
        public Entity ToFollow = null;
        public bool DoLogic = true;
        public FollowAIComponent(Entity Parent) : base(Parent){}
        public FollowAIComponent(Entity Parent, Entity ToFollow) : base(Parent){
            this.ToFollow = ToFollow;
        }
        
        public override void Update(){
            if(ToFollow == null || !DoLogic) return;

            if ((ToFollow.Position - Parent.Position).LengthSquared() >
                GeneralSettings.UpdateDistanceSquared * .5f)
                Parent.Position = ToFollow.Position;

            if( (ToFollow.Position - Parent.Position).LengthSquared() > 8*8 )
            {                
                Parent.Orientation = (ToFollow.Position - Parent.Position).Xz().NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
                Parent.Physics.Move();
            }
        }
    }
}
