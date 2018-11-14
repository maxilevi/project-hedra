using System;
using System.Collections.Generic;

namespace Hedra.Engine.Sound
{
    public class SoundFamily
    {
        private readonly Random _rng;
        private readonly List<SoundBuffer> _buffers;
        
        public SoundFamily()
        {
            _buffers = new List<SoundBuffer>();
            _rng = new Random();
        }
        
        public void Add(SoundBuffer Buffer)
        {
            _buffers.Add(Buffer);
        }

        public SoundBuffer Get()
        {
            return _buffers[_rng.Next(0, _buffers.Count)];
        }
    }
}