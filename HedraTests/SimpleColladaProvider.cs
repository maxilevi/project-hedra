using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;

namespace HedraTests
{
    public class SimpleColladaProvider : IColladaProvider
    {
        public AnimatedModelData LoadColladaModel(string ColladaFile, int MaxWeights)
        {
            return new AnimatedModelData(
                new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0], 0),
                new JointsData(0, new JointData(1, "Root", Matrix4.Identity))
                );
        }

        public AnimationData LoadColladaAnimation(string ColladaFile)
        {
            return new AnimationData(0, new KeyFrameData[0]);
        }
    }
}