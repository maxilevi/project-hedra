/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of TreeBlueprint.
	/// </summary>
	public abstract class TreeBlueprint
	{
		public TreeSlot[][] Slots = new TreeSlot[3][];
		public Vector4 ActiveColor;
		
		public TreeBlueprint(){
			for(int i = 0; i < Slots.Length; i++){
				Slots[i] = new TreeSlot[4];
				for(int j = 0; j < Slots[i].Length; j++){
					Slots[i][j] = new TreeSlot();
				}
			}
		}
	}
}
