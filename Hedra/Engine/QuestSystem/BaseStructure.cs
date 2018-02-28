﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Generation;
using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.QuestSystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of Structure.
	/// </summary>
	public class BaseStructure : IDisposable, IStructure
	{
        public Vector3 Position { get; set; }
		protected bool Disposed;
		
		public virtual void Dispose(){
			Disposed = true;
		}
	}
}
