/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/12/2016
 * Time: 04:54 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Campfire.
	/// </summary>
	public class Cage : BaseStructure, IUpdatable
	{
		public Cage(Vector3 Position) : base() {
			base.Position = Position;
		}
		
		public void Update(){}
	}
}
