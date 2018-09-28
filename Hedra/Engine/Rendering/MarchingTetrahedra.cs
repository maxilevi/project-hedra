/*
 * Author: Zaphyk
 * Date: 06/04/2016
 * Time: 09:10 p.m.
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Hedra.Engine.Rendering.Geometry;
using OpenTK;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of MarchingTetraHedra.
	/// </summary>
	public static class MarchingTetraHedra
	{
        
         public static MarchingData Process(double IsoLevel, GridCell Cell, Vector4 Color)
        {
            MarchingData MData = new MarchingData(Color);
            MData.VerticesArrays = new Vector3[]{};
            MData.Normals = new Vector3[]{};
            MData.Indices = new List<uint>();
            MData.Color = new Vector4[]{};
            MData = Build(MData, PolygoniseTri(Cell,IsoLevel,0,2,3,7));
            MData = Build(MData, PolygoniseTri(Cell,IsoLevel,0,2,6,7));
            MData = Build(MData, PolygoniseTri(Cell,IsoLevel,0,4,6,7));
            MData = Build(MData, PolygoniseTri(Cell,IsoLevel,0,6,1,2));
            MData = Build(MData, PolygoniseTri(Cell,IsoLevel,0,6,1,4));
	        MData = Build(MData, PolygoniseTri(Cell,IsoLevel,5,6,1,4));
            return MData;
        }
        
        private static Triangle[] PolygoniseTri(GridCell Cell,double IsoLevel, int v0,int v1,int v2,int v3)
		{
		   int TriIndex;
		   Triangle[] Tri = new Triangle[2];
		   Tri[0].P = new Vector3[3];
		   Tri[1].P = new Vector3[3];
		   /*
		      Determine which of the 16 cases we have Celliven which vertices
		      are above or below the IsoLevelsurface
		   */
		   TriIndex = 0;
		   if (Cell.Density[v0] > IsoLevel) TriIndex |= 1;
		   if (Cell.Density[v1] > IsoLevel) TriIndex |= 2;
		   if (Cell.Density[v2] > IsoLevel) TriIndex |= 4;
		   if (Cell.Density[v3] > IsoLevel) TriIndex |= 8;
		
		   /* Form the vertices of the TrianCellles for each case */
		   switch (TriIndex) {
		   case 0x00:
		   case 0x0F:
		      break;
		   case 0x0E:
		   case 0x01:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v1], Cell.Density[v0], Cell.Density[v1]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v2], Cell.Density[v0], Cell.Density[v2]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v3], Cell.Density[v0], Cell.Density[v3]);
		      
		      break;
		   case 0x0D:
		   case 0x02:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v0], Cell.Density[v1], Cell.Density[v0]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v3], Cell.Density[v1], Cell.Density[v3]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v2], Cell.Density[v1], Cell.Density[v2]);
		      
		      break;
		   case 0x0C:
		   case 0x03:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v3], Cell.Density[v0], Cell.Density[v3]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v2], Cell.Density[v0], Cell.Density[v2]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v3], Cell.Density[v1], Cell.Density[v3]);
		      
		      Tri[1].P[0] = Tri[0].P[2];
		      Tri[1].P[1] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v2], Cell.Density[v1], Cell.Density[v2]);
		      Tri[1].P[2] = Tri[0].P[1];
		      
		      break;
		   case 0x0B:
		   case 0x04:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v2], Cell.P[v0], Cell.Density[v2], Cell.Density[v0]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v2], Cell.P[v1], Cell.Density[v2], Cell.Density[v1]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v2], Cell.P[v3], Cell.Density[v2], Cell.Density[v3]);
		      
		      break;
		   case 0x0A:
		   case 0x05:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v1], Cell.Density[v0], Cell.Density[v1]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v2], Cell.P[v3], Cell.Density[v2], Cell.Density[v3]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v3], Cell.Density[v0], Cell.Density[v3]);
		      
		      Tri[1].P[0] = Tri[0].P[0];
		      Tri[1].P[1] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v2], Cell.Density[v1], Cell.Density[v2]);
		      Tri[1].P[2] = Tri[0].P[1];
		      
		      break;
		   case 0x09:
		   case 0x06:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v1], Cell.Density[v0], Cell.Density[v1]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v1], Cell.P[v3], Cell.Density[v1], Cell.Density[v3]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v2], Cell.P[v3], Cell.Density[v2], Cell.Density[v3]);
		      
		      Tri[1].P[0] = Tri[0].P[0];
		      Tri[1].P[1] = VertexInterp(IsoLevel, Cell.P[v0], Cell.P[v2], Cell.Density[v0], Cell.Density[v2]);
		      Tri[1].P[2] = Tri[0].P[2];
		      
		      break;
		   case 0x07:
		   case 0x08:
		      Tri[0].P[0] = VertexInterp(IsoLevel, Cell.P[v3], Cell.P[v0], Cell.Density[v3], Cell.Density[v0]);
		      Tri[0].P[1] = VertexInterp(IsoLevel, Cell.P[v3], Cell.P[v2], Cell.Density[v3], Cell.Density[v2]);
		      Tri[0].P[2] = VertexInterp(IsoLevel, Cell.P[v3], Cell.P[v1], Cell.Density[v3], Cell.Density[v1]);
		      
		      break;
		   }

        	return Tri;
        }
       
      	private static Vector3 VertexInterp(double IsoLevel, Vector3 P1, Vector3 P2, double valp1, double valp2)
        {
           Vector4 p1 = new Vector4(P1, (float) valp1);
      		Vector4 p2 = new Vector4(P2, (float) valp2);
     		
           if (p2.Length < p1.Length){
		        Vector4 temp;
	        	temp = p1;
		        p1 = p2;
	        	p2 = temp;    
		    }
		
		    Vector3 p;
		    if(Math.Abs(p1.W - p2.W) > 0.00001)
		    	p = p1.Xyz + (p2.Xyz - p1.Xyz ) / (p2.W - p1.W)*( (float) IsoLevel - p1.W);
		    else 
		        p = p1.Xyz;
		    return p;
        }
 
        private static MarchingData Build(MarchingData Data, Triangle[] Triangles)
        {
        	List<Vector3> Vertices = Data.VerticesArrays.ToList();
        	List<Vector3> Normals = Data.Normals.ToList();
        	List<Vector4> Colors = Data.Color.ToList();
        	List<uint> Indices = Data.Indices;
            for (uint i = 0; i < Triangles.Length; i++)
            {
            	if(!IsEmpty(Triangles[i].P)){
	            	Indices.Add( (uint) Vertices.Count + 0);
	            	Indices.Add( (uint) Vertices.Count + 1);
	            	Indices.Add( (uint) Vertices.Count + 2);
	            	
	            	Vertices.Add(Triangles[i].P[0]);
	            	Vertices.Add(Triangles[i].P[1]);
	            	Vertices.Add(Triangles[i].P[2]);
	            	
	            	Vector3 Normal = Mathf.CrossProduct(Triangles[i].P[1] - Triangles[i].P[0], Triangles[i].P[2] - Triangles[i].P[0]).Normalized();
	            	Normals.Add(Normal);
	            	Normals.Add(Normal);
	            	Normals.Add(Normal);
	            	
	            	Colors.Add(Data.TemplateColor);
	            	Colors.Add(Data.TemplateColor);
	            	Colors.Add(Data.TemplateColor);
            	}     
            }
            Data.VerticesArrays = Vertices.ToArray();
            Data.Indices = Indices;
            Data.Color = Colors.ToArray();
            Data.Normals = Normals.ToArray();
            return Data;
        }
        
        private static bool IsEmpty(Vector3[] Array){
        	if(Array[0] == Vector3.Zero && Array[1] == Vector3.Zero && Array[2] == Vector3.Zero)
        		return true;
        	return false;
        }
	}
}
