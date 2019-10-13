

using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public class FramebufferHandler
    {
        private uint _currentlyBound;
        private FramebufferTarget _currentTarget;

        public void Bind(FramebufferTarget ObjectTarget, uint ObjectId)
        {
            Renderer.Provider.BindFramebuffer(ObjectTarget, ObjectId);
            _currentlyBound = ObjectId;
            _currentTarget = ObjectTarget;
        }

        public uint Id => _currentlyBound;
        public FramebufferTarget Target => _currentTarget;
    }
}