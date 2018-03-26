/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 05:30 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using System.Drawing;
using Hedra.Engine.Rendering.UI;
using System.Collections.Generic;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of Skydome.
	/// </summary>
	public sealed class Skydome
	{
		public bool Enabled = true;
		public int Segments {get; private set;}
		public SkydomeShader SkydomeShader = new SkydomeShader("Shaders/Skydome.vert","Shaders/Skydome.frag");
		public VBO<Vector3> Vertices {get; private set;}
		public VBO<Vector3> Normals {get; private set;}
		public VBO<Vector2> UVs {get; private set;}
		public VBO<ushort> Indices {get; private set;}
		public Vector4 TopColor = new Vector4(Mathf.ToVector4(Color.CornflowerBlue));
		public Vector4 BotColor = new Vector4(Mathf.ToVector4(Color.LightYellow));
	    private int _previousShader;

        public Skydome(int Segments){
			this.Segments = Segments;
			this.Generate();
		}
		
		public void Draw(){
			if(!Enabled) return;

		    //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Blend);
			_previousShader = GraphicsLayer.ShaderBound;
			SkydomeShader.Bind();
			
			GL.Uniform4(SkydomeShader.TopColorUniformLocation, TopColor);
			GL.Uniform4(SkydomeShader.BotColorUniformLocation, BotColor);
			GL.Uniform1(SkydomeShader.HeightUniformLocation, (float) GameSettings.Height);

		    DrawManager.UIRenderer.SetupQuad();
		    DrawManager.UIRenderer.DrawQuad();

            GL.UseProgram(_previousShader);
			GraphicsLayer.ShaderBound = _previousShader;
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}
		
		public void Generate(){
			Vector3[] vertices = new Vector3[Segments * (Segments - 1) + 2];
		    Vector2[] uvs = new Vector2[Segments * (Segments - 1) + 2];
		    ushort[] Elements = new ushort[2 * Segments * (Segments - 1) * 3];
		 
		    double deltaLatitude = Math.PI / Segments;
		    double deltaLongitude = Math.PI * 2.0 / Segments;
		    int index = 0;
		 
		    // create the rings of the dome using polar coordinates
		    for (int i = 1; i < Segments; i++)
		    {
		        double r0 = Math.Sin(i * deltaLatitude);
		        double y0 = Math.Cos(i * deltaLatitude);
		 
		        for (int j = 0; j < Segments; j++)
		        {
		            double x0 = r0 * Math.Sin(j * deltaLongitude);
		            double z0 = r0 * Math.Cos(j * deltaLongitude);
		 
		            vertices[index] = new Vector3( (float) x0, (float) y0, (float) z0);
		            uvs[index++] = new Vector2(0, 1.0f - (float)y0);
		        }
		    }
		 
		    // create the top of the dome
		    vertices[index] = new Vector3(0, 1, 0);
		    uvs[index++] = new Vector2(0, 0);
		 
		    // create the bottom of the dome
		    vertices[index] = new Vector3(0, -1, 0);
		    uvs[index] = new Vector2(0, 2);
		 
		    // create the faces of the rings
		    index = 0;
		    for (int i = 0; i < Segments - 2; i++)
		    {
		        for (int j = 0; j < Segments; j++)
		        {
		        	Elements[index++] = (ushort) (Segments * i + j);
		        	Elements[index++] = (ushort) (Segments * i + (j + 1) % Segments);
		        	Elements[index++] = (ushort) (Segments * (i + 1) + (j + 1) % Segments);
		        	Elements[index++] = (ushort) (Segments * i + j);
		        	Elements[index++] = (ushort) (Segments * (i + 1) + (j + 1) % Segments);
		            Elements[index++] = (ushort) (Segments * (i + 1) + j);
		        }
		    }
		 
		    // create the faces of the top of the dome
		    for (int i = 0; i < Segments; i++)
		    {
		    	Elements[index++] = (ushort) (Segments * (Segments - 1));
		    	Elements[index++] = (ushort) ((i + 1) % Segments);
		        Elements[index++] = (ushort) i;
		    }
		 
		    // create the faces of the bottom of the dome
		    for (int i = 0; i < Segments; i++)
		    {
		    	Elements[index++] = (ushort) (Segments * (Segments - 1) + 1);
		    	Elements[index++] = (ushort) (Segments * (Segments - 2) + i);
		    	Elements[index++] = (ushort) (Segments * (Segments - 2) + (i + 1) % Segments);
		    }
		 
		    Vector3[] normals = Mathf.CalculateNormals(vertices, Elements);
		    
		    this.Vertices = new VBO<Vector3>(vertices, vertices.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
		    this.Indices = new VBO<ushort>(Elements, Elements.Length * sizeof(ushort), VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
		}
	}
}