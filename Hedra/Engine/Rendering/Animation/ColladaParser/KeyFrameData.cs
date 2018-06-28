/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of KeyFrameData.
	/// </summary>
	internal class KeyFrameData
	{
		public readonly float Time;
		public readonly List<JointTransformData> JointTransforms = new List<JointTransformData>();
		
		public KeyFrameData(float Time){
			this.Time = Time;
		}
		
		public void AddJointTransform(JointTransformData Transform){
			JointTransforms.Add(Transform);
		}
	}
}
