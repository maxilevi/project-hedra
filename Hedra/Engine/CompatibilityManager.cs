using System;
using System.IO;
using System.Text;
using Hedra.Engine.IO;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Core;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;


namespace Hedra.Engine
{
    /// <summary>
    /// For some reason, certain OpenGL functions explode on specific video cards.
    /// This is a wrapper to avoid this issue and provide workarounds to that methods
    /// </summary>
    public static  class CompatibilityManager
    {
        public static Action<PrimitiveType, uint[], DrawElementsType, IntPtr[], int> MultiDrawElementsMethod { get; private set; }
        public static bool SupportsGeometryShaders { get; private set; } = true;
        public static bool SupportsMeshOptimizer { get; private set; } = true;

        public static void Load()
        {
            DetectGeometryShaderSupport();
            DefineMultiDrawElementsMethod();
            DetectMeshOptimizerSupport();
        }

        private static void DetectMeshOptimizerSupport()
        {
            SupportsMeshOptimizer = true;
            try
            {
                MeshOptimizer.OptimizeCache(new uint[0], 0);
            }
            catch (DllNotFoundException e)
            {
                Log.WriteLine($"Failed to load MeshOptimizer with the following error: {e}");
                SupportsMeshOptimizer = false;
            }

            if (!SupportsMeshOptimizer)
            {
                Log.WriteLine("Failed to load required libraries (core.dll). Please contact me with the game log.");
                Environment.Exit(1);
            }
        }
        
        private static void DetectGeometryShaderSupport()
        {
            SupportsGeometryShaders = true;
            var previousSeverity = Renderer.Severity;
            try
            {
                Renderer.Severity = ErrorSeverity.High;
                var shader = AnimatedModelShader.GenerateDeathShader();
                shader.Bind();
                shader["viewMatrix"] = Matrix4.Identity;
                shader.Unbind();
            }
            catch (Exception e)
            {
                SupportsGeometryShaders = false;
            }
            finally
            {
                Renderer.Severity = previousSeverity;
            }
            Log.WriteLine($"Geometry shaders are {(SupportsGeometryShaders ? "ENABLED" : "DISABLED")}");
        }

        private static void WriteSpecificationsList(string AppPath)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"OpenGL Specifications: {Environment.NewLine}");
            Action<GetPName> write = E => builder.AppendLine($"{E.ToString()} = {Renderer.GetInteger(E)}");

            write(GetPName.MaxGeometryInputComponents);
            write(GetPName.MaxGeometryOutputVertices);
            write(GetPName.MaxGeometryOutputComponents);
            write(GetPName.MaxGeometryTotalOutputComponents);

            builder.AppendLine($"Extensions = {Renderer.GetString(StringName.Extensions)}");
            File.WriteAllText($"{AppPath}/openRenderer.txt", builder.ToString());
        }


        private static void DefineMultiDrawElementsMethod()
        {
            var previousSeverity = Renderer.Severity;
            var useCompatibilityFunction = false;
            try
            {
                var testVAO = PrepareTestData(out var indices, out var buffer);
                Renderer.Severity = ErrorSeverity.High;
                
                Shader.Passthrough.Bind();
                testVAO.Bind();
                indices.Bind();
                
                Renderer.Provider
                    .MultiDrawElements(PrimitiveType.Triangles, new []{3u}, DrawElementsType.UnsignedInt, new []{IntPtr.Zero}, 0);
                
                testVAO.Unbind();
                Shader.Passthrough.Unbind();
                indices.Unbind();

                testVAO.Dispose();
                buffer.Dispose();
                indices.Dispose();
            }
            catch (Exception e)
            {
                useCompatibilityFunction = true;
            }
            finally
            {
                Renderer.Severity = previousSeverity;
            }
            Log.WriteLine($"Compatibility mode is {(useCompatibilityFunction ? "ON" : "OFF")}...");
            if (useCompatibilityFunction)
            {
                Log.WriteLine("Using compatibility draw...");
                MultiDrawElementsMethod = delegate(PrimitiveType Type, uint[] Counts, DrawElementsType DrawType,
                    IntPtr[] Offsets, int Length)
                {
                    for (var i = 0; i < Length; i++)
                    {
                        Renderer.Provider.DrawElements(PrimitiveType.Triangles, (int)Counts[i], DrawType, Offsets[i]);
                    }
                };
            }
            else
            {
                MultiDrawElementsMethod =
                    delegate(PrimitiveType Type, uint[] Counts, DrawElementsType DrawType, IntPtr[] Offsets, int Length)
                    {
                        Renderer.Provider.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Length);
                    };
            }
        }

        private static VAO<Vector3> PrepareTestData(out VBO<uint> Indices, out VBO<Vector3> Buffer)
        {
            var verts = new []
            {
                new Vector3(0,0,1),
                new Vector3(0,1,0), 
                new Vector3(1,0,0)
            };
            Buffer = new VBO<Vector3>(verts, verts.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            Indices = new VBO<uint>(new uint[] {0, 1, 2}, sizeof(uint) * verts.Length, VertexAttribPointerType.UnsignedInt);
            return new VAO<Vector3>(Buffer);
        }
        /*
        public static int QueryAvailableVideoMemory()
        {
            var previousSeverity = Renderer.Severity;
            try
            {
                Renderer.Severity = ErrorSeverity.Ignore;
                Renderer.GetInteger((GetPName) AtiMeminfo.VboFreeMemoryAti, out var mem);
                Log.WriteLine($"Detected '{mem}KB' as ATI memory");
                Renderer.GetError();

                if (mem != 0) return mem / 1024;
                Log.WriteLine($"Detected '{mem}KB' as NVIDIA memory");
                Renderer.GetInteger((GetPName) NvxGpuMemoryInfo.GpuMemoryInfoCurrentAvailableVidmemNvx, out mem);
                Renderer.GetError();
                return mem / 1024;
            }
            finally
            {
                Renderer.Severity = previousSeverity;
            }
        }*/
    }
}
