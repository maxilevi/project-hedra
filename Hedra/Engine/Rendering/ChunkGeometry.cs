﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/09/2017
 * Time: 11:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of ChunkGeometry.
	/// </summary>
	public static class ChunkGeometry
	{
		private static Dictionary<int, byte[]> Geometry = new Dictionary<int, byte[]>();
		
		public static void Get(){}
		public static void Add(){}
		public static void Remove(){}
		public static void Clear(){}
		
		private static int m_Length = 0;
		public static int Length{
			get{ return m_Length; }
		}
	}
}
