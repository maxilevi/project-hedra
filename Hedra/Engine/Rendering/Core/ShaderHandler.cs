using System;

namespace Hedra.Engine.Rendering
{
    public class ShaderHandler
    {
        private uint _currentBound;
        public uint Id => _currentBound;
        public int Skipped { get; private set; }

        public void ResetStats()
        {
            Skipped = 0;
        }

        public void Use(uint Id)
        {
            if (_currentBound == Id)
            {
                ++Skipped;
                return;
            }
            Renderer.Provider.UseProgram(Id);
            _currentBound = Id;
        }
    }
}