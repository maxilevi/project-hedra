/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.Xml;
using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of JointsLoader.
    /// </summary>
    public class JointsLoader
    {
        private const string ArmatureName = "Armature";
        private XmlNode ArmatureData;
        private List<string> BoneOrder;
        private int JointCount = 0;
        private static readonly Matrix4 Correction = Matrix4.CreateFromAxisAngle(Vector3.UnitX, -90f * Mathf.Radian);
    
        public JointsLoader(XmlNode VisualSceneNode, List<string> BoneOrder) {
            this.ArmatureData = VisualSceneNode["visual_scene"].ChildWithAttribute("node", "id", ArmatureName);
            this.BoneOrder = BoneOrder;
        }
        
        public JointsData ExtractBoneData(){
            XmlNode headNode = ArmatureData["node"];
            JointData headJoint = LoadJointData(headNode, true);
            if(BoneOrder.Count > JointCount) throw new ArgumentOutOfRangeException($"Some vertex groups have no attached joint ({BoneOrder.Count}). Probably check that the exported model has no duplicated vertex groups.");
            return new JointsData(JointCount, headJoint);
        }
        
        private JointData LoadJointData(XmlNode JointNode, bool IsRoot){
            JointData joint = ExtractMainJointData(JointNode, IsRoot);
            List<XmlNode> Childs = JointNode.Children("node");
            for(int i = 0; i < Childs.Count; i++){
                joint.AddChild(LoadJointData(Childs[i], false));
            }
            return joint;
        }
        
        private JointData ExtractMainJointData(XmlNode JointNode, bool IsRoot){
            string jointId = JointNode.GetAttribute("id").Value.Replace($"{ArmatureName}_", string.Empty);
            string jointSid = JointNode.GetAttribute("sid").Value;
            if(jointId != jointSid) throw new ArgumentException($"Joint ID '{jointId}' differs from Joint NAME '{jointSid}'.");
            int index = BoneOrder.IndexOf(jointId);
            if(IsRoot && index == -1) throw new ArgumentException($"Root bone cannot have an index of -1");
            string[] matrixData = JointNode["matrix"].InnerText.Split(' ');
            Matrix4 matrix = Mat4FromString(matrixData);
            matrix.Transpose();
            if(IsRoot)matrix = matrix * Correction;
            JointCount++;
            return new JointData(index, jointId, matrix);
        }
        
        private Matrix4 Mat4FromString(String[] rawData){
            if(rawData.Length != 16) throw new ArgumentException("Not enough values.");
            return new Matrix4( float.Parse(rawData[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                                float.Parse(rawData[4], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[5], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[6], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[7], NumberStyles.Any, CultureInfo.InvariantCulture),
                                float.Parse(rawData[8], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[9], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[10], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[11], NumberStyles.Any, CultureInfo.InvariantCulture),
                                float.Parse(rawData[12], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[13], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[14], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(rawData[15], NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
        }
    }
}
