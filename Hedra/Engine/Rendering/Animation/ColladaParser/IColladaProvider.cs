namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public interface IColladaProvider
    {
        AnimatedModelData LoadColladaModel(string ColladaFile, bool LoadAllJoints = false);
        ModelData LoadModel(string ColladaFile);
        AnimationData LoadColladaAnimation(string ColladaFile);
    }
}