/*
 * Author: Zaphyk
 * Date: 06/02/2016
 * Time: 05:23 a.m.
 *
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Shaders;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    /// <summary>
    ///     A class which combines vertex and fragment shaders and compiles them.
    ///     For how to write a glsl shader https://www.openRenderer.org/registry/doc/GLSLangSpec.Full.1.20.8.pdf
    /// </summary>
    public class Shader : IDisposable
    {
        private readonly Dictionary<string, UniformArray> _arrayMappings;
        private readonly ShaderData _fragmentShader;
        private readonly ShaderData _geometryShader;
        private readonly List<ShaderInput> _inputs;
        private readonly Dictionary<string, bool> _knownMappings;
        private readonly Dictionary<string, UniformMapping> _mappings;
        private readonly Dictionary<string, MappingType> _mappingTypes;
        private readonly List<ShaderOutput> _outputs;
        private readonly ShaderData _vertexShader;

        static Shader()
        {
            Passthrough = Build("Shaders/Passthrough.vert", "Shaders/Passthrough.frag");
        }


        private Shader(ShaderData DataV, ShaderData DataG, ShaderData DataF)
        {
            _vertexShader = DataV;
            _geometryShader = DataG;
            _fragmentShader = DataF;
            _mappingTypes = new Dictionary<string, MappingType>();
            _mappings = new Dictionary<string, UniformMapping>();
            _arrayMappings = new Dictionary<string, UniformArray>();
            _knownMappings = new Dictionary<string, bool>();
            _inputs = new List<ShaderInput>();
            _outputs = new List<ShaderOutput>();
            Name = $"<{DataV?.Name ?? string.Empty}>:<{DataF?.Name ?? string.Empty}>:<{DataG?.Name ?? string.Empty}>";
            CompileShaders(_vertexShader, _geometryShader, _fragmentShader);
        }

        public static Shader Passthrough { get; }
        protected int ShaderVid { get; private set; }
        protected int ShaderFid { get; private set; }
        protected int ShaderGid { get; private set; }
        public string Name { get; }
        public int ShaderId { get; private set; }
        public ShaderInput[] Inputs => _inputs.ToArray();
        public ShaderOutput[] Outputs => _outputs.ToArray();

        public object this[string Key]
        {
            get => _arrayMappings.ContainsKey(Key) ? _arrayMappings[Key].Values : _mappings[Key].Value;
            set
            {
                if (_arrayMappings.ContainsKey(Key))
                {
                    var asArray = (Array)value;
                    var pool = ArrayPool<object>.Shared;
                    var temp = pool.Rent(asArray.Length);
                    var i = 0;
                    foreach (var obj in asArray) temp[i++] = obj;
                    _arrayMappings[Key].Load(temp, asArray.Length);
                    pool.Return(temp);
                }
                else
                {
                    if (value.GetType().IsArray)
                        throw new ArgumentException(
                            $"Uniform mapping for array {Key} of type {value.GetType().Name} could not be found.");
                    if (!_mappings.ContainsKey(Key))
                    {
                        var location = Renderer.GetUniformLocation(ShaderId, Key);
#if DEBUG
                        if (location == -1) throw new ArgumentException($"Uniform {Key} does not exist in shader");
#endif
                        _mappings.Add(Key, new UniformMapping(location, value, _mappingTypes[Key]));
                    }
#if DEBUG
                    if (Renderer.GetInteger(GetPName.CurrentProgram) != ShaderId)
                        throw new ArgumentException("Uniforms need to be uploaded when the owner's shader is bound.");
#endif
                    if (ShaderId != Renderer.ShaderBound)
                        throw new ArgumentException("Uniforms need to be uploaded when the owner's shader is bound.");

                    if (_mappings[Key].Value != value || !_mappings[Key].Loaded)
                    {
                        _mappings[Key].Value = value;
                        _mappings[Key].Loaded = true;
                        LoadMapping(_mappings[Key]);
                    }
                }
            }
        }

        public void Dispose()
        {
            ShaderManager.UnregisterShader(this);
            Renderer.DeleteProgram(ShaderId);
        }

        public static event ShaderChangeEvent ShaderChanged;
        public event ShaderChangeEvent ShaderReloaded;

        public static Shader GetById(uint Id)
        {
            return ShaderManager.GetById(Id);
        }

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
            return Build(vertexShader, null, fragmentShader);
        }

        public static Shader Build(ShaderData DataV, ShaderData DataG, ShaderData DataF)
        {
            return new Shader(DataV, DataG, DataF);
        }

        private bool UpdateSource()
        {
            var updated = false;
            var sources = new[] { _vertexShader, _geometryShader, _fragmentShader };
            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                var preProcessedSource = sources[i].SourceFinder();
                var newSource = ShaderParser.ProcessSource(preProcessedSource);
                if (newSource != sources[i].Source)
                {
                    Log.WriteLine($"Shader source '{sources[i].Name}' has been updated. ");
                    updated = true;
                }
            }

            if (updated)
                for (var i = 0; i < sources.Length; i++)
                {
                    if (sources[i] == null) continue;
                    sources[i].Source = sources[i].SourceFinder();
                }

            return updated;
        }

        public void Reload()
        {
            if (ShaderId == 0) throw new ArgumentException("Cannot rebuild a non existent shader");
            if (UpdateSource())
            {
                _mappings.Clear();
                _arrayMappings.Clear();
                _knownMappings.Clear();
                _outputs.Clear();
                _inputs.Clear();
                _mappingTypes.Clear();
                CompileShaders(_vertexShader, _geometryShader, _fragmentShader);
                ShaderReloaded?.Invoke();
            }
        }

        private void AddArrayMappings(UniformArray[] Array)
        {
            for (var i = 0; i < Array.Length; i++) _arrayMappings.Add(Array[i].Key, Array[i]);
        }

        private void AddShaderInputs(ShaderInput[] Array, ShaderType Type)
        {
            if (Type != ShaderType.VertexShader && Array.Length > 0)
                throw new ArgumentException(
                    $"Expected '0' inputs for shader '{Name}' of type '{Type}' but got '{Array.Length}'");
            _inputs.AddRange(Array);
        }

        private void AddShaderOutputs(ShaderOutput[] Array, ShaderType Type)
        {
            if (Type != ShaderType.FragmentShader && Array.Length > 0)
                throw new ArgumentException(
                    $"Expected '0' outputs for shader '{Name}' of type '{Type}' but got '{Array.Length}'");
            _outputs.AddRange(Array);
        }

        private static void Compile(out int ID, string Source, string Name, ShaderType Type)
        {
            var asciiSource = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Source));
            ID = Renderer.CreateShader(Type);
            Renderer.ShaderSource(ID, asciiSource);
            Renderer.CompileShader(ID);
            Renderer.GetShader(ID, ShaderParameter.InfoLogLength, out var logLength);
            var log = string.Empty;
            if (logLength > 0) Renderer.GetShaderInfoLog(ID);
            Renderer.GetShader(ID, ShaderParameter.CompileStatus, out var result);
            if (result != 1)
                Log.WriteResult(result == 1, "Shader " + Name + " has compiled" + " | " + log + " | " + result);
        }

        private void CompileShaders(ShaderData DataV, ShaderData DataG, ShaderData DataF)
        {
            if (DataV != null) DataV.Source = ShaderParser.ProcessSource(DataV.Source);
            if (DataG != null) DataG.Source = ShaderParser.ProcessSource(DataG.Source);
            if (DataF != null) DataF.Source = ShaderParser.ProcessSource(DataF.Source);

            if (ShaderId != 0) Renderer.DeleteProgram(ShaderId);
            var shadervid = -1;
            var shaderfid = -1;
            var shadergid = -1;
            if (DataV != null) Compile(out shadervid, DataV.Source, DataV.Name, ShaderType.VertexShader);
            if (DataF != null) Compile(out shaderfid, DataF.Source, DataF.Name, ShaderType.FragmentShader);
            if (DataG != null) Compile(out shadergid, DataG.Source, DataG.Name, ShaderType.GeometryShader);
            ShaderVid = shadervid;
            ShaderFid = shaderfid;
            ShaderGid = shadergid;

            Combine();
        }

        private void Combine()
        {
            ShaderId = Renderer.CreateProgram();
            Log.WriteLine($"Shader'{Name}' compiled succesfully with Id '{ShaderId}'.", LogType.System);

            if (ShaderVid > 0) Renderer.AttachShader(ShaderId, ShaderVid);
            if (ShaderGid > 0) Renderer.AttachShader(ShaderId, ShaderGid);
            if (ShaderFid > 0) Renderer.AttachShader(ShaderId, ShaderFid);

            LinkProgram();

            if (ShaderVid > 0) Renderer.DetachShader(ShaderId, ShaderVid);
            if (ShaderGid > 0) Renderer.DetachShader(ShaderId, ShaderGid);
            if (ShaderFid > 0) Renderer.DetachShader(ShaderId, ShaderFid);

            if (ShaderVid > 0) Renderer.DeleteShader(ShaderVid);
            if (ShaderGid > 0) Renderer.DeleteShader(ShaderGid);
            if (ShaderFid > 0) Renderer.DeleteShader(ShaderFid);

            var parser = new ShaderParser(_vertexShader.Source);
            parser.AddUniformTypes(_mappingTypes);
            AddArrayMappings(parser.ParseUniformArrays(ShaderId));
            AddShaderInputs(parser.ParseShaderInputs(), ShaderType.VertexShader);
            AddShaderOutputs(parser.ParseShaderOutputs(), ShaderType.VertexShader);

            parser.Source = _fragmentShader.Source;
            parser.AddUniformTypes(_mappingTypes);
            AddArrayMappings(parser.ParseUniformArrays(ShaderId));
            AddShaderInputs(parser.ParseShaderInputs(), ShaderType.FragmentShader);
            AddShaderOutputs(parser.ParseShaderOutputs(), ShaderType.FragmentShader);

            if (_geometryShader != null)
            {
                parser.Source = _geometryShader.Source;
                parser.AddUniformTypes(_mappingTypes);
                AddArrayMappings(parser.ParseUniformArrays(ShaderId));
                AddShaderInputs(parser.ParseShaderInputs(), ShaderType.GeometryShader);
                AddShaderOutputs(parser.ParseShaderOutputs(), ShaderType.GeometryShader);
            }

            ShaderManager.RegisterShader(this);
        }

        private void LinkProgram()
        {
            Renderer.LinkProgram(ShaderId);
            Renderer.GetProgram(ShaderId, GetProgramParameterName.LinkStatus, out var isLinked);
            Renderer.GetProgram(ShaderId, GetProgramParameterName.InfoLogLength, out var length);
            var log = string.Empty;
            if (length > 0) Renderer.GetProgramInfoLog(ShaderId, out log);
            if (isLinked == 0)
            {
                Log.WriteResult(false, $"Shader '{Name}' linking failed |  {log}  | ");
            }
            else
            {
                if (log != string.Empty)
                    Log.WriteWarning($"Shader '{Name}' warnings:{Environment.NewLine}{log}");
                Log.WriteLine($"Shader '{Name}' linked successfully", LogType.System);
            }
        }

        public bool HasUniform(string Key)
        {
            if (!_knownMappings.ContainsKey(Key))
                _knownMappings.Add(Key, Renderer.GetUniformLocation(ShaderId, Key) != -1);
            return _knownMappings[Key];
        }

        public static void LoadMapping(UniformMapping Mapping)
        {
            switch (Mapping.Type)
            {
                case MappingType.Integer:
                    if (Mapping.Value is int)
                        Renderer.Uniform1(Mapping.Location, (int)Mapping.Value);
                    else
                        Renderer.Uniform1(Mapping.Location, (float)Mapping.Value);
                    break;
                case MappingType.Double:
                    Renderer.Uniform1(Mapping.Location, (double)Mapping.Value);
                    break;
                case MappingType.Float:
                    Renderer.Uniform1(Mapping.Location, (float)Mapping.Value);
                    break;
                case MappingType.Vector2:
                    Renderer.Uniform2(Mapping.Location, (Vector2)Mapping.Value);
                    break;
                case MappingType.Vector3:
                    Renderer.Uniform3(Mapping.Location, (Vector3)Mapping.Value);
                    break;
                case MappingType.Vector4:
                    Renderer.Uniform4(Mapping.Location, (Vector4)Mapping.Value);
                    break;
                case MappingType.Matrix4:
                    var matrix4 = (Matrix4x4)Mapping.Value;
                    Renderer.UniformMatrix4x4(Mapping.Location, false, ref matrix4);
                    break;
                case MappingType.Matrix3:
                    var matrix3X3 = (Matrix4x4)Mapping.Value;
                    Renderer.UniformMatrix3(Mapping.Location, false, ref matrix3X3);
                    break;
                case MappingType.Matrix2:
                    var matrix2 = (Matrix4x4)Mapping.Value;
                    Renderer.UniformMatrix2(Mapping.Location, false, ref matrix2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unkown type {Mapping.Type}");
            }
        }

        private void LoadBuiltinUniforms()
        {
            if (HasUniform(ShaderManager.ModelViewMatrixName))
                this[ShaderManager.ModelViewMatrixName] = Renderer.ModelViewMatrix;

            if (HasUniform(ShaderManager.ModelViewProjectionName))
                this[ShaderManager.ModelViewProjectionName] = Renderer.ModelViewProjectionMatrix;
        }

        public void Bind()
        {
            if (ShaderId < 0) throw new AccessViolationException($"{GetType().Name} is corrupt. {ShaderId}");

            Renderer.BindShader((uint)ShaderId);
            LoadBuiltinUniforms();
        }

        public void Unbind()
        {
            Renderer.BindShader(0);
            ShaderChanged?.Invoke();
        }
    }
}