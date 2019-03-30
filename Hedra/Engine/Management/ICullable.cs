/*
 * Author: Zaphyk
 * Date: 03/02/2016
 * Time: 11:58 p.m.
 *
 */
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Management
{
    public interface ICullable
    {
        bool Enabled { get; }
        bool PrematureCulling { get; }
        bool WasCulled { set; }
        Vector3 Position { get; }
        Vector3 Max { get; }
        Vector3 Min { get; }
    }
}