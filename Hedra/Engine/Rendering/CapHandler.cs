using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    internal class CapHandler : StateHandler<EnableCap>
    {
        protected override void DoEnable(EnableCap Index)
        {
            GL.Enable(Index);
        }

        protected override void DoDisable(EnableCap Index)
        {
            GL.Disable(Index);
        }
    }
}
