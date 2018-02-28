﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/01/2017
 * Time: 05:49 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.QuestSystem.Objectives
{
	/// <summary>
	/// Description of DummyObjective.
	/// </summary>
	public class DummyObjective : Objective
	{
		public DummyObjective(){}
		
		public override bool ShouldDisplay {
			get {return false;}
		}
		
		public override void Setup(Engine.Generation.Chunk UnderChunk){}
		
		public override void Recreate(){}
		
		public override uint QuestLogIcon => 0;

	    public override void Dispose()
		{
			base.Disposed = true;
		}
		
		public override string Description => "";
	}
}
