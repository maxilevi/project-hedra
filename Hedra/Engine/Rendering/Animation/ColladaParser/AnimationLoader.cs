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
using OpenToolkit.Mathematics;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Hedra.Core;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of AnimationLoader.
    /// </summary>
    public class AnimationLoader
    {
        private const string LinearInterpolationKeyword = "LINEAR";
        private const string BezierInterpolationKeyword = "BEZIER";
        private const string ArmatureName = "Armature";
        private static readonly Matrix4 Correction = Matrix4.CreateRotationX( -90f * Mathf.Radian);
        private readonly XmlNode _animationData;
        private readonly XmlNode _jointHierarchy;
        private readonly string[] _jointIds;
        
        public AnimationLoader(XmlNode AnimationData, XmlNode JointHierarchy)
        {
            _animationData = AnimationData;
            _jointHierarchy = JointHierarchy;
            _jointIds = JointsLoader.GetJointIds(GetSkeletonNode());
        }
        
        public AnimationData ExtractAnimation()
        {
            var rootNode = FindRootJointName();
            var times = GetKeyTimes();
            var duration = times[times.Length-1];
            var keyFrames = InitKeyFrames(times);
            var animationNodes = _animationData.Children("animation").ToArray();
            for(var i = 0; i < animationNodes.Length; i++)
            {
                LoadJointTransforms(keyFrames, animationNodes[i], rootNode);
            }
            return new AnimationData(duration, keyFrames);
        }
        
        private float[] GetKeyTimes()
        {
            XmlNode timeData = _animationData["animation"]["source"]["float_array"];
            string[] rawTimes = timeData.InnerText.Split(' ');
            float[] times = new float[rawTimes.Length];
            for(int i=0;i<times.Length;i++){
                times[i] = float.Parse(rawTimes[i], NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            return times;
        }
        
        private static KeyFrameData[] InitKeyFrames(float[] Times)
        {
            var frames = new KeyFrameData[Times.Length];
            for(var i=0; i < frames.Length; i++)
            {
                frames[i] = new KeyFrameData(Times[i]);
            }
            return frames;
        }
        
        private void LoadJointTransforms(KeyFrameData[] frames, XmlNode JointData, String rootNodeId)
        {
            string jointNameId = GetJointName(JointData);
            if(Array.IndexOf(_jointIds, jointNameId) == -1) return;
            
            string dataId = GetDataId(JointData);
            XmlNode transformData = JointData.ChildWithAttribute("source", "id", dataId);
            AssertInterpolationMode(JointData.InnerText);
            string[] rawData = transformData["float_array"].InnerText.Split(' ');
            this.ProcessTransforms(jointNameId, rawData, frames, jointNameId == rootNodeId);
        }
        
        private string GetDataId(XmlNode jointData){
            XmlNode node = jointData["sampler"].ChildWithAttribute("input", "semantic", "OUTPUT");
            return node.GetAttribute("source").Value.Substring(1);
        }
        
        private string GetJointName(XmlNode jointData)
        {
            XmlNode channelNode = jointData["channel"];
            string data = channelNode.GetAttribute("target").Value;
            return data.Split('/')[0].Replace($"{ArmatureName}_", string.Empty);
        }
        
        private void ProcessTransforms(String jointName, String[] rawData, KeyFrameData[] keyFrames, bool root)
        {
            float[] matrixData = new float[16];
            for(int i=0; i<keyFrames.Length; i++)
            {
                for(var j=0; j<16; j++)
                {
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
        
        private string FindRootJointName()
        {
            return GetSkeletonNode()["node"].GetAttribute("id");
        }

        private XmlNode GetSkeletonNode()
        {
            return _jointHierarchy["visual_scene"].ChildWithAttribute("node", "id", ArmatureName);
        }

        private static void AssertInterpolationMode(string Text)
        {
            if(Text.Contains(BezierInterpolationKeyword))
                throw new ArgumentException($"Animation is '{BezierInterpolationKeyword}' interpolated but should be '{LinearInterpolationKeyword}'");
        }
    }
}
