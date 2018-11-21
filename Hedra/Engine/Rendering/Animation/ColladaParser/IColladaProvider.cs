namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public interface IColladaProvider
    {
        AnimatedModelData LoadColladaModel(string ColladaFile);
        ModelData LoadModel(string ColladaFile);
        AnimationData LoadColladaAnimation(string ColladaFile);
    }
}