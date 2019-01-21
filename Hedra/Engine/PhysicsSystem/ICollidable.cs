/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/11/2016
 * Time: 06:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    /// <summary>
    /// Description of ICollidable.
    /// </summary>
    public interface ICollidable
    {
        float BroadphaseRadius { get; }
        Vector3 BroadphaseCenter { get; }
        CollisionShape AsShape();
        Box AsBox();
        CollisionGroup AsGroup();

    }
}
