/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:50 a.m.
 *
 */
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of EntityComponent.
	/// </summary>
	public abstract class EntityComponent : IUpdatable
	{	
		protected bool Disposed;
		protected Entity Parent;
	    public bool DrawsUI;
	    protected EntityComponent(Entity Entity){
			this.Parent = Entity;
		}
		
		public abstract void Update();
		
		public virtual void Draw(){}
		
		public virtual void Dispose(){
			this.Disposed = true;
		}
	}
}
