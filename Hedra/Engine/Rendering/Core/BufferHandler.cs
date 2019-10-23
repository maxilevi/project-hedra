using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public class BufferHandler
    {
        private uint _currentlyBound;
        private BufferTarget _currentTarget;
        
        public void Bind(BufferTarget ObjectTarget, uint ObjectId)
        {
            Renderer.Provider.BindBuffer(ObjectTarget, ObjectId);
            _currentlyBound = ObjectId;
            _currentTarget = ObjectTarget;
        }

        public uint Id => _currentlyBound;
        public BufferTarget Target => _currentTarget;
    }
}