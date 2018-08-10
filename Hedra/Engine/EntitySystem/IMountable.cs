/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/01/2017
 * Time: 04:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.EntitySystem
{
	public interface IMountable
	{
		bool IsMountable{ get; }
        bool HasRider { get; set; }
	}
}
