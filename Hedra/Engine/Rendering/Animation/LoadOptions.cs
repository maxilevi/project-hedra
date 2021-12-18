namespace Hedra.Engine.Rendering.Animation;

public class LoadOptions
{
    public bool LoadAllJoints { get; set; }
    public bool FlipNormals { get; set; }

    public static LoadOptions Default => new LoadOptions
    {

    };
}