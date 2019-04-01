using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class VertexAttributeHandler : StateHandler<uint>
    {
        protected override void DoEnable(uint Index)
        {
            Renderer.Provider.EnableVertexAttribArray(Index);
        }

        protected override void DoDisable(uint Index)
        {
            Renderer.Provider.DisableVertexAttribArray(Index);
        }
    }
}
