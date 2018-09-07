using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine
{
    /// <summary>
    /// For some reason, certain OpenGL functions explode on specific video cards.
    /// This is a wrapper to avoid this issue and provide workarounds to that methods
    /// </summary>
    public static  class CompatibilityManager
    {
        public static Action<PrimitiveType, int[], DrawElementsType, IntPtr[], int> MultiDrawElementsMethod { get; private set; }
        public static bool SupportsGeometryShaders { get; private set; } = true;

        public static void Load(string AppPath)
        {
            //CompatibilityManager.WriteSpecificationsList(AppPath);
            CompatibilityManager.DetectGeometryShaderSupport();
            CompatibilityManager.DefineMultiDrawElementsMethod();
        }

        private static void DetectGeometryShaderSupport()
        {
            SupportsGeometryShaders = true;
            try
            {
                var shader = AnimatedModelShader.GenerateDeathShader();
                shader.Bind();
                shader["viewMatrix"] = Matrix4.Identity;
                shader.Unbind();
            }
            catch (ArgumentException e)
            {
                SupportsGeometryShaders = false;
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

        /// <summary>
        /// If the current video card is a type of AMD Radeon HD use the workaround.
        /// </summary>
        private static void DefineMultiDrawElementsMethod()
        {
            var videoCardString = Renderer.GetString(StringName.Renderer);
            var manufacturerString = Renderer.GetString(StringName.Vendor);
            var useCompatibilityFunction = true;//videoCardString.Contains("AMD") || manufacturerString.Contains("Intel");
            if(useCompatibilityFunction) Log.WriteLine("glMultiDrawElements issue detected. Enabling compatibility mode...");

            if (useCompatibilityFunction)
            {
                MultiDrawElementsMethod = delegate(PrimitiveType Type, int[] Counts, DrawElementsType DrawType,
                    IntPtr[] Offsets, int Length)
                {
                    for (var i = 0; i < Counts.Length; i++)
                    {
                        Renderer.Provider.DrawElements(PrimitiveType.Triangles, Counts[i], DrawType, Offsets[i]);
                    }
                };
            }
            else
            {
                MultiDrawElementsMethod =
                    delegate(PrimitiveType Type, int[] Counts, DrawElementsType DrawType, IntPtr[] Offsets, int Length)
                    {
                        Renderer.Provider.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets,
                            Counts.Length);
                    };
            }
        }
    }
}
