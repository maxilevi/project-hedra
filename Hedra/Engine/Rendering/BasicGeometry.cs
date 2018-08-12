
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of BasicGeometry.
	/// </summary>
	public static class BasicGeometry
	{
		
		private static CubeData Data;
		private static VBO<Vector3> CubeVerts;	
		private static VBO<uint> CubeIndices;
		static BasicGeometry(){
			Data = new CubeData();
			Data.AddFace(Face.ALL);
			CubeVerts = new VBO<Vector3>(Data.VerticesArrays, Data.VerticesArrays.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
			CubeIndices = new VBO<uint>(Data.Indices.ToArray(), Data.Indices.Count * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
		}
	
		static int[,] faces = new int[,]{
	            {0, 1, 2, 3},
	            {3, 2, 6, 7},
	            {7, 6, 5, 4},
	            {4, 5, 1, 0},
	            {5, 6, 2, 1},
	            {7, 4, 0, 3}
	        };
	    static float[,] v = new float[8,3];
	    static int[] mv = new int[24];
		static float[,] n = new float[,]{
	            {-1.0f, 0.0f, 0.0f},
	            {0.0f, 1.0f, 0.0f},
	            {1.0f, 0.0f, 0.0f},
	            {0.0f, -1.0f, 0.0f},
	            {0.0f, 0.0f, 1.0f},
	            {0.0f, 0.0f, -1.0f}
	    };

		public static void DrawBox(Vector3 Min, Vector3 Size)
        {
	    	Renderer.Disable(EnableCap.CullFace);
	    	Vector4 color = Colors.Transparent;
	    	float w = Size.X, h = Size.Y, d = Size.Z;
			v[0,0] = v[1,0] = v[2,0] = v[3,0] = 0;
	        v[4,0] = v[5,0] = v[6,0] = v[7,0] = w;
	        v[0,1] = v[1,1] = v[4,1] = v[5,1] = 0;
	        v[2,1] = v[3,1] = v[6,1] = v[7,1] = h;
	        v[0,2] = v[3,2] = v[4,2] = v[7,2] = 0;
	        v[1,2] = v[2,2] = v[5,2] = v[6,2] = d;
	        int i;
	        Renderer.PushMatrix();
	        //Renderer.Scale(Size);
	        Renderer.Translate(Min);
	        Renderer.Begin(PrimitiveType.Quads);
    		for (i = 5; i >= 0; i--) {
	            Renderer.Color3(color.Xyz);
	            Renderer.Vertex3(ref v[faces[i, 0], 0]);
	           	Renderer.Color3(color.Xyz);
	            Renderer.Vertex3(ref v[faces[i, 1], 0]);
	            Renderer.Color3(color.Xyz);
	            Renderer.Vertex3(ref v[faces[i, 2], 0]);
	            Renderer.Color3(color.Xyz);
	            Renderer.Vertex3(ref v[faces[i, 3], 0]);
    		}
	       Renderer.End();
	       Renderer.PopMatrix();
	       Renderer.Enable(EnableCap.CullFace);
	    }

	    public static void DrawShapes(CollisionShape[] Shapes, Vector4 DrawColor)
	    {
	        for (var i = 0; i < Shapes.Length; i++)
	        {
	            BasicGeometry.DrawShape(Shapes[i], DrawColor);
	        }
	    }

	    public static void DrawShape(CollisionShape Shape, Vector4 DrawColor){
	    	Renderer.Disable(EnableCap.CullFace);
	    	Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

	        Renderer.PointSize(6f);
            if (Shape.Indices.Length > 0)
	        {
	            Renderer.Begin(PrimitiveType.Triangles);
	            for (var j = 0; j < Shape.Indices.Length; j++)
	            {
	                Renderer.Color3(DrawColor);
	                Renderer.Vertex3(Shape.Vertices[(int) Shape.Indices[j]]);
	            }
	        }
	        else
	        {
	            Renderer.Begin(PrimitiveType.Points);
	            for (var j = 0; j < Shape.Vertices.Length; j++)
	            {
	                Renderer.Color3(DrawColor);
	                Renderer.Vertex3(Shape.Vertices[j]);
	            }
            }
	        Renderer.End();
			
			Renderer.Enable(EnableCap.CullFace);
			Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
	    }
		
		public static void DrawPlane(int size, Vector4 Color){
			Renderer.PushMatrix();
			Renderer.Rotate(270f, new OpenTK.Vector3(1,0,0));
			Renderer.Begin(PrimitiveType.Quads);
				Renderer.Color3(Color);
				Renderer.Vertex3(new Vector3(1 * size, 1 * size, 0f));
				Renderer.Color3(Color);
				Renderer.Vertex3(new Vector3(-1 * size, 1 * size, 0f));
				Renderer.Color3(Color);
				Renderer.Vertex3(new Vector3(-1 * size, -1f * size, 0f));
				Renderer.Color3(Color);
				Renderer.Vertex3(new Vector3(1 * size, -1f * size, 0f));
			Renderer.End();
			Renderer.PopMatrix();
		}

	    public static void DrawPoint(Vector3 Point){
	    	Renderer.Begin(PrimitiveType.Points);
	    	
	    	Renderer.Vertex3(Point);
	    
	    	Renderer.End();
	    }
	    
	    public static void DrawLine(Vector3 Point1, Vector3 Point2){
	    	Renderer.Begin(PrimitiveType.Lines);
	    	
	    	Renderer.Vertex3(Point1);
	    	Renderer.Vertex3(Point2);
	    	
	    	Renderer.End();
	    }
	}
}
