namespace Hedra.Engine.Rendering.Animation
{
    public static class AnimationLoader
    {
        public static IAnimationProvider Provider { get; set; } = new AnimationProvider();

        public static void EmptyCache()
        {
            Provider.EmptyCache();
        }

        public static Animation LoadAnimation(string ColladaFile)
        {
            return Provider.LoadAnimation(ColladaFile);
        }
    }
}