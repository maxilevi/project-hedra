/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    ///     Description of KeyFrame.
    /// </summary>
    public class KeyFrame
    {
        public KeyFrame(float TimeStamp, Dictionary<JointName, JointTransform> JointKeyFrames)
        {
            this.TimeStamp = TimeStamp;
            Pose = JointKeyFrames;
        }

        public float TimeStamp { get; }
        public Dictionary<JointName, JointTransform> Pose { get; }
    }
}