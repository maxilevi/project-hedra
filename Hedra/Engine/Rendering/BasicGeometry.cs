
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
	internal static class BasicGeometry
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
	    static int[] indices = new int[8]{0,1,2,3,4,5,6,7};	    
	    
		public static void DrawBox(Vector3 Min, Vector3 Size){
	    	Renderer.Disable(EnableCap.CullFace);
	    	Color color = Color.Transparent;
	    	float w = Size.X, h = Size.Y, d = Size.Z;
			v[0,0] = v[1,0] = v[2,0] = v[3,0] = 0;
	        v[4,0] = v[5,0] = v[6,0] = v[7,0] = w;
	        v[0,1] = v[1,1] = v[4,1] = v[5,1] = 0;
	        v[2,1] = v[3,1] = v[6,1] = v[7,1] = h;
	        v[0,2] = v[3,2] = v[4,2] = v[7,2] = 0;
	        v[1,2] = v[2,2] = v[5,2] = v[6,2] = d;
	        int i;
	        GL.PushMatrix();
	        //GL.Scale(Size);
	        GL.Translate(Min);
	        GL.Begin(PrimitiveType.Quads);
    		for (i = 5; i >= 0; i--) {
	            GL.Color3(color);
	            GL.Vertex3(ref v[faces[i, 0], 0]);
	           	GL.Color3(color);
	            GL.Vertex3(ref v[faces[i, 1], 0]);
	            GL.Color3(color);
	            GL.Vertex3(ref v[faces[i, 2], 0]);
	            GL.Color3(color);
	            GL.Vertex3(ref v[faces[i, 3], 0]);
    		}
	       GL.End();
	       GL.PopMatrix();
	       Renderer.Enable(EnableCap.CullFace);
	    }

	    public static void DrawShapes(CollisionShape[] Shapes, Color DrawColor)
	    {
	        for (var i = 0; i < Shapes.Length; i++)
	        {
	            BasicGeometry.DrawShape(Shapes[i], DrawColor);
	        }
	    }

	    public static void DrawShape(CollisionShape Shape, Color DrawColor){
	    	Renderer.Disable(EnableCap.CullFace);
	    	GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

	        GL.PointSize(6f);
            if (Shape.Indices.Length > 0)
	        {
	            GL.Begin(PrimitiveType.Triangles);
	            for (var j = 0; j < Shape.Indices.Length; j++)
	            {
	                GL.Color3(DrawColor);
	                GL.Vertex3(Shape.Vertices[(int) Shape.Indices[j]]);
	            }
	        }
	        else
	        {
	            GL.Begin(PrimitiveType.Points);
	            for (var j = 0; j < Shape.Vertices.Length; j++)
	            {
	                GL.Color3(DrawColor);
	                GL.Vertex3(Shape.Vertices[j]);
	            }
            }
	        GL.End();
			
			Renderer.Enable(EnableCap.CullFace);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
	    }
		
		public static void DrawPlane(int size, System.Drawing.Color color){
			GL.PushMatrix();
			GL.Rotate(270f, new OpenTK.Vector3(1,0,0));
			GL.Begin(PrimitiveType.Quads);
				GL.Color3(color);
				GL.Vertex3( 1*size, 1*size, 0f);
				GL.Color3(color);
				GL.Vertex3(-1*size, 1*size, 0f);
				GL.Color3(color);
				GL.Vertex3(-1*size, -1f*size, 0f);
				GL.Color3(color);
				GL.Vertex3( 1*size, -1f*size, 0f);
			GL.End();
			GL.PopMatrix();
		}
	    public static void DrawPlaneWithTexture(int size, uint TexID){
			GL.PushMatrix();
			
			GL.Rotate(270f, new OpenTK.Vector3(1,0,0));
			Renderer.Enable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.BindTexture(TextureTarget.Texture2D, TexID);

			GL.TexCoord2(1,1);
			GL.Vertex2( 10, 10);
			GL.TexCoord2(0,1);
			GL.Vertex2(0, 10);
			GL.TexCoord2(0,0);
			GL.Vertex2(0, 0);
			GL.TexCoord2(1,0);
			GL.Vertex2( 10, 0);
	
			GL.End();
			Renderer.Disable(EnableCap.Texture2D);
			Renderer.Enable(EnableCap.CullFace);
			
			GL.PopMatrix();
		}
	    
	    public static void DrawPoint(Vector3 Point){
	    	GL.Begin(PrimitiveType.Points);
	    	
	    	GL.Vertex3(Point);
	    
	    	GL.End();
	    }
	    
	    public static void DrawLine(Vector3 Point1, Vector3 Point2){
	    	GL.Begin(PrimitiveType.Lines);
	    	
	    	GL.Vertex3(Point1);
	    	GL.Vertex3(Point2);
	    	
	    	GL.End();
	    }
	}
}
