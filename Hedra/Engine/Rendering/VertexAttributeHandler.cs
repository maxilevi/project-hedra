using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class VertexAttributeHandler : StateHandler<uint>
    {
        protected override void DoEnable(uint Index)
        {
            GL.EnableVertexAttribArray(Index);
        }

        protected override void DoDisable(uint Index)
        {
            GL.DisableVertexAttribArray(Index);
        }
    }
}
