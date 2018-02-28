/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:50 a.m.
 *
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of EntityComponent.
	/// </summary>
	public abstract class EntityComponent
	{	
		protected bool Disposed = false;
		protected Entity Parent;
		public bool DrawsUI {get; set;}

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
