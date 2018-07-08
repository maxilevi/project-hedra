/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/12/2016
 * Time: 03:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of Occludable.
	/// </summary>
	internal abstract class Occludable
	{
		public Vector3 OccluMin, OccluSize;
		protected int OcclusionQuery;
		public bool Occluded {get; protected set;}
		protected OcclusionState State;
		
		public Occludable(){
			Executer.ExecuteOnMainThread( () => OcclusionQuery = GL.GenQuery() );
		}
		
		public void DrawQuery(){
			//Not initalized yet
			if(OccluMin == Vector3.Zero && OccluSize == Vector3.Zero || OcclusionQuery == 0) return;
			
	        int Passed = int.MaxValue;
	        int Available = 0;
	
	        if(State == OcclusionState.WAITING)
	        	GL.GetQueryObject(OcclusionQuery, GetQueryObjectParam.QueryResultAvailable, out Available);
	
	        if(Available != 0)
	        {
	          Passed = 0;
	          GL.GetQueryObject(OcclusionQuery, GetQueryObjectParam.QueryResult, out Passed);
	          State = (Passed != 0) ? OcclusionState.VISIBLE : OcclusionState.HIDDEN;
	          Occluded = (Passed == 0);
	        }
			
			if(State != OcclusionState.WAITING && !GameSettings.LockFrustum)
			{
			    State = OcclusionState.WAITING;
			    GL.BeginQuery(QueryTarget.AnySamplesPassed, OcclusionQuery);
			    
			    BasicGeometry.DrawBox(OccluMin, OccluSize);
				
			    GL.EndQuery(QueryTarget.AnySamplesPassed);
			}
		}
		
		public void Dispose(){
			Executer.ExecuteOnMainThread( () => GL.DeleteQuery( OcclusionQuery ) );
		}
	}
}
