/*
 * Author: Zaphyk
 * Date: 06/02/2016
 * Time: 05:23 a.m.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Shaders;
using OpenTK;

namespace Hedra.Engine.Rendering
{

	/// <summary>
	/// A class which combines vertex and fragment shaders and compiles them.
	/// For how to write a glsl shader https://www.opengl.org/registry/doc/GLSLangSpec.Full.1.20.8.pdf
	/// </summary>
	public class Shader : IDisposable
	{
	    private readonly Dictionary<string, UniformMapping> _mappings;
	    private readonly Dictionary<string, UniformArray> _arrayMappings;
        private readonly ShaderData _vertexShader;
	    private readonly ShaderData _fragmentShader;
	    private readonly ShaderData _geometryShader;
        protected int ShaderVid { get; private set; }
        protected int ShaderFid { get; private set; }
        protected int ShaderGid { get; private set; }
        public int ShaderId { get; private set; }
		public int ClipPlaneLocation { get; set; }
        public int LightColorLocation { get; set; }
        public int LightPositionLocation { get; set; }
        public int[] PointLightsColorUniform { get; set; }
        public int[] PointLightsPositionUniform { get; set; }
        public int[] PointLightsRadiusUniform { get; set; }

	    public static Shader Build(string FileSourceV, string FileSourceF)
	    {
	        var vertexShader = new ShaderData
	        {
	            Name = FileSourceV.Remove(0, FileSourceV.LastIndexOf("/", StringComparison.Ordinal) + 1),
	            Source = AssetManager.ReadShader(FileSourceV),
                Path = FileSourceV
	        };

	        var fragmentShader = new ShaderData
	        {
	            Name = FileSourceF.Remove(0, FileSourceF.LastIndexOf("/", StringComparison.Ordinal) + 1),
	            Source = AssetManager.ReadShader(FileSourceF),
                Path = FileSourceF
	        };
	        return Shader.Build(vertexShader, null, fragmentShader);
	    }

	    public static Shader Build(ShaderData DataV, ShaderData DataG, ShaderData DataF)
	    {
	        return new Shader(DataV, DataG, DataF);
	    }


	    private Shader(ShaderData DataV, ShaderData DataG, ShaderData DataF)
	    {
	        _vertexShader = DataV;
	        _geometryShader = DataG;
	        _fragmentShader = DataF;
            _mappings = new Dictionary<string, UniformMapping>();
	        _arrayMappings = new Dictionary<string, UniformArray>();
	        this.CompileShaders(_vertexShader, _geometryShader, _fragmentShader);
        }

	    private bool UpdateSource()
	    {
	        var updated = false;
	        var sources = new[] { _vertexShader, _geometryShader, _fragmentShader };
	        for (var i = 0; i < sources.Length; i++)
	        {
	            if (sources[i] == null) continue;
	            var newSource = sources[i].SourceFinder();
	            if (newSource != sources[i].Source)
	            {
	                Log.WriteLine($"Shader source '{sources[i].Name}' has been updated. ");
	                sources[i].Source = sources[i].SourceFinder();
	                updated = true;
	            }
	        }
	        return updated;
	    }

	    public void Reload()
	    {
	        if (ShaderId == 0) throw new ArgumentException($"Cannot rebuild a non existent shader");
	        if (this.UpdateSource())
	        {
	            _mappings.Clear();
	            _arrayMappings.Clear();
	            this.CompileShaders(_vertexShader, _geometryShader, _fragmentShader);
	        }
	    }

	    private void AddArrayMappings(UniformArray[] Array)
	    {
	        for (var i = 0; i < Array.Length; i++)
	        {
	            _arrayMappings.Add(Array[i].Key, Array[i]);
	        }
	    }

	    private static void Compile(out int ID, string Source, string Name, ShaderType Type)
	    {
	        ID = GL.CreateShader(Type);
	        GL.ShaderSource(ID, Source);
	        GL.CompileShader(ID);
	        int result = -1;
	        string log = "No log available.";
	        GL.GetShader(ID, ShaderParameter.CompileStatus, out result);
	        GL.GetShaderInfoLog(ID, out log);
	        if (log == "") log = GL.GetError().ToString();
	        if (result != 1) Log.WriteResult(result == 1, "Shader " + Name + " has compiled" + " | " + log + " | " + result);
	    }

        private void CompileShaders(ShaderData DataV, ShaderData DataG, ShaderData DataF)
	    {
            if(ShaderId != 0) GL.DeleteProgram(ShaderId);
	        int shadervid = -1;
	        int shaderfid = -1;
	        int shadergid = -1;
	        if (DataV != null) Compile(out shadervid, DataV.Source, DataV.Name, ShaderType.VertexShader);
	        if (DataF != null) Compile(out shaderfid, DataF.Source, DataF.Name, ShaderType.FragmentShader);
            if (DataG != null) Compile(out shadergid, DataG.Source, DataG.Name, ShaderType.GeometryShader);
	        this.ShaderVid = shadervid;
	        this.ShaderFid = shaderfid;
	        this.ShaderGid = shadergid;

            this.Combine();

	        DisposeManager.Add(this);
        }
		
		private void Combine(){
			ShaderId = GL.CreateProgram();

            if (ShaderVid > 0) GL.AttachShader(ShaderId, ShaderVid);              
            if (ShaderGid > 0) GL.AttachShader(ShaderId, ShaderGid);
            if (ShaderFid > 0) GL.AttachShader(ShaderId, ShaderFid);

            GL.LinkProgram(ShaderId);
		    
            if (ShaderVid > 0) GL.DetachShader(ShaderId, ShaderVid);		               
            if (ShaderGid > 0) GL.DetachShader(ShaderId, ShaderGid);		               
            if (ShaderFid > 0) GL.DetachShader(ShaderId, ShaderFid);
		    
            if (ShaderVid > 0) GL.DeleteShader(ShaderVid);	                
            if (ShaderGid > 0) GL.DeleteShader(ShaderGid);		                
            if (ShaderFid > 0) GL.DeleteShader(ShaderFid);

		    var parser = new ShaderParser(_vertexShader.Source);
		    this.AddArrayMappings(parser.ParseUniformArrays(ShaderId));

		    parser.Source = _fragmentShader.Source;
		    this.AddArrayMappings(parser.ParseUniformArrays(ShaderId));

		    if (_geometryShader != null)
		    {
		        parser.Source = _geometryShader.Source;
		        this.AddArrayMappings(parser.ParseUniformArrays(ShaderId));
		    }

            ShaderManager.RegisterShader(this);
		}

	    public object this[string Key]
	    {
	        get => _arrayMappings.ContainsKey(Key) ? _arrayMappings[Key].Values : _mappings[Key].Value;
	        set
	        {
	            if (_arrayMappings.ContainsKey(Key))
	            {
	                var array = ((IEnumerable) value).Cast<object>().ToArray();
                    _arrayMappings[Key].Load(array);
	            }
	            else
	            {
                    if(value.GetType().IsArray) throw new ArgumentException($"Uniform mapping for array {Key} of type {value.GetType().Name} could not be found.");
	                if (!_mappings.ContainsKey(Key))
	                {
	                    var location = GL.GetUniformLocation(ShaderId, Key);
#if DEBUG
                        if(location == -1) throw new ArgumentException($"Uniform {Key} does not exist in shader");
#endif
	                    _mappings.Add(Key, new UniformMapping(location, value));
	                }
                    if(this.ShaderId != GraphicsLayer.ShaderBound) throw new ArgumentException($"Uniforms need to be uploaded when the owner's shader is bound.");
	                _mappings[Key].Value = value;
	                Shader.LoadMapping(_mappings[Key]); 
	            }
	        }
	    }

        public static void LoadMapping(UniformMapping Mapping)
	    {
	        switch (Mapping.Type)
	        {
	            case MappingType.Integer:
	                GL.Uniform1(Mapping.Location, (int)Mapping.Value);
                    break;
	            case MappingType.Double:
	                GL.Uniform1(Mapping.Location, (double)Mapping.Value);
	                break;
	            case MappingType.Float:
	                GL.Uniform1(Mapping.Location, (float)Mapping.Value);
	                break;
                case MappingType.Vector2:
	                GL.Uniform2(Mapping.Location, (Vector2)Mapping.Value);
                    break;
	            case MappingType.Vector3:
	                GL.Uniform3(Mapping.Location, (Vector3)Mapping.Value);
                    break;
	            case MappingType.Vector4:
	                GL.Uniform4(Mapping.Location, (Vector4)Mapping.Value);
                    break;
	            case MappingType.Matrix4:
	                var matrix4 = (Matrix4) Mapping.Value;
                    GL.UniformMatrix4(Mapping.Location, false, ref matrix4);
	                break;
	            case MappingType.Matrix4X3:
	                var matrix4X3 = (Matrix4x3)Mapping.Value;
	                GL.UniformMatrix4x3(Mapping.Location, false, ref matrix4X3);
                    break;
	            case MappingType.Matrix4X2:
	                var matrix4X2 = (Matrix4x2)Mapping.Value;
	                GL.UniformMatrix4x2(Mapping.Location, false, ref matrix4X2);
                    break;
	            case MappingType.Matrix3X4:
	                var matrix3X4 = (Matrix3x4)Mapping.Value;
	                GL.UniformMatrix3x4(Mapping.Location, false, ref matrix3X4);
                    break;
	            case MappingType.Matrix3X2:
	                var matrix3X2 = (Matrix3x2)Mapping.Value;
	                GL.UniformMatrix3x2(Mapping.Location, false, ref matrix3X2);
                    break;
	            case MappingType.Matrix2X4:
	                var matrix2X4 = (Matrix2x4)Mapping.Value;
	                GL.UniformMatrix2x4(Mapping.Location, false, ref matrix2X4);
                    break;
	            case MappingType.Matrix3:
	                var matrix3X3 = (Matrix3)Mapping.Value;
	                GL.UniformMatrix3(Mapping.Location, false, ref matrix3X3);
                    break;
	            case MappingType.Matrix2X3:
	                var matrix2X3 = (Matrix2x3)Mapping.Value;
	                GL.UniformMatrix2x3(Mapping.Location, false, ref matrix2X3);
                    break;
	            case MappingType.Matrix2:
	                var matrix2 = (Matrix2)Mapping.Value;
	                GL.UniformMatrix2(Mapping.Location, false, ref matrix2);
                    break;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	    }

		public void Bind(){
			if(GraphicsLayer.ShaderBound == ShaderId) return;
		    if (ShaderId < 0) throw new GraphicsException($"{this.GetType().Name} is corrupt. {this.ShaderId}");

            GL.UseProgram(ShaderId);
			GraphicsLayer.ShaderBound = ShaderId;
        }
		
		public void UnBind(){
			GL.UseProgram(0);
			GraphicsLayer.ShaderBound = 0;
		}
		
		public void Dispose(){
			GL.DeleteProgram(ShaderId);
		}
	}
}