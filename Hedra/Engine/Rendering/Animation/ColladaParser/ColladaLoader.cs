/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of ColladaLoader.
    /// </summary>
    public static class ColladaLoader
    {
        public static IColladaProvider Provider { get; set; } = new ColladaProvider();
        
        public static AnimatedModelData LoadColladaModel(string ColladaFile, int MaxWeights)
        {
            return Provider.LoadColladaModel(ColladaFile, MaxWeights);
        }
    
        public static AnimationData LoadColladaAnimation(string ColladaFile)
        {
            return Provider.LoadColladaAnimation(ColladaFile);
        }
    }
}
