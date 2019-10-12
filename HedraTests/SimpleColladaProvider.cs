using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenToolkit.Mathematics;

namespace HedraTests
{
    public class SimpleColladaProvider : IColladaProvider
    {
        public AnimatedModelData LoadColladaModel(string ColladaFile)
        {
            return new AnimatedModelData(
                new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0]),
                new JointsData(0, new JointData(1, "Root", Matrix4.Identity))
                );
        }

        public ModelData LoadModel(string ColladaFile)
        {
            return new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0],
                new Vector3[0]);
        }

        public AnimationData LoadColladaAnimation(string ColladaFile)
        {
            return new AnimationData(0, new KeyFrameData[0]);
        }
    }
}