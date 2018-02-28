/*
 * Author: Zaphyk
 * Date: 06/02/2016
 * Time: 12:50 a.m.
 *
 */
using System;
using System.Collections;

namespace VoxelShift.Engine.Generation
{
	/// <summary>
	/// An interface for Chunk Generators to implement.
	/// </summary>
	public interface IGenerator
	{
		void Generate();
	}
}
