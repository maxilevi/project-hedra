using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Native
{
    public class HedraCoreNative
    {
        private const string HedraCoreDLL = "hedracore.dll";
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_simplify(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, uint[] Blacklisted, UIntPtr BlacklistedIndexCount);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_simplifySloppy(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_generateVertexRemap(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_remapIndexBuffer(uint[] Destination, uint[] Indices, UIntPtr IndexCount, uint[] Remap);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_remapVertexBuffer(IntPtr Destination, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize, uint[] Remap);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_optimizeVertexCache(uint[] Destination, uint[] Indices, UIntPtr IndexCount, UIntPtr VertexCount);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_optimizeOverdraw(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr Stride, float Threshold);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_optimizeVertexFetch(IntPtr Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_createObject(int Seed);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_deleteObject(IntPtr Pointer);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_freeNoise(IntPtr NoiseSet);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getSimplexFractalSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getSimplexSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getCubicFractalSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getCubicSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getPerlinFractalSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getPerlinSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getValueFractalSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getValueSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_getCellularSet(IntPtr Object, float xOffset, float yOffset, float zOffset, int xSize, int ySize, int zSize, float xScale, float yScale, float zScale);

        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_setFrequency(IntPtr Object, float Frequency);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fastnoise_setSeed(IntPtr Object, int Seed);
        /*
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_initialize();
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_destroy(IntPtr Object);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_getGridCell();*/
    }
}