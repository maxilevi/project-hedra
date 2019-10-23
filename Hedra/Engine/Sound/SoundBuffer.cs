/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 01:26 a.m.
 *
 */
using System;
using Silk.NET.OpenAL;

namespace Hedra.Engine.Sound
{
    /// <summary>
    /// Description of SoundBuffer.
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        public readonly uint Id;
        private readonly AL _al;
        
        public unsafe SoundBuffer(short[] Data, BufferFormat Format, int SampleRate)
        {
            _al = AL.GetApi();
            var id = 0u;
            _al.GenBuffers(1, &id);
            fixed (void* ptr = Data)
            {
                _al.BufferData(id, Format, ptr, Data.Length * sizeof(short), SampleRate);
            }

            Id = id;
        }
        
        public unsafe void Dispose()
        {
            var id = Id;
            _al.DeleteBuffers(1, &id);
        }
    }
}
