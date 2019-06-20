using System;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class DynamicLine : IDisposable
    {
        private TrailRenderer _trail;

        public DynamicLine()
        {
            _trail = new TrailRenderer(() => Vector3.Zero, Vector4.One);
        }

        public void Dispose()
        {
            
        }
    }
}