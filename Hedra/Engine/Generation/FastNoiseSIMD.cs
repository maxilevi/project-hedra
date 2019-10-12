using System;
using System.Collections.Generic;
using Hedra.Engine.Native;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Generation
{
    public class FastNoiseSIMD : IDisposable
    {
        private readonly IntPtr _native;
        private bool _disposed;
        private int _seed;

        public FastNoiseSIMD(int Seed)
        {
            _native = HedraCoreNative.fastnoise_createObject(Seed);
        }

        public int Seed
        {
            get => _seed;
            set
            {
                if(_seed != value)
                    HedraCoreNative.fastnoise_setSeed(_native, _seed = value);
            }
        }

        public float PerturbAmp
        {
            set => HedraCoreNative.fastnoise_setPerturbAmp(_native, value);
        }
        
        public float PerturbFrequency
        {
            set => HedraCoreNative.fastnoise_setPerturbFrequency(_native, value);
        }
        
        public PerturbType PerturbType
        {
            set => HedraCoreNative.fastnoise_setPerturbType(_native, value);
        }

        public float FractalLacunarity
        {
            set => HedraCoreNative.fastnoise_setFractalLacunarity(_native, value);
        }
        
        public float FractalGain
        {
            set => HedraCoreNative.fastnoise_setFractalGain(_native, value);
        }
        
        public int FractalOctaves
        {
            set => HedraCoreNative.fastnoise_setFractalOctaves(_native, value);
        }
        
        public FractalType FractalType
        {
            set => HedraCoreNative.fastnoise_setFractalType(_native, value);
        }
        
        public float Frequency
        {
            set => HedraCoreNative.fastnoise_setFrequency(_native, value);
        }
        
        public CellularReturnType CellularReturnType
        {
            set => HedraCoreNative.fastnoise_setCellularReturnType(_native, value);
        }

        public float[] GetSimplexSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getSimplexSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }

        public float[] GetPerlinFractalSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getPerlinFractalSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetPerlinSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getPerlinSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetSimplexFractalSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getSimplexFractalSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetCubicFractalSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getCubicFractalSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetCubicSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getCubicSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetValueFractalSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getValueFractalSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetValueSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getValueSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetCellularSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            if (_disposed) return null;
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getCellularSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetCellularSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            if (_disposed) return null;
            return GetCellularSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y),
                new Vector3(Scale.X, 1, Scale.Y), frequency);
        }
        
        public float[] GetPerlinFractalSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            return GetPerlinFractalSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), new Vector3(Scale.X, 1, Scale.Y), frequency);
        }
        
        public float[] GetSimplexFractalSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            return GetSimplexFractalSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), new Vector3(Scale.X, 1, Scale.Y), frequency);
        }

        public float[] GetSimplexSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            return GetSimplexSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), new Vector3(Scale.X, 1, Scale.Y), frequency);
        }
        
        public float[] GetPerlinSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            return GetPerlinSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), new Vector3(Scale.X, 1, Scale.Y), frequency);
        }

        private float[] PointerToSet(IntPtr Address, uint Size)
        {
            var buffer = new float[Size];
            unsafe
            {
                var pointer = (float*) Address.ToPointer();
                var index = 0;
                for (var i = 0; i < Size; i++)
                {
                    buffer[index++] = pointer[i];
                }
            }

            HedraCoreNative.fastnoise_freeNoise(Address);
            return buffer;
        }
        
        public void Dispose()
        {
            _disposed = true;
            HedraCoreNative.fastnoise_deleteObject(_native);
        }

        ~FastNoiseSIMD()
        {
            if(!_disposed)
                Dispose();
        }
    }
}