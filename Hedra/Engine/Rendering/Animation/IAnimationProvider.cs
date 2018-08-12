namespace Hedra.Engine.Rendering.Animation
{
    public interface IAnimationProvider
    {
        void EmptyCache();
        Animation LoadAnimation(string ColladaFile);
    }
}