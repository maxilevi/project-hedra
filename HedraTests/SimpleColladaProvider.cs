using Hedra.Engine.Rendering.Animation.ColladaParser;
using System.Numerics;

namespace HedraTests
{
    public class SimpleColladaProvider : IColladaProvider
    {
        public AnimatedModelData LoadColladaModel(string ColladaFile, bool LoadAllJoints = false)
        {
            return new AnimatedModelData(
                new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0], new string[0]),
                new JointsData(0, new JointData(1, "Root", Matrix4x4.Identity))
                );
        }

        public ModelData LoadModel(string ColladaFile)
        {
            return new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0],
                new Vector3[0], new string[0]);
        }

        public AnimationData LoadColladaAnimation(string ColladaFile)
        {
            return new AnimationData(0, new KeyFrameData[0]);
        }
    }
}