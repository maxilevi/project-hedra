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
	public class Plateau
	{
		public Vector3 Position;
		public float Height = 64, Radius = 64, MaxHeight = 32;
		
		public Plateau(Vector3 Position, float Radius, float Height, float MaxHeight){
			this.Position = Position;
			this.Radius = Radius;
			this.Height = Height;
			this.MaxHeight = MaxHeight;
		}
	}
}
