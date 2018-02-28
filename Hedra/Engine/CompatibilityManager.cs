using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.IO;
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

        public static void Load()
        {
            CompatibilityManager.DefineMultiDrawElementsMethod();
        }

        /// <summary>
        /// If the current video card is a type of AMD Radeon HD use the workaround.
        /// </summary>
        public static void DefineMultiDrawElementsMethod()
        {
            var videoCardString = GL.GetString(StringName.Renderer);
            var manufacturerString = GL.GetString(StringName.Vendor);
            var useCompatibilityFunction = videoCardString.Contains("AMD Radeon HD") || manufacturerString.Contains("Intel");
            if(useCompatibilityFunction) Log.WriteLine("glMultiDrawElements issue detected. Enabling compatibility mode...");

            if (useCompatibilityFunction)
            {
                MultiDrawElementsMethod = delegate(PrimitiveType Type, int[] Counts, DrawElementsType DrawType,
                    IntPtr[] Offsets, int Length)
                {
                    for (int i = 0; i < Counts.Length; i++)
                    {
                        GL.DrawElements(PrimitiveType.Triangles, Counts[i], DrawType, Offsets[i]);
                    }
                };
            }
            else
            {
                MultiDrawElementsMethod =
                    delegate(PrimitiveType Type, int[] Counts, DrawElementsType DrawType, IntPtr[] Offsets, int Length)
                    {
                        GL.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets,
                            Counts.Length);
                    };
            }
        }
    }
}
