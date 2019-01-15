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
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    /// <summary>
    /// Description of MoveCommand.
    /// </summary>
    public struct MoveCommand
    {
        public Vector3 Delta;
        public bool OnlyY;
        public bool IsRecursive;
        
        public MoveCommand(Vector3 Delta, bool OnlyY = false)
        {
            this.Delta = Delta;
            this.OnlyY = OnlyY;
            this.IsRecursive = false;
        }
    }
}
