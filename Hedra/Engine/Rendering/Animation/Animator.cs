/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Core;
using Hedra.Framework;
using Hedra.Game;

namespace Hedra.Engine.Rendering.Animation
{
    public class Animator
    {
        private readonly Joint _rootJoint;
        private float _blendingAnimationTime;
        private JointName[] _blendJoints;
        private readonly ObjectPool<Dictionary<JointName, int>> _blendMapPool;
        private Dictionary<JointName, JointTransform> _currentBlendPose;
        private readonly Dictionary<JointName, JointTransform> _currentPose;
        private readonly ObjectPool<KeyFrame[]> _framePool;
        private Dictionary<JointName, JointTransform> _pose;
        private readonly ObjectPool<List<JointName>> _stringListPool;

        public Animator(Joint RootJoint)
        {
            _rootJoint = RootJoint;
            _blendMapPool = new ObjectPool<Dictionary<JointName, int>>(() => new Dictionary<JointName, int>());
            _stringListPool = new ObjectPool<List<JointName>>(() => new List<JointName>());
            _framePool = new ObjectPool<KeyFrame[]>(() => new KeyFrame[2]);
            _currentPose = new Dictionary<JointName, JointTransform>();
            _currentBlendPose = new Dictionary<JointName, JointTransform>();
            _pose = new Dictionary<JointName, JointTransform>();
            SetupDictionaries(_rootJoint);
        }

        public float AnimationTime { get; private set; }
        public float AnimationSpeed { get; set; } = 1f;
        public bool Stop { get; set; }

        public Animation AnimationPlaying { get; private set; }

        public Animation BlendingAnimation { get; private set; }

        private void SetupDictionaries(Joint Joint)
        {
            _currentBlendPose.Add(Joint.Name, JointTransform.Default);
            _currentPose.Add(Joint.Name, JointTransform.Default);
            _pose.Add(Joint.Name, JointTransform.Default);
            for (var i = 0; i < Joint.Children.Count; i++) SetupDictionaries(Joint.Children[i]);
        }

        public void Reset()
        {
            AnimationPlaying = null;
            BlendingAnimation = null;
        }

        public void ResetBlending()
        {
            BlendingAnimation?.Reset();
            BlendingAnimation = null;
        }

        public void PlayAnimation(Animation Animation)
        {
            AnimationPlaying = Animation;
            AnimationPlaying.Reset();
            AnimationTime = 0;
        }

        public void BlendAnimation(Animation Animation)
        {
            _blendJoints = CalculateJointsToBlend(Animation);
            BlendingAnimation = Animation;
            BlendingAnimation.Reset();
            _blendingAnimationTime = 0;
        }

        public bool Update()
        {
            if (AnimationPlaying == null || Stop) return false;

            var animationPose = CalculateCurrentAnimationPose(_currentPose);
            _pose = InterpolatePoses(_pose, animationPose, Time.IndependentDeltaTime * 16f, out var interpolated);
            if (interpolated) ApplyPoseToJoints(_pose, _rootJoint, Matrix4x4.Identity);
            IncreaseAnimationTime();
            return interpolated;
        }

        private void IncreaseAnimationTime()
        {
            if (Stop || GameSettings.Paused && !GameManager.InStartMenu) return;

            if (AnimationPlaying != null)
            {
                AnimationTime += Time.IndependentDeltaTime * AnimationPlaying.Speed * AnimationSpeed;
                AnimationPlaying.DispatchEvents(AnimationTime / AnimationPlaying.Length);
                if (AnimationTime > AnimationPlaying.Length)
                {
                    AnimationPlaying.Reset();
                    if (AnimationPlaying.Loop) AnimationTime %= AnimationPlaying.Length;
                    else AnimationPlaying = null;
                }
            }

            if (BlendingAnimation == null) return;
            _blendingAnimationTime += Time.IndependentDeltaTime * BlendingAnimation.Speed * AnimationSpeed;
            BlendingAnimation.DispatchEvents(_blendingAnimationTime / BlendingAnimation.Length);
            if (_blendingAnimationTime > BlendingAnimation.Length)
            {
                BlendingAnimation.Reset();
                BlendingAnimation = null;
            }
        }

        /**
         * This method returns the current animation pose of the entity. It returns
         * the desired local-space transforms for all the joints in a map, indexed
         * by the name of the joint that they correspond to.
         * 
         * The pose is calculated based on the previous and next keyframes in the
         * current animation. Each keyframe provides the desired pose at a certain
         * time in the animation, so the animated pose for the current time can be
         * calculated by interpolating between the previous and next keyframe.
         * 
         * This method first finds the preious and next keyframe, calculates how far
         * between the two the current animation is, and then calculated the pose
         * for the current animation time by interpolating between the transforms at
         * those keyframes.
         * 
         * @return The current pose as a map of the desired local-space transforms
         * for all the joints. The transforms are indexed by the name ID of
         * the joint that they should be applied to.
         */
        private Dictionary<JointName, JointTransform> CalculateCurrentAnimationPose(
            Dictionary<JointName, JointTransform> Pose)
        {
            var currentFrames = _framePool.GetObject();
            GetPreviousAndNextFrames(AnimationPlaying, AnimationTime, currentFrames);
            var currentProgression = CalculateProgression(currentFrames[0], currentFrames[1], AnimationTime);
            var pose = InterpolateJointPosesFromKeyframes(currentFrames[0], currentFrames[1], currentProgression,
                ref Pose);
            _framePool.PutObject(currentFrames);

            if (BlendingAnimation != null)
            {
                var blendingFrames = _framePool.GetObject();
                GetPreviousAndNextFrames(BlendingAnimation, _blendingAnimationTime, blendingFrames);
                var blendingProgression =
                    CalculateProgression(blendingFrames[0], blendingFrames[1], _blendingAnimationTime);
                var blendingPose = InterpolateJointPosesFromKeyframes(blendingFrames[0], blendingFrames[1],
                    blendingProgression, ref _currentBlendPose);
                _framePool.PutObject(blendingFrames);
                for (var i = 0; i < _blendJoints.Length; i++) pose[_blendJoints[i]] = blendingPose[_blendJoints[i]];
            }

            return pose;
        }

        /**
         * This is the method where the animator calculates and sets those all-
         * important "joint transforms" that I talked about so much in the tutorial.
         * 
         * This method applies the current pose to a given joint, and all of its
         * descendants. It does this by getting the desired local-transform for the
         * current joint, before applying it to the joint. Before applying the
         * transformations it needs to be converted from local-space to model-space
         * (so that they are relative to the model's origin, rather than relative to
         * the parent joint). This can be done by multiplying the local-transform of
         * the joint with the model-space transform of the parent joint.
         * 
         * The same thing is then done to all the child joints.
         * 
         * Finally the inverse of the joint's bind transform is multiplied with the
         * model-space transform of the joint. This basically "subtracts" the
         * joint's original bind (no animation applied) transform from the desired
         * pose transform. The result of this is then the transform required to move
         * the joint from its original model-space transform to it's desired
         * model-space posed transform. This is the transform that needs to be
         * loaded up to the vertex shader and used to transform the vertices into
         * the current pose.
         * 
         * @param currentPose
         * - a map of the local-space transforms for all the joints for
         * the desired pose. The map is indexed by the name of the joint
         * which the transform corresponds to.
         * @param joint
         * - the current joint which the pose should be applied to.
         * @param parentTransform
         * - the desired model-space transform of the parent joint for
         * the pose.
         */
        private void ApplyPoseToJoints(Dictionary<JointName, JointTransform> CurrentPose, Joint Joint,
            Matrix4x4 ParentTransform)
        {
            var currentLocalTransform = CurrentPose[Joint.Name];
            var currentTransform = currentLocalTransform.LocalTransform * ParentTransform;
            for (var i = 0; i < Joint.Children.Count; i++)
                ApplyPoseToJoints(CurrentPose, Joint.Children[i], currentTransform);
            currentTransform = Joint.InverseBindTransform * currentTransform;
            Joint.AnimatedTransform = currentTransform;
        }

        /**
         * Finds the previous keyframe in the animation and the next keyframe in the
         * animation, and returns them in an array of length 2. If there is no
         * previous frame (perhaps current animation time is 0.5 and the first
         * keyframe is at time 1.5) then the first keyframe is used as both the
         * previous and next keyframe. The last keyframe is used for both next and
         * previous if there is no next keyframe.
         * 
         * @return The previous and next keyframes, in an array which therefore will
         * always have a length of 2.
         */
        private static void GetPreviousAndNextFrames(Animation Animation, float Time, KeyFrame[] CurrentFrames)
        {
            var allFrames = Animation.KeyFrames;
            var previousFrame = allFrames[0];
            var nextFrame = allFrames[0];
            for (var i = 1; i < allFrames.Length; i++)
            {
                nextFrame = allFrames[i];
                if (nextFrame.TimeStamp > Time) break;
                previousFrame = allFrames[i];
            }

            ;
            CurrentFrames[0] = previousFrame;
            CurrentFrames[1] = nextFrame;
        }

        private static float CalculateProgression(KeyFrame PreviousFrame, KeyFrame NextFrame, float Time)
        {
            if (PreviousFrame == NextFrame) return 0.5f;

            var totalTime = NextFrame.TimeStamp - PreviousFrame.TimeStamp;
            var currentTime = Time - PreviousFrame.TimeStamp;
            return currentTime / totalTime;
        }

        private Dictionary<JointName, JointTransform> InterpolatePoses
        (Dictionary<JointName, JointTransform> Pose, IDictionary<JointName, JointTransform> TargetPose, float Progression,
            out bool Interpolated)
        {
            var interpolated = false;
            var pool = ArrayPool<KeyValuePair<JointName, JointTransform>>.Shared;
            var buffer = pool.Rent(Pose.Count);
            CopyToArray(Pose, buffer);
            for (var i = 0; i < Pose.Count; ++i)
            {
                var pair = buffer[i];
                var previousTransform = pair.Value;
                var nextTransform = TargetPose[pair.Key];
                var areEqual = previousTransform.Equals(nextTransform);
                Pose[pair.Key] =
                    areEqual
                        ? previousTransform
                        : JointTransform.Interpolate(previousTransform, nextTransform, Progression);
                interpolated |= !areEqual;
            }

            pool.Return(buffer);
            Interpolated = interpolated;
            return Pose;
        }

        private void CopyToArray(Dictionary<JointName, JointTransform> Pose, KeyValuePair<JointName, JointTransform>[] Buffer)
        {
            var i = 0;
            foreach (var pair in Pose)
                Buffer[i++] = pair;
        }

        private Dictionary<JointName, JointTransform> InterpolateJointPosesFromKeyframes(KeyFrame PreviousFrame,
            KeyFrame NextFrame, float Progression, ref Dictionary<JointName, JointTransform> Pose)
        {
            foreach (var pair in PreviousFrame.Pose)
            {
                var previousTransform = PreviousFrame.Pose[pair.Key];
                var nextTransform = NextFrame.Pose[pair.Key];
                var currentTransform = JointTransform.Interpolate(previousTransform, nextTransform, Progression);
                Pose[pair.Key] = currentTransform;
            }

            return Pose;
        }

        private JointName[] CalculateJointsToBlend(Animation Animation)
        {
            var blendedJointsCandidates = _blendMapPool.GetObject();
            for (var i = 0; i < Animation.KeyFrames.Length; i++)
                foreach (var pair in Animation.KeyFrames[i].Pose)
                    if (blendedJointsCandidates.ContainsKey(pair.Key))
                    {
                        if (Animation.KeyFrames[i - 1].Pose[pair.Key].Position ==
                            Animation.KeyFrames[i].Pose[pair.Key].Position
                            && Animation.KeyFrames[i - 1].Pose[pair.Key].Rotation ==
                            Animation.KeyFrames[i].Pose[pair.Key].Rotation)
                            blendedJointsCandidates[pair.Key] += 1;
                    }
                    else
                    {
                        blendedJointsCandidates.Add(pair.Key, 1);
                    }

            var blendedJoints = _stringListPool.GetObject();
            foreach (var pair in blendedJointsCandidates)
                if (pair.Value != Animation.KeyFrames.Length)
                    blendedJoints.Add(pair.Key);

            var toReturn = blendedJoints.ToArray();

            blendedJointsCandidates.Clear();
            _blendMapPool.PutObject(blendedJointsCandidates);
            blendedJoints.Clear();
            _stringListPool.PutObject(blendedJoints);

            return toReturn;
        }
    }
}