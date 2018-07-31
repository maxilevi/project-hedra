/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/09/2017
 * Time: 01:02 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Rendering
{
	public struct DrawElementsIndirectCommand{
	    public uint  count;
	    public uint  primCount;
	    public uint  firstIndex;
	    public int   baseVertex;
	    public uint  baseInstance;
	}
}
