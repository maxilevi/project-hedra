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
        bool Enabled { get; set; }
        bool PrematureCulling { get; }
        Vector3 Position { get; set; }
        Box CullingBox { get; set; }
    }
}