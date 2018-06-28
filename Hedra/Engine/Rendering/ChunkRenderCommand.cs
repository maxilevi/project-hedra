﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/10/2017
 * Time: 06:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of ChunkRenderCommand.
	/// </summary>
	internal class ChunkRenderCommand
	{
		public int DrawCount, VertexCount;
		public int ByteOffset => Entries[0].Offset;
	    public MemoryEntry[] Entries;
	}
}
