using System;
using System.Collections.Generic;
using Hedra.Engine.Native;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class FastNoiseSIMD : IDisposable
    {
        private readonly IntPtr _native;
        private bool _disposed;

        public FastNoiseSIMD(int Seed)
        {
            _native = HedraCoreNative.fastnoise_createObject(Seed);
        }

        public int Seed
        {
            set => HedraCoreNative.fastnoise_setSeed(_native, value);
        }

        public float Frequency
        {
            set => HedraCoreNative.fastnoise_setFrequency(_native, value);
        }

        public float[] GetSimplexFractalSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getSimplexFractalSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }
        
        public float[] GetSimplexSetWithFrequency(Vector3 Offset, Vector3 Size, Vector3 Scale, float frequency)
        {
            Frequency = frequency;
            var pointer = HedraCoreNative.fastnoise_getSimplexSet(_native, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, Scale.X, Scale.Y, Scale.Z);
            return PointerToSet(pointer, (uint)(Size.X * Size.Y * Size.Z));
        }

        public float[] GetSimplexSetWithFrequency(Vector2 Offset, Vector2 Size, Vector2 Scale, float frequency)
        {
            return GetSimplexSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), new Vector3(Scale.X, 1, Scale.Y), frequency);
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