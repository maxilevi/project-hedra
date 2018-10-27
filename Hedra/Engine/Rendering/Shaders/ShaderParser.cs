using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Rendering.Shaders
{
    /// <summary>
    /// Partial shader parser for detecting certain uniforms
    /// </summary>
    public class ShaderParser
    {        
        public string Source { get; set; }

        public ShaderParser(string Source)
        {
            this.Source = Source;
        }

        public static string ProcessSource(string Source)
        {
            if (Source == null) return null;
            var matches = Regex.Matches(Source, @"(!\s*include\s*<\s*""([a-zA-Z\/\.]+)""\s*>)").Cast<Match>().ToArray();
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
            var matches = Regex.Matches(Source, @"\buniform\s+([a-zA-Z0-9]+)\s+([a-zA-Z0-9]+)\s*\[\s*(\d+|[a-zA-Z_]+)\s*\]").Cast<Match>().ToArray();
            for (var i = 0; i < matches.Length; i ++)
            {
                if ((matches[i].Groups.Count - 1) % 3 != 0) throw new ArgumentException($"Expected remainder 0 got {(matches[i].Groups.Count - 1) % 3}");
                var type = ParseType(matches[i].Groups[1].Value);
                var key = matches[i].Groups[2].Value;
                var size = this.ParseArraySize(matches[i].Groups[3].Value);
                mappings.Add(new UniformArray(type, ShaderId, key, size));
            }
            return mappings.ToArray();
        }

        private int ParseArraySize(string Value)
        {
            var tryInt = int.TryParse(Value, out int newValue);
            if (!tryInt)
            {
                newValue = int.Parse(this.GetValueFromConstant(Value));
            }
            return newValue;
        }

        private string GetValueFromConstant(string VariableName)
        {
           var match = Regex.Match(Source, @"const\s+[a-zA-Z0-9]+\s+"+VariableName+@"\s*=\s*([a-zA-Z0-9]+)\s*;");
           return match.Groups[1].Value;
        }

        private static Type ParseType(string Type)
        {
            switch (Type)
            {
                case "mat4":
                    return typeof(Matrix4);
                case "mat3":
                    return typeof(Matrix3);
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
                    // FIXME: This doesnt work when obfuscated
                    var possibleType = InferType(Type);
                    if (possibleType != null) return possibleType;
                    throw new ArgumentException($"Type '{Type}' could not be mapped to a valid type");
            }
        }

        private static Type InferType(string ClassName)
        {
            return Assembly.GetExecutingAssembly().GetLoadableTypes().FirstOrDefault(T => T.Name == ClassName);
        }

        private static string AddBuiltinUniforms(string Source)
        {
            var lines = Source.Split(Environment.NewLine.ToCharArray()).ToList();
            var offset = lines.FindIndex(S => S.Contains("#version"));
            lines.Insert(offset + 1, $"uniform mat4 {ShaderManager.ModelViewMatrixName};");
            lines.Insert(offset + 2, $"uniform mat4 {ShaderManager.ModelViewProjectionName};");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
