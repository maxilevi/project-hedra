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
using System.Globalization;
using System.Numerics;
using System.Xml;
using BulletSharp.SoftBody;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of JointsLoader.
    /// </summary>
    public class JointsLoader
    {
        private const string ArmatureName = "Armature";

        private static readonly Matrix4x4
            Correction = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, -90f * Mathf.Radian);

        private readonly XmlNode ArmatureData;
        private readonly List<string> BoneOrder;
        private int JointCount;
        public Vector3 Translation { get; }
        public Vector3 Rotation { get; }
        public Vector3 Scale { get; }

        public JointsLoader(XmlNode VisualSceneNode, List<string> BoneOrder)
        {
            ArmatureData = VisualSceneNode["visual_scene"].ChildWithAttribute("node", "id", ArmatureName);
            this.BoneOrder = BoneOrder ?? CollectBoneOrder();
            Translation = GetTranslation(VisualSceneNode);
            Rotation = GetRotation(VisualSceneNode);
            Scale = GetScale(VisualSceneNode);
        }

        private static Vector3 GetVector3(string[] data)
        {
            return new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
        }

        private static XmlNode GetArmatureNode(XmlNode VisualSceneNode)
        {
            return VisualSceneNode["visual_scene"].ChildWithAttribute("node", "id", ArmatureName);
        }

        public static Vector3 GetScale(XmlNode VisualSceneNode)
        {
            var armature = GetArmatureNode(VisualSceneNode);
            return GetVector3(armature["scale"].InnerText.Split(" "));
        }
        
        public static Vector3 GetTranslation(XmlNode VisualSceneNode)
        {
            var armature = GetArmatureNode(VisualSceneNode);
            return GetVector3(armature["translate"].InnerText.Split(" "));
        }

        public static Vector3 GetRotation(XmlNode VisualSceneNode)
        {
            var armature = GetArmatureNode(VisualSceneNode);
            var x = armature.ChildWithAttribute("rotate", "sid", "rotationX").InnerText.Split(" ");
            var y = armature.ChildWithAttribute("rotate", "sid", "rotationY").InnerText.Split(" ");
            var z = armature.ChildWithAttribute("rotate", "sid", "rotationZ").InnerText.Split(" ");
            return new Vector3(float.Parse(x[3]), float.Parse(y[3]), float.Parse(z[3]));
        }
        
        private List<string> CollectBoneOrder()
        {
            var order = new List<string>();
            var headNode = ArmatureData["node"];
            LoadJointName(headNode, order);
            return order;
        }

        private void LoadJointName(XmlNode JointNode, List<string> Order)
        {
            Order.Add(JointNode.GetAttribute("sid").Value);
            var childs = JointNode.Children("node");
            for (var i = 0; i < childs.Count; i++) LoadJointName(childs[i], Order);
        }

        public JointsData ExtractBoneData()
        {
            var headNode = ArmatureData["node"];
            var headJoint = LoadJointData(headNode, true);
            if (BoneOrder.Count > JointCount)
                throw new ArgumentOutOfRangeException(
                    $"Some vertex groups have no attached joint ({BoneOrder.Count}). Probably check that the exported model has no duplicated vertex groups.");
            return new JointsData(JointCount, headJoint);
        }

        public static string[] GetJointIds(XmlNode Node)
        {
            var names = new List<string>();
            var name = GetJointId(Node);
            if (Node.GetAttribute("type").Value == "JOINT")
                names.Add(name);
            var children = Node.Children("node");
            for (var i = 0; i < children.Count; ++i) names.AddRange(GetJointIds(children[i]));
            return names.ToArray();
        }

        private JointData LoadJointData(XmlNode JointNode, bool IsRoot)
        {
            var joint = ExtractMainJointData(JointNode, IsRoot);
            var Childs = JointNode.Children("node");
            for (var i = 0; i < Childs.Count; i++) joint.AddChild(LoadJointData(Childs[i], false));
            return joint;
        }

        private JointData ExtractMainJointData(XmlNode JointNode, bool IsRoot)
        {
            var jointId = GetJointId(JointNode);
            var jointSid = JointNode.GetAttribute("sid").Value;
            if (jointId != jointSid)
                throw new ArgumentException($"Joint ID '{jointId}' differs from Joint NAME '{jointSid}'.");
            var index = BoneOrder.IndexOf(jointId);
            if (IsRoot && index == -1) throw new ArgumentException("Root bone cannot have an index of -1");
            var matrixData = JointNode["matrix"].InnerText.Split(' ');
            var matrix = Mat4FromString(matrixData).Transposed();
            
            if (IsRoot)
            {
                matrix *= Matrix4x4.CreateFromQuaternion(QuaternionMath.FromEuler(Rotation * Mathf.Radian));
                matrix *= Correction;
                matrix *= Matrix4x4.CreateTranslation(Translation);
            }
            JointCount++;
            return new JointData(index, jointId, matrix);
        }

        private Matrix4x4 Mat4FromString(string[] rawData)
        {
            if (rawData.Length != 16) throw new ArgumentException("Not enough values.");
            return new Matrix4x4(float.Parse(rawData[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[2], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[4], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[5], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[6], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[7], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[8], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[9], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[10], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[11], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[12], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[13], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[14], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(rawData[15], NumberStyles.Any, CultureInfo.InvariantCulture)
            );
        }

        private static string GetJointId(XmlNode JointNode)
        {
            return JointNode.GetAttribute("id").Value.Replace($"{ArmatureName}_", string.Empty);
        }
    }
}