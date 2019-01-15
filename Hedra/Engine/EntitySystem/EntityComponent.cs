/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:50 a.m.
 *
 */

using System.Reflection;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc />
    /// <summary>
    /// Description of EntityComponent.
    /// </summary>
    public abstract class EntityComponent : Component<IEntity>, IUpdatable
    {
        protected EntityComponent(IEntity Entity) : base(Entity)
        {
        }
    }
}
