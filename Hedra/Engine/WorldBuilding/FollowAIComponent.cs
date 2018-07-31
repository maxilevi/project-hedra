/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 07:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.WorldBuilding
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

		    if ((ToFollow.Position - Parent.Position).LengthSquared >
		        GameSettings.UpdateDistance * GameSettings.UpdateDistance * .5f * .5f)
		        Parent.Position = ToFollow.Position;

			if( (ToFollow.Position - Parent.Position).LengthSquared > 8*8 ){
				
				Parent.Orientation = (ToFollow.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
				Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
				Parent.Physics.DeltaTranslate( Parent.Speed * 8 * Parent.Orientation);
				
				Parent.Model.Run();
			}else{
				Parent.Model.Idle();
			}
		}
	}
}
