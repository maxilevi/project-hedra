namespace Hedra.Engine.Rendering.Effects
{
    public interface FXShader
    {
        int ScaleUniform { get; set; }
        int PositionUniform { get; set; }
        int BackGroundUniform { get; set; }
        int GUIUniform { get; set; }
        int FlippedUniform { get; set; }
        int SizeUniform { get; set; }

        void Bind();
        void UnBind();
    }
}