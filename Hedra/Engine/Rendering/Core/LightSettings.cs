using System.Numerics;
using System.Runtime.InteropServices;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct LightSettings
    {
        [FieldOffset(0)] public fixed byte LightsData[1024];
        [FieldOffset(1024)] public int LightCount;
        [FieldOffset(1040)] public Vector3 LightPosition;
        [FieldOffset(1056)] public Vector3 LightColor;

        public LightSettings(AlignedPointLight[] Lights, Vector3 LightColor, Vector3 LightPosition, int LightCount)
        {
            this.LightColor = LightColor;
            this.LightPosition = LightPosition;
            this.LightCount = LightCount;
            var output = new byte[1024];
            const int sizeOfLight = 32;

            void MarshalLight(AlignedPointLight Light, byte[] Output, int Offset)
            {
                var size = Marshal.SizeOf(Light);
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(Light, ptr, false);
                Marshal.Copy(ptr, Output, Offset, size);
                Marshal.FreeHGlobal(ptr);
            }

            for (var i = 0; i < Lights.Length; ++i) MarshalLight(Lights[i], output, i * sizeOfLight);

            fixed (byte* p = LightsData)
            {
                for (var i = 0; i < output.Length; ++i) *(p + i) = output[i];
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (LightSettings)obj;
            if (LightCount == other.LightCount && LightColor == other.LightColor &&
                LightPosition == other.LightPosition)
            {
                var hasSameBytes = true;
                for (var i = 0; i < 1024; ++i) hasSameBytes &= other.LightsData[i] == LightsData[i];

                return hasSameBytes;
            }

            return false;
        }
    }
}