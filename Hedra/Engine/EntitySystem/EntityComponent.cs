/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:50 a.m.
 *
 */

using System.Reflection;
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of EntityComponent.
	/// </summary>
	public abstract class EntityComponent : IUpdatable
	{	
        public bool Renderable { get; }
		protected bool Disposed { get; private set; }
		protected Entity Parent { get; set; }

	    protected EntityComponent(Entity Entity)
        {
			this.Parent = Entity;
            this.Renderable = this.GetType().GetMethod("Draw")?.DeclaringType != base.GetType().BaseType;
	    }
		
		public abstract void Update();
		
		public virtual void Draw(){}
		
		public virtual void Dispose()
        {
			this.Disposed = true;
		}
	}
}
