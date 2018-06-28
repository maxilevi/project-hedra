/*
 * Author: Zaphyk
 * Date: 21/03/2016
 * Time: 12:20 a.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of LineData.
	/// </summary>
	internal class LineData : DataContainer
	{
		public LineData(Vector3 Origin, Vector3 End, Vector4 Color, int LOD)
		{
			List<Vector3> VertexData = new List<Vector3>();
			Vector3 LODVec = (End - Origin) / LOD;
			
			VertexData.Add(Origin);
			for(int i = 0; i < LOD; i++){
				VertexData.Add( Origin + ( LODVec * (i - 1) ) );
				VertexData.Add( Origin + ( LODVec * (i + 1) ) );
			}
			VertexData.Add(End);
			
			this.VerticesArrays = VertexData.ToArray();
			for(uint i = 0; i < VerticesArrays.Length; i++){
				Indices.Add(i);
				if(i != 0 && i != VerticesArrays.Length-1)
					Indices.Add(i);
			}
			
			List<Vector4> Colors = new List<Vector4>();
			for(int i = 0; i < VerticesArrays.Length; i++){
				Colors.Add(new Vector4(Color.Xyz, (uint) (Indices[i]) ));
			}
			this.Color = Colors.ToArray();
			
			this.Position = Position;
			this.TransformVerts(Position);
		}
	}
}
