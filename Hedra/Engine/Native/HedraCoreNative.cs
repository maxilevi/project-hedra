using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Native
{
    public class HedraCoreNative
    {
        private const string HedraCoreDLL = "hedracore.dll";
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_simplify(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, uint[] Blacklisted, UIntPtr BlacklistedIndexCount);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_simplifySloppy(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_generateVertexRemap(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_remapIndexBuffer(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr Remap);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_remapVertexBuffer(IntPtr Destination, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize, IntPtr Remap);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_optimizeVertexCache(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, UIntPtr VertexCount);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void meshopt_optimizeOverdraw(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr VertexPositions, UIntPtr VertexCount, UIntPtr Stride, float Threshold);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint meshopt_optimizeVertexFetch(IntPtr Destination, IntPtr Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexSize);

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
        public static extern void fastnoise_setFrequency(IntPtr Object, float Frequency);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setCellularReturnType(IntPtr Object, CellularReturnType returnType);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setSeed(IntPtr Object, int Seed);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setFractalGain(IntPtr pointer, float gain);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setFractalLacunarity(IntPtr pointer, float lacunarity);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setFractalOctaves(IntPtr pointer, int octaves);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setFractalType(IntPtr pointer, FractalType fractalType);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setPerturbAmp(IntPtr pointer, float amplitude);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setPerturbFrequency(IntPtr pointer, float frequency);

        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void fastnoise_setPerturbType(IntPtr pointer, PerturbType perturbType);

        /*
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_initialize();
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_destroy(IntPtr Object);
        
        [DllImport(HedraCoreDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr meshing_getGridCell();*/
    }
    
    public enum CellularReturnType { CellValue, Distance, Distance2, Distance2Add, Distance2Sub, Distance2Mul, Distance2Div, NoiseLookup, Distance2Cave };
    public enum FractalType { FBM, Billow, RigidMulti };
    public enum PerturbType { None, Gradient, GradientFractal, Normalise, Gradient_Normalise, GradientFractal_Normalise };
}