/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/11/2016
 * Time: 09:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using System.Collections.Generic;

namespace Hedra.Engine
{
	/// <summary>
	/// Description of GJKCollision.
	/// </summary>
	public static class GJKCollision
	{
		private static Vector3 Support(List<Vector3> Vertices, Vector3 Direction){
		    float highest = float.MinValue;
		    Vector3 support = Vector3.Zero;
		
		    for (int i = 0; i < Vertices.Count; ++i) {
		        Vector3 v = Data.Vertices[i];
		        float dot = Vector3.Dot(Direction, v);
		
		        if (dot > highest) {
		            highest = dot;
		            support = v;
		        }
		    }
		
		    return support;
		}
		
		public static bool Collides(List<Vector3> Vertices){
			Vector3 D = -Vertices[0].NormalizedFast();
			Vector3 S = Support(D) - Support(-D);
			
			List<Vector3> Points = new List<Vector3>();
			Points.Add(S);
			
			D = -S.NormalizedFast(); // -D
			
			while(true){
				Vector3 A = Support(D);
				if( Vector3.Dot(A, D) < 0 ) return false;
				
				Points.Add(A);
				
				if( DoSimplex(Points, D) ) return true;
			}
		}
		
		private static bool DoSimplex(List<Vector3> Array, Vector3 Direction){
			
			if(Array.Count == 4) return true;
			
			
		}
	}
}
