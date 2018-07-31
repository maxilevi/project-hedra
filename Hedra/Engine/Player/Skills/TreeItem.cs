/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/08/2016
 * Time: 11:43 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Hedra.Engine.Player.Skills
{
	public class TreeItem
	{
		public Type AbilityType { get; set; }
	    public bool Locked { get; set; }
	    public int Level { get; set; }
	    public uint Image { get; set; }
	    public bool Enabled { get; set; }
	}
}
