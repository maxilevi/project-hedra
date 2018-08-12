using Hedra.Engine.Rendering.Animation;

namespace HedraTests
{
    public class SimpleAnimationProvider : IAnimationProvider
    {
        public void EmptyCache()
        {
        }

        public Animation LoadAnimation(string ColladaFile)
        {
            return new Animation(0, new KeyFrame[0]);
        }
    }
}