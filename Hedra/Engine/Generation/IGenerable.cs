/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/02/2017
 * Time: 04:23 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of IGenerable.
	/// </summary>
	public interface IGenerable
	{
		float Generate(float WSeed, int _x, int _y);
	}
}
