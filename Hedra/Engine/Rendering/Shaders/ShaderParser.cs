using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Microsoft.Scripting;

namespace Hedra.Engine.Rendering.Shaders
{
    /// <summary>
    ///     Partial shader parser for detecting certain uniforms
    /// </summary>
    public class ShaderParser
    {
        public ShaderParser(string Source)
        {
            this.Source = Source;
        }

        public string Source { get; set; }

        public static string ProcessSource(string Source)
        {
            if (Source == null) return null;
            var matches = Regex.Matches(Source, @"(!\s*include\s*<\s*""([a-zA-Z\/\.]+)""\s*>)").ToArray();
            for (var i = 0; i < matches.Length; i++)
            {
                var statement = matches[i].Groups[1].Value;
                var includeFile = matches[i].Groups[2].Value;
                Source = Source.Replace(statement, AssetManager.ReadShader(includeFile));
            }

            return AddBuiltinUniforms(Source);
        }

        public string GetVersionString()
        {
            var firstMatch = Regex.Match(Source, @"\s*#\s*version\s+([0-9]+\s*[a-zA-Z]+)\s+");
            return firstMatch.Value;
        }

        public int GetVersionNumber()
        {
            var firstMatch = Regex.Match(Source, @"\s*#\s*version\s+(\d+)\s+");
            return int.Parse(firstMatch.Value);
        }

        public UniformArray[] ParseUniformArrays(int ShaderId)
        {
            var mappings = new List<UniformArray>();
            var matches = Regex
                .Matches(Source, @"\buniform\s+([a-zA-Z0-9]+)\s+([a-zA-Z0-9]+)\s*\[\s*(\d+|[a-zA-Z_]+)\s*\]").ToArray();
            for (var i = 0; i < matches.Length; i++)
            {
                if ((matches[i].Groups.Count - 1) % 3 != 0)
                    throw new ArgumentException($"Expected remainder 0 got {(matches[i].Groups.Count - 1) % 3}");
                var type = ParseType(matches[i].Groups[1].Value);
                var mapping = ParseMappingType(matches[i].Groups[1].Value);
                var key = matches[i].Groups[2].Value;
                var size = ParseArraySize(matches[i].Groups[3].Value);
                mappings.Add(new UniformArray(type, ShaderId, key, size, mapping));
            }

            return mappings.ToArray();
        }

        public void AddUniformTypes(Dictionary<string, MappingType> Map)
        {
            var matches = Regex.Matches(Source, @"\buniform\s+([a-zA-Z0-9]+)\s+([a-zA-Z0-9_]+)").ToArray();
            for (var i = 0; i < matches.Length; i++)
            {
                if ((matches[i].Groups.Count - 1) % 2 != 0)
                    throw new ArgumentException($"Expected remainder 0 got {(matches[i].Groups.Count - 1) % 3}");
                var type = ParseMappingType(matches[i].Groups[1].Value);
                var key = matches[i].Groups[2].Value;
                if (Map.ContainsKey(key) && Map[key] != type)
                    throw new ArgumentTypeException(
                        $"Different types '{Map[key]}' and '{type}' for the same key '{key}'");
                Map[key] = type;
            }
        }

        public ShaderInput[] ParseShaderInputs()
        {
            return ParseShaderIO<ShaderInput>(@"layout\s*\(\s*location\s*=\s*(\d)\)\s*in\s+(.*?)\s+(.*?);");
        }

        public ShaderOutput[] ParseShaderOutputs()
        {
            return ParseShaderIO<ShaderOutput>(@"layout\s*\(\s*location\s*=\s*(\d)\)\s*out\s+(.*?)\s+(.*?);");
        }

        private T[] ParseShaderIO<T>(string RegexString) where T : ShaderIO
        {
            var mappings = new List<T>();
            var matches = Regex.Matches(Source, RegexString).ToArray();
            for (var i = 0; i < matches.Length; i++)
            {
                var location = uint.Parse(matches[i].Groups[1].Value);
                var type = ParseType(matches[i].Groups[2].Value);
                var name = matches[i].Groups[3].Value;
                mappings.Add((T)Activator.CreateInstance(typeof(T), location, name, type));
            }

            return mappings.ToArray();
        }

        private int ParseArraySize(string Value)
        {
            var tryInt = int.TryParse(Value, out var newValue);
            if (!tryInt) newValue = int.Parse(GetValueFromConstant(Value));
            return newValue;
        }

        private string GetValueFromConstant(string VariableName)
        {
            var match = Regex.Match(Source, @"const\s+[a-zA-Z0-9]+\s+" + VariableName + @"\s*=\s*([a-zA-Z0-9]+)\s*;");
            return match.Groups[1].Value;
        }

        private static Type ParseType(string Type)
        {
            switch (Type)
            {
                case "mat4":
                    return typeof(Matrix4x4);
                case "mat3":
                    return typeof(Matrix4x4);
                case "mat2":
                    return typeof(Matrix4x4);
                case "vec4":
                    return typeof(Vector4);
                case "vec3":
                    return typeof(Vector3);
                case "vec2":
                    return typeof(Vector2);
                case "float":
                    return typeof(float);
                case "bool":
                    return typeof(bool);
                case "int":
                    return typeof(int);
                default:
                    // FIXME: This does not work when obfuscated
                    var possibleType = InferType(Type);
                    if (possibleType != null) return possibleType;
                    throw new ArgumentException($"Type '{Type}' could not be mapped to a valid type");
            }
        }


        private MappingType ParseMappingType(string Type)
        {
            return Type == "mat4" ? MappingType.Matrix4
                : Type == "mat3" ? MappingType.Matrix3
                : Type == "mat2" ? MappingType.Matrix2
                : Type == "vec4" ? MappingType.Vector4
                : Type == "vec3" ? MappingType.Vector3
                : Type == "vec2" ? MappingType.Vector2
                : Type == "bvec2" ? MappingType.Vector2
                : Type == "float" ? MappingType.Float
                : Type == "double" ? MappingType.Double
                : Type == "int" ? MappingType.Integer
                : Type == "bool" ? MappingType.Integer
                : Type == "sampler2D" ? MappingType.Integer
                : Type == "sampler3D" ? MappingType.Integer
                : Type == "sampler2DShadow" ? MappingType.Integer
                : Type == "samplerCube" ? MappingType.Integer
                : throw new ArgumentOutOfRangeException($"Unknown mapping '{Type}'");
        }

        private static Type InferType(string ClassName)
        {
            return Assembly.GetExecutingAssembly().GetLoadableTypes().FirstOrDefault(T => T.Name == ClassName);
        }

        private static string AddBuiltinUniforms(string Source)
        {
            var lines = Source.Split("\n".ToCharArray()).ToList();
            var offset = lines.FindIndex(S => S.Contains("#version"));
            lines.Insert(offset + 1, $"uniform mat4 {ShaderManager.ModelViewMatrixName};");
            lines.Insert(offset + 2, $"uniform mat4 {ShaderManager.ModelViewProjectionName};");
            return string.Join("\n", lines);
        }
    }
}