﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of MeshData.
	/// </summary>
	public class ModelData
	{
		public Vector3[] JointIds {get; private set;}
		public Vector3[] VertexWeights {get; private set;}
		public float FurthestPoint {get; private set;}
		public Vector3[] Vertices {get; private set;}
		public Vector3[] Colors {get; private set;}
		public Vector3[] Normals {get; private set;}
		public uint[] Indices {get; private set;}
	
		public ModelData(Vector3[] Vertices, Vector3[] Colors, Vector3[] Normals, uint[] Indices, Vector3[] JointIds, Vector3[] VertexWeights, float FurthestPoint) {
			this.Vertices = Vertices;
			this.Colors = Colors;
			this.Normals = Normals;
			this.Indices = Indices;
			this.JointIds = JointIds;
			this.VertexWeights = VertexWeights;
			this.FurthestPoint = FurthestPoint;
		}
		
		public VertexData ToVertexData(){
			VertexData Data = new VertexData();
			Data.Vertices = this.Vertices.ToList();
			Data.Normals = this.Normals.ToList();
			Data.Indices = this.Indices.ToList();
			Data.Colors = new List<Vector4>();
			
			this.Colors.ToList().ForEach( Color => Data.Colors.Add( new Vector4(Color.X, Color.Y, Color.Z, 1f) ) );
			
			return Data;
		}
	}
}
