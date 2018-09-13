/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.WorldBuilding
{
	/// <inheritdoc />
	/// <summary>
	/// Description of Structure.
	/// </summary>
	public class BaseStructure : IDisposable, IStructure, ISearchable
    {
        public virtual Vector3 Position { get; set; }
		public bool Disposed { get; protected set; }
		
		public virtual void Dispose()
		{
			Disposed = true;
		}
	}
}
