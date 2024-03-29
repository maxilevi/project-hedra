/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of ColladaLoader.
    /// </summary>
    public static class ColladaLoader
    {
        public static IColladaProvider Provider { get; set; } = new ColladaProvider();

        public static AnimatedModelData LoadColladaModel(string ColladaFile, LoadOptions Options)
        {
            return Provider.LoadColladaModel(ColladaFile, Options);
        }

        public static ModelData LoadModel(string ColladaFile)
        {
            return Provider.LoadModel(ColladaFile);
        }

        public static AnimationData LoadColladaAnimation(string ColladaFile)
        {
            return Provider.LoadColladaAnimation(ColladaFile);
        }
    }
}