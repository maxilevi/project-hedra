using System;
using System.Collections.Generic;
using Hedra.Engine.Native;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class FastNoiseSIMD : IDisposable
    {
        private readonly HashSet<float[]> _sets;
        private readonly IntPtr _native;
        private bool _disposed;

        public FastNoiseSIMD(int Seed)
        {
            _sets = new HashSet<float[]>();
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

        public float[] GetSimplexFractalSetWithFrequency(Vector3 Offset, Vector3 Size, float frequency, float scaleModifier)
        {
            //Frequency = frequency;
            var set = HedraCoreNative.fastnoise_getSimplexFractalSet(_native, (int)Offset.X, (int)Offset.Y, (int)Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, frequency * scaleModifier);
            AddSet(set);
            return set;
        }
        
        public float[] GetSimplexSetWithFrequency(Vector3 Offset, Vector3 Size, float frequency, float scaleModifier)
        {
            //Frequency = frequency;
            var set = HedraCoreNative.fastnoise_getSimplexSet(_native, (int)Offset.X, (int)Offset.Y, (int)Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z, frequency * scaleModifier);
            AddSet(set);
            return set;
        }

        public float[] GetSimplexSetWithFrequency(Vector2 Offset, Vector2 Size, float frequency, float ScaleModifier)
        {
            return GetSimplexSetWithFrequency(new Vector3(Offset.X, 0, Offset.Y), new Vector3(Size.X, 1, Size.Y), frequency, ScaleModifier);
        }

        private void AddSet(float[] Set)
        {
            _sets.Add(Set);
        }

        public void FreeSet(float[] Set)
        {
            _sets.Remove(Set);
            HedraCoreNative.fastnoise_freeNoise(Set);
        }

        public void Dispose()
        {
            if(_sets.Count > 0)
                throw new ArgumentOutOfRangeException("Not all noise sets have been free'd");
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