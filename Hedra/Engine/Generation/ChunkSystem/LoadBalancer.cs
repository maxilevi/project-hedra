using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class LoadBalancer
    {

        public int Capacity { get; }

        public LoadBalancer(int Capacity)
        {
            this.Capacity = Capacity;

        }

    }
}