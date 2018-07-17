/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/06/2017
 * Time: 02:29 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Plateau.
	/// </summary>
	internal class Plateau
	{
		public Vector3 Position { get; set; }
		public float Height { get; set; }
	    public float Radius { get; set; }
	    public float MaxHeight { get; set; }

	    public Plateau(Vector3 Position, float Radius, float Height, float MaxHeight){
			this.Position = Position;
			this.Radius = Radius;
		    this.Height = Height;
			this.MaxHeight = MaxHeight;
		}
	}
}
