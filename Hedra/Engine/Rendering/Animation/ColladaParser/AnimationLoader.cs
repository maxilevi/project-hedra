/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using OpenTK;
using System.Xml;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of AnimationLoader.
	/// </summary>
	public class AnimationLoader
	{
		private static readonly Matrix4 Correction = Matrix4.CreateRotationX( -90f * Mathf.Radian);
		private XmlNode AnimationData;
		private XmlNode JointHierarchy;
		
		public AnimationLoader(XmlNode AnimationData, XmlNode JointHierarchy){
			this.AnimationData = AnimationData;
			this.JointHierarchy = JointHierarchy;
		}
		
		public AnimationData ExtractAnimation(){
			string rootNode = FindRootJointName();
			float[] times = GetKeyTimes();
			float duration = times[times.Length-1];
			KeyFrameData[] keyFrames = InitKeyFrames(times);
			List<XmlNode> animationNodes = AnimationData.Children("animation");
			for(int i = 0; i<animationNodes.Count; i++){
				LoadJointTransforms(keyFrames, animationNodes[i], rootNode);
			}
			return new AnimationData(duration, keyFrames);
		}
		
		private float[] GetKeyTimes(){
			XmlNode timeData = AnimationData["animation"]["source"]["float_array"];
			string[] rawTimes = timeData.InnerText.Split(' ');
			float[] times = new float[rawTimes.Length];
			for(int i=0;i<times.Length;i++){
				times[i] = float.Parse(rawTimes[i], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			return times;
		}
		
		private KeyFrameData[] InitKeyFrames(float[] times){
			KeyFrameData[] frames = new KeyFrameData[times.Length];
			for(int i=0;i<frames.Length;i++){
				frames[i] = new KeyFrameData(times[i]);
			}
			return frames;
		}
		
		private void LoadJointTransforms(KeyFrameData[] frames, XmlNode JointData, String rootNodeId){
			string jointNameId = GetJointName(JointData);
			string dataId = GetDataId(JointData);
			XmlNode transformData = JointData.ChildWithAttribute("source", "id", dataId);
			string[] rawData = transformData["float_array"].InnerText.Split(' ');
			this.ProcessTransforms(jointNameId, rawData, frames, jointNameId == rootNodeId);
		}
		
		private string GetDataId(XmlNode jointData){
			XmlNode node = jointData["sampler"].ChildWithAttribute("input", "semantic", "OUTPUT");
			return node.GetAttribute("source").Value.Substring(1);
		}
		
		private string GetJointName(XmlNode jointData){
			XmlNode channelNode = jointData["channel"];
			string data = channelNode.GetAttribute("target").Value;
			return data.Split('/')[0];
		}
		
		private void ProcessTransforms(String jointName, String[] rawData, KeyFrameData[] keyFrames, bool root){
			float[] matrixData = new float[16];
			for(int i=0; i<keyFrames.Length; i++){
				for(int j=0; j<16; j++){
					matrixData[j] = float.Parse(rawData[i*16 + j], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				Matrix4 transform = new Matrix4(matrixData[0], matrixData[1], matrixData[2], matrixData[3],
				                                 matrixData[4], matrixData[5], matrixData[6], matrixData[7],
				                                 matrixData[8], matrixData[9], matrixData[10], matrixData[11],
				                                 matrixData[12], matrixData[13], matrixData[14], matrixData[15]);
				transform.Transpose();
				if(root) transform = transform * Correction;
				
				keyFrames[i].AddJointTransform(new JointTransformData(jointName, transform));
			}
		}
		
		private string FindRootJointName(){
			XmlNode skeleton = JointHierarchy["visual_scene"].ChildWithAttribute("node", "id", "Armature");
			return skeleton["node"].GetAttribute("id");
		}
	}
}
