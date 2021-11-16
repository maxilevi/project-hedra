/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of JointsData.
    /// </summary>
    public class JointsData
    {
        public readonly JointData HeadJoint;
        public readonly int JointCount;

        public JointsData(int JointCount, JointData HeadJoint)
        {
            this.JointCount = JointCount;
            this.HeadJoint = HeadJoint;
        }
    }
}