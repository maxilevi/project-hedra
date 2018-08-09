/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation
{
	
	public class Animator
	{
	    public float AnimationTime { get; private set; }
	    public float AnimationSpeed { get; set; } = 2f;
	    public float BlendingAnimationTime { get; private set; }
        public bool Stop { get; set; }
		private readonly AnimatedModel _entity;
		private Animation _currentAnimation;
		private Animation _blendingAnimation;
		private string[] _blendJoints;
		private Dictionary<string, JointTransform> _pose;
	    private Dictionary<string, JointTransform> _lastPose;
	    private int _frameCounter;

        public Animator(AnimatedModel Entity)
		{
			_entity = Entity;
		}

		public void PlayAnimation(Animation Animation)
		{
			_currentAnimation = Animation;
			AnimationTime = 0;
			if (_pose == null)
			{
				_pose = CalculateCurrentAnimationPose();
			}
		}
		
		public void BlendAnimation(Animation Animation)
		{
			_blendJoints = CalculateJointsToBlend(Animation);
			_blendingAnimation = Animation;
		    BlendingAnimationTime = 0;
		}
		
		#region Blending Axuliary Methods
		
		private string[] CalculateJointsToBlend(Animation Animation)
		{
			var blendedJointsCandidates = new Dictionary<string, int>();
			for(var i = 0; i < Animation.KeyFrames.Length; i++)
			{
				foreach(var pair in Animation.KeyFrames[i].Pose)
				{
					if(blendedJointsCandidates.ContainsKey(pair.Key))
					{
						if(Animation.KeyFrames[i-1].Pose[pair.Key].Position == Animation.KeyFrames[i].Pose[pair.Key].Position 
						   && Animation.KeyFrames[i-1].Pose[pair.Key].Rotation == Animation.KeyFrames[i].Pose[pair.Key].Rotation)
						{						
							blendedJointsCandidates[pair.Key] += 1; 
						}
					}
					else
					{
						blendedJointsCandidates.Add(pair.Key, 1);
					}
				}
			}		
			var blendedJoints = new List<string>();
			foreach(var pair in blendedJointsCandidates)
			{
				if(pair.Value != Animation.KeyFrames.Length)
				{
					blendedJoints.Add(pair.Key);
				}
			}
			
			return blendedJoints.ToArray();
		}

		#endregion
		
		/**
		 * This method should be called each frame to update the animation currently
		 * being played. This increases the animation time (and loops it back to
		 * zero if necessary), finds the pose that the entity should be in at that
		 * time of the animation, and then applies that pose to all the model's
		 * joints by setting the joint transforms.
		 */
		public void Update()
		{
			if (_currentAnimation == null || Stop) return;

		    var animationPose = _frameCounter % 2 != 0 || _lastPose == null
		        ? _lastPose = CalculateCurrentAnimationPose()
		        : _lastPose;
			_pose = InterpolatePoses(_pose, animationPose, Time.IndependantDeltaTime * 8f);
			ApplyPoseToJoints(_pose, _entity.RootJoint, Matrix4.Identity);
			if(_frameCounter % 2 == 0) IncreaseAnimationTime();
		    _frameCounter++;

		}
	
		/**
		 * Increases the current animation time which allows the animation to
		 * progress. If the current animation has reached the end then the timer is
		 * reset, causing the animation to loop.
		 */
		private void IncreaseAnimationTime() 
		{
			if(GameManager.InMenu || (!_currentAnimation.Loop && AnimationTime > _currentAnimation.Length) || Stop) 
				return;

			AnimationTime += Time.IndependantDeltaTime * _currentAnimation.Speed * AnimationSpeed;
            _currentAnimation.DispatchEvents(AnimationTime / _currentAnimation.Length);	
			if (AnimationTime > _currentAnimation.Length){
				_currentAnimation.Reset();
				if(_currentAnimation.Loop) AnimationTime %= _currentAnimation.Length;	
			}

            if(_blendingAnimation == null) return;
		    BlendingAnimationTime += Time.IndependantDeltaTime * _blendingAnimation.Speed * AnimationSpeed;
		    _blendingAnimation.DispatchEvents(BlendingAnimationTime / _blendingAnimation.Length);
		    if (BlendingAnimationTime > _blendingAnimation.Length)
		    {
		        _blendingAnimation.Reset();
		        _blendingAnimation = null;
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
		 *         for all the joints. The transforms are indexed by the name ID of
		 *         the joint that they should be applied to.
		 */
		private Dictionary<string, JointTransform> CalculateCurrentAnimationPose() 
		{
			var currentFrames = GetPreviousAndNextFrames(_currentAnimation, AnimationTime);
			var currentProgression = CalculateProgression(currentFrames[0], currentFrames[1], AnimationTime);
			var pose = InterpolateJointPosesFromKeyframes(currentFrames[0], currentFrames[1], currentProgression);

			if (_blendingAnimation != null)
			{
				var blendingFrames = GetPreviousAndNextFrames(_blendingAnimation, BlendingAnimationTime);
				var blendingProgression = CalculateProgression(blendingFrames[0], blendingFrames[1], BlendingAnimationTime);
				var blendingPose = InterpolateJointPosesFromKeyframes(blendingFrames[0], blendingFrames[1], blendingProgression);
				for (var i = 0; i < _blendJoints.Length; i++)
				{
					pose[_blendJoints[i]] = blendingPose[_blendJoints[i]];
				}
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
		 *            - a map of the local-space transforms for all the joints for
		 *            the desired pose. The map is indexed by the name of the joint
		 *            which the transform corresponds to.
		 * @param joint
		 *            - the current joint which the pose should be applied to.
		 * @param parentTransform
		 *            - the desired model-space transform of the parent joint for
		 *            the pose.
		 */
		private void ApplyPoseToJoints(Dictionary<String, JointTransform> CurrentPose, Joint Joint, Matrix4 ParentTransform)
		{
			var currentLocalTransform = CurrentPose[Joint.Name];
			var currentTransform = currentLocalTransform.LocalTransform * ParentTransform;
			for (var i = 0; i < Joint.Children.Count; i++){
				ApplyPoseToJoints(CurrentPose, Joint.Children[i], currentTransform);
			}
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
		 *         always have a length of 2.
		 */
		private KeyFrame[] GetPreviousAndNextFrames(Animation Animation, float Time)
		{
			var allFrames = Animation.KeyFrames;
			var previousFrame = allFrames[0];
			var nextFrame = allFrames[0];
			for (var i = 1; i < allFrames.Length; i++) {
				nextFrame = allFrames[i];
				if (nextFrame.TimeStamp > Time) {
					break;
				}
				previousFrame = allFrames[i];
			}
			return new [] { previousFrame, nextFrame };
		}
	
		/**
		 * Calculates how far between the previous and next keyframe the current
		 * animation time is, and returns it as a value between 0 and 1.
		 * 
		 * @param previousFrame
		 *            - the previous keyframe in the animation.
		 * @param nextFrame
		 *            - the next keyframe in the animation.
		 * @return A number between 0 and 1 indicating how far between the two
		 *         keyframes the current animation time is.
		 */
		private float CalculateProgression(KeyFrame PreviousFrame, KeyFrame NextFrame, float Time)
		{
			if(PreviousFrame == NextFrame) return 0.5f;
			
			var totalTime = NextFrame.TimeStamp - PreviousFrame.TimeStamp;
			var currentTime = Time - PreviousFrame.TimeStamp;
			return currentTime / totalTime;
		}
		
		private Dictionary<string, JointTransform> InterpolatePoses
			(Dictionary<string, JointTransform> Pose, Dictionary<string, JointTransform> TargetPose, float Progression)
		{
		    var newPose = new Dictionary<string, JointTransform>();
            foreach (var pair in Pose) {
				var previousTransform = pair.Value;
				var nextTransform = TargetPose[pair.Key];
                newPose.Add(pair.Key, JointTransform.Interpolate(previousTransform, nextTransform, Progression));
			}
			return newPose;
		}
	
		/**
		 * Calculates all the local-space joint transforms for the desired current
		 * pose by interpolating between the transforms at the previous and next
		 * keyframes.
		 * 
		 * @param previousFrame
		 *            - the previous keyframe in the animation.
		 * @param nextFrame
		 *            - the next keyframe in the animation.
		 * @param progression
		 *            - a number between 0 and 1 indicating how far between the
		 *            previous and next keyframes the current animation time is.
		 * @return The local-space transforms for all the joints for the desired
		 *         current pose. They are returned in a map, indexed by the name of
		 *         the joint to which they should be applied.
		 */		
		private Dictionary<string, JointTransform> InterpolateJointPosesFromKeyframes(KeyFrame PreviousFrame, KeyFrame NextFrame, float Progression)
		{
			var currentPose = new Dictionary<string, JointTransform>();
			foreach(var pair in PreviousFrame.Pose) {
				var previousTransform = PreviousFrame.Pose[pair.Key];
				var nextTransform = NextFrame.Pose[pair.Key];
				var currentTransform = JointTransform.Interpolate(previousTransform, nextTransform, Progression);
				currentPose.Add(pair.Key, currentTransform);
			}
			return currentPose;
		}
		
		public Animation AnimationPlaying => _currentAnimation;
		
		public Animation BlendingAnimation => _blendingAnimation;
	}
}
