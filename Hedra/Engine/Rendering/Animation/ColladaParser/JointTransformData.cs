/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of JointTransformData.
    /// </summary>
    public class JointTransformData
    {
        public readonly Matrix4x4 JointLocalTransform;
        public readonly JointName JointNameId;

        public JointTransformData(JointName JointNameId, Matrix4x4 JointLocalTransform)
        {
            this.JointNameId = JointNameId;
            this.JointLocalTransform = JointLocalTransform;
        }
    }
}