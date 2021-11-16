using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public class FramebufferHandler
    {
        public uint Id { get; private set; }

        public FramebufferTarget Target { get; private set; }

        public void Bind(FramebufferTarget ObjectTarget, uint ObjectId)
        {
            Renderer.Provider.BindFramebuffer(ObjectTarget, ObjectId);
            Id = ObjectId;
            Target = ObjectTarget;
        }
    }
}