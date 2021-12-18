/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of AnimatedModelData.
    /// </summary>
    public class AnimatedModelData
    {
        public AnimatedModelData(ModelData Mesh, JointsData Joints, Vector3 Scale, Vector3 Rotation, Vector3 Translation)
        {
            this.Joints = Joints;
            this.Mesh = Mesh;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Translation = Translation;
        }

        public JointsData Joints { get; }
        public ModelData Mesh { get; }
        
        public Vector3 Scale { get; }
        
        public Vector3 Rotation { get; }
        
        public Vector3 Translation { get; }
    }
}