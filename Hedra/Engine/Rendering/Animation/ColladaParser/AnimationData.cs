/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of AnimationData.
    /// </summary>
    public class AnimationData
    {
        public readonly float LengthSeconds;
        public readonly KeyFrameData[] KeyFrames;
        
        public AnimationData(float LengthSeconds, KeyFrameData[] KeyFrames){
            this.LengthSeconds = LengthSeconds;
            this.KeyFrames = KeyFrames;
        }
    }
}
