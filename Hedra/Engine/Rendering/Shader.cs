/*
 * Author: Zaphyk
 * Date: 06/02/2016
 * Time: 05:23 a.m.
 *
 */
using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{

	/// <summary>
	/// A class which combines vertex and fragment shaders and compiles them.
	/// For how to write a glsl shader https://www.opengl.org/registry/doc/GLSLangSpec.Full.1.20.8.pdf
	/// </summary>
	public class Shader : IDisposable
	{
	    protected int ShaderVID;
	    protected int ShaderFID;
	    protected int ShaderGID;
		public int ShaderID;
		public int ClipPlaneLocation;
		public int LightColorLocation;
		public int LightPositionLocation;
		public int[] PointLightsColorUniform;
		public int[] PointLightsPositionUniform;
		public int[] PointLightsRadiusUniform;
		
		public Shader(string FileSourceV, string FileSourceF) : this(FileSourceV, null, FileSourceF){}
		
		public Shader(string FileSourceV, string FileSourceG, string FileSourceF)
		{
		    ShaderData dataV = null;
		    ShaderData dataG = null;
		    ShaderData dataF = null;

            dataV = new ShaderData
            {
                Name = FileSourceV.Remove(0, FileSourceV.LastIndexOf("/", StringComparison.Ordinal) + 1),
                Source = AssetManager.ReadShader(FileSourceV)
            };

            if (FileSourceG != null)
            {
                dataG = new ShaderData
                {
                    Name = FileSourceG.Remove(0, FileSourceG.LastIndexOf("/", StringComparison.Ordinal) + 1),
                    Source = AssetManager.ReadShader(FileSourceG)
                };
            }

            dataF = new ShaderData
            {
                Name = FileSourceF.Remove(0, FileSourceF.LastIndexOf("/", StringComparison.Ordinal) + 1),
                Source = AssetManager.ReadShader(FileSourceF)
            };

            this.CompileShaders(dataV, dataG, dataF);
		}

	    public Shader(ShaderData DataV, ShaderData DataG, ShaderData DataF)
	    {
	        this.CompileShaders(DataV, DataG, DataF);
	    }

	    private void CompileShaders(ShaderData DataV, ShaderData DataG, ShaderData DataF)
	    {
	        this.Compile(out ShaderVID, DataV.Source, DataV.Name, ShaderType.VertexShader);
	        this.Compile(out ShaderFID, DataF.Source, DataF.Name, ShaderType.FragmentShader);

            if(DataG != null)
	            this.Compile(out ShaderGID, DataG.Source, DataG.Name, ShaderType.GeometryShader);

	        this.Combine();

	        DisposeManager.Add(this);
        }

	    private void Compile(out int ID, string Source, string Name,ShaderType Type){
			ID = GL.CreateShader(Type);
			GL.ShaderSource(ID,  Source);
			GL.CompileShader(ID);
			int result = -1;
			string log = "No log available.";
			GL.GetShader(ID, ShaderParameter.CompileStatus, out result);
			GL.GetShaderInfoLog(ID,out log);
			if(log == "") log = GL.GetError().ToString();
			if(result != 1)
				Log.WriteResult((result == 1), "Shader "+ Name +" has compiled" + " | "+log+" | "+result);
		}
		
		public virtual void Combine(){
			ShaderID = GL.CreateProgram();
			
			if(ShaderVID != 0)
				GL.AttachShader(ShaderID, ShaderVID);
			if(ShaderGID != 0)
				GL.AttachShader(ShaderID, ShaderGID);
			if(ShaderFID != 0)
				GL.AttachShader(ShaderID, ShaderFID);

			GL.LinkProgram(ShaderID);
			
			if(ShaderVID != 0)
				GL.DetachShader(ShaderID, ShaderVID);
			if(ShaderGID != 0)
				GL.DetachShader(ShaderID, ShaderGID);
			if(ShaderFID != 0)
				GL.DetachShader(ShaderID, ShaderFID);
			
			if(ShaderVID != 0)
				GL.DeleteShader(ShaderVID);
			if(ShaderGID != 0)
				GL.DeleteShader(ShaderGID);
			if(ShaderFID != 0)
				GL.DeleteShader(ShaderFID);
			
			GetUniformsLocations();
			ShaderManager.RegisterShader(this);
		}
		
		public virtual void GetUniformsLocations(){
			
		}
		
		public void Bind(){
			if(GraphicsLayer.ShaderBound == ShaderID) return;
			
			GL.UseProgram(ShaderID);
			GraphicsLayer.ShaderBound = ShaderID;
		}
		
		public void UnBind(){
			GL.UseProgram(0);
			GraphicsLayer.ShaderBound = 0;
		}
		
		public void Dispose(){
			GL.DeleteProgram(ShaderID);
		}
	}

    public class ShaderData
    {
        public string Source;
        public string Name;
    }
}