/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of AnimatedModelData.
    /// </summary>
    public class AnimatedModelData
    {
        public JointsData Joints { get; }
        public ModelData Mesh { get; }
        
        public AnimatedModelData(ModelData Mesh, JointsData Joints){
            this.Joints = Joints;
            this.Mesh = Mesh;
        }
    }
}
