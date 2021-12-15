using System.Collections.Generic;
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
            return new Animation(1, new KeyFrame[2]
                {
                    new KeyFrame(0, new Dictionary<JointName, JointTransform>()),
                    new KeyFrame(1, new Dictionary<JointName, JointTransform>())
                }
            );
        }
    }
}