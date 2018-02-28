/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of JointTransformData.
	/// </summary>
	public class JointTransformData
	{
		public readonly string JointNameId;
		public readonly Matrix4 JointLocalTransform;
		
		public JointTransformData(string JointNameId, Matrix4 JointLocalTransform){
			this.JointNameId = JointNameId;
			this.JointLocalTransform = JointLocalTransform;
		}
	}
}
