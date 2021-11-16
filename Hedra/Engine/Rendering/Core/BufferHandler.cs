using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public class BufferHandler
    {
        public uint Id { get; private set; }

        public BufferTarget Target { get; private set; }

        public void Bind(BufferTarget ObjectTarget, uint ObjectId)
        {
            Renderer.Provider.BindBuffer(ObjectTarget, ObjectId);
            Id = ObjectId;
            Target = ObjectTarget;
        }
    }
}