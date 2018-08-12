namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public interface IColladaProvider
    {
        AnimatedModelData LoadColladaModel(string ColladaFile, int MaxWeights);
        AnimationData LoadColladaAnimation(string ColladaFile);
    }
}