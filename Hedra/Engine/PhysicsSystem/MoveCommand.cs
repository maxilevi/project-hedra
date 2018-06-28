/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/09/2017
 * Time: 04:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
	/// <summary>
	/// Description of MoveCommand.
	/// </summary>
	internal struct MoveCommand
	{
		public Vector3 Delta;
		public Entity Parent;
	    public bool IsRecursive;
		
		public MoveCommand(Entity Parent, Vector3 Delta){
			this.Delta = Delta;
			this.Parent = Parent;
		    this.IsRecursive = false;

		}
	}
}
