/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    /// Description of KeyFrame.
    /// </summary>
    public class KeyFrame
    {
        public float TimeStamp { get; }
        public Dictionary<string, JointTransform> Pose { get; }
    
        public KeyFrame(float TimeStamp, Dictionary<string, JointTransform> JointKeyFrames)
        {
            this.TimeStamp = TimeStamp;
            this.Pose = JointKeyFrames;
        }
    }
}
