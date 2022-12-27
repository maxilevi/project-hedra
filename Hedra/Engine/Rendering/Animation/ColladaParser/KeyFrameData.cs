/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of KeyFrameData.
    /// </summary>
    public class KeyFrameData
    {
        public readonly List<JointTransformData> JointTransforms = new List<JointTransformData>();
        public readonly float Time;
        public bool AlignArmatureRotation = false;
        public Vector3 ArmaturePosition = Vector3.Zero;
        public Vector3 ArmatureRotation = Vector3.Zero;
        public Vector3 ArmatureScale = Vector3.One;

        public KeyFrameData(float Time)
        {
            this.Time = Time;
        }

        public void AddJointTransform(JointTransformData Transform)
        {
            JointTransforms.Add(Transform);
        }

        public void BakeArmatureTransformations() 
        {
            var scale = Matrix4x4.CreateScale(ArmatureScale);
            var rotation = Matrix4x4.CreateFromQuaternion(QuaternionMath.FromEuler(ArmatureRotation * Mathf.Radian));
            if (AlignArmatureRotation)
            {
                rotation *= Matrix4x4.CreateRotationX(-90f * Mathf.Radian);
            }
            var translation = Matrix4x4.CreateTranslation(ArmaturePosition);
            var transform = JointTransforms[0];
            transform.JointLocalTransform = transform.JointLocalTransform * scale * rotation * translation;

            ArmaturePosition = Vector3.Zero;
            ArmatureRotation = Vector3.Zero;
            ArmatureScale = Vector3.Zero;
            AlignArmatureRotation = false;
        }
    }
}