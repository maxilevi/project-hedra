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
using System.Linq;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation
{
	
	public class Animator
	{
		public AnimatedModel Entity {get; private set;}
		public Animation AnimationPlaying { get { if(TargetAnimation != null) return this.TargetAnimation; else return this.CurrentAnimation;} }
		public Animation BlendingAnimation => BlendAnimator?.AnimationPlaying;
	    public float AnimationTime {get; private set;}
	    public float AnimationSpeed { get; set; } = 1f;
        public bool AnimationBlending {get; set;}
		public bool Stop {get; set;}
		private Animation CurrentAnimation;
		private Animation TargetAnimation;
		private float TargetAnimationTime, BlendAnimationTime;
		private Animator BlendAnimator;
		private string[] BlendJoints;
		private bool ShouldExitBlend;
		private Dictionary<string, JointTransform> LastPose;
		public OnAnimationHandler OnExitBlend;
	
		/**
		 * @param entity
		 *            - the entity which will by animated by this animator.
		 */
		public Animator(AnimatedModel Entity) {
			this.Entity = Entity;
		}
	
		/**
		 * Indicates that the entity should carry out the given animation. Resets
		 * the animation time so that the new animation starts from the beginning.
		 * 
		 * @param animation
		 *            - the new animation to carry out.
		 */
		public void PlayAnimation(Animation Animation) {
			if(CurrentAnimation == null){
				this.AnimationTime = 0;
				this.CurrentAnimation = Animation;
			}else{
				if(Animation == CurrentAnimation){
					//this.CurrentAnimation.DispatchEvents(1.0f);
					this.AnimationTime = 0;
					this.CurrentAnimation = this.TargetAnimation ?? Animation;
				}
				//if(TargetAnimation != null)
				//	this.TargetAnimation.DispatchEvents(1.0f);
				this.TargetAnimation = Animation;
				this.TargetAnimationTime = 0;
			}
		}
		
		public void BlendAnimation(Animation Animation){
			if(Animation == null) return;
			if(BlendAnimator?.AnimationPlaying != null)
			{
			    Animation.DispatchEvents(1.0f);
			    return;
			}
				this.AnimationBlending = true;
			
			if(BlendAnimator == null)
				BlendAnimator = new Animator(Entity);
			
			BlendJoints = this.CalculateJointsToBlend(Animation);
			BlendAnimator.Reset();
			this.BlendAnimationTime = 0;
			BlendAnimator.PlayAnimation(Animation);
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
		public void Update() {
			if (CurrentAnimation == null || Stop) return;
			
			LastPose = this.CalculateCurrentJointAnimationPose();
			
			if(TargetAnimation == null){
				this.IncreaseAnimationTime();
				Dictionary<string, Matrix4> CurrentPose = this.CalculateCurrentAnimationPose();
				
				this.ManageBlending(CurrentPose);
				
				this.ApplyPoseToJoints(CurrentPose, Entity.RootJoint, Matrix4.Identity);
			}else{
				if(TargetAnimationTime >= 1){
					this.SwitchAnimations();
					return;
				}
				
				this.IncreaseAnimationTime();
				this.IncreaseTargetAnimationTime();
				Dictionary<String, Matrix4> TargetPose = this.CalculateTargetAnimationPose();
				
				this.ManageBlending(TargetPose);
				
				this.ApplyPoseToJoints(TargetPose, Entity.RootJoint, Matrix4.Identity);
			}
		}
		
		private void ManageBlending(Dictionary<string, Matrix4> CurrentPose){
			if(this.Stop) 
				return;
			
			if(this.AnimationBlending){
				if(BlendAnimationTime >= 1)
					BlendAnimator.Update();
				
				if(!this.ShouldExitBlend){
					BlendAnimationTime += Engine.Time.IndependantDeltaTime * 4f * this.BlendingAnimation.Speed;
					BlendAnimationTime = Mathf.Clamp(BlendAnimationTime, 0,1);
					
					Dictionary<string, JointTransform> JointPose = LastPose;
					Dictionary<string, JointTransform> BlendPose = BlendAnimator.CalculateCurrentJointAnimationPose();
					foreach(KeyValuePair<string, JointTransform> Pair in JointPose) {
						
						JointTransform PreviousTransform = JointPose[Pair.Key];
						JointTransform NextTransform = BlendPose[Pair.Key];
						JointTransform CurrentTransform = JointTransform.Interpolate(PreviousTransform, NextTransform, BlendAnimationTime);
						
						if(this.BlendJoints.Contains(Pair.Key))
							CurrentPose[Pair.Key] = CurrentTransform.LocalTransform;
						
					}
					
					if(BlendAnimator.AnimationTime >= BlendAnimator.AnimationPlaying.Length){
						this.ShouldExitBlend = true;
						this.BlendAnimator.Stop = true;
					}
				}else{
					BlendAnimationTime -= Engine.Time.IndependantDeltaTime * 4f * this.CurrentAnimation.Speed;
					BlendAnimationTime = Mathf.Clamp(BlendAnimationTime, 0,1);
					
					Dictionary<string, JointTransform> JointPose = this.CalculateCurrentJointAnimationPose();
					Dictionary<string, JointTransform> BlendPose = BlendAnimator.CalculateCurrentJointAnimationPose();
					foreach(KeyValuePair<string, JointTransform> Pair in JointPose) {
						
						JointTransform PreviousTransform = JointPose[Pair.Key];
						JointTransform NextTransform = BlendPose[Pair.Key];
						JointTransform CurrentTransform = JointTransform.Interpolate(PreviousTransform, NextTransform, BlendAnimationTime);
						
						if(this.BlendJoints.Contains(Pair.Key))
							CurrentPose[Pair.Key] = CurrentTransform.LocalTransform;
						
					}
					
					if(BlendAnimationTime <= 0){
						this.ShouldExitBlend = false;
						this.AnimationBlending = false;
						this.BlendJoints = null;
						this.BlendAnimator.Stop = false;
						this.BlendAnimator.Reset();
						if(OnExitBlend != null)
							OnExitBlend.Invoke(this.BlendingAnimation);
					}
				}
			}
		}
		
		public void SwitchAnimations(){
			this.AnimationTime = 0;
			this.CurrentAnimation = this.TargetAnimation;
			this.TargetAnimation = null;
			this.TargetAnimationTime = 0;
			this.Update();
		}
		
		public void Reset(){
			this.BlendAnimationTime = 0;
			this.AnimationTime = 0;
			this.CurrentAnimation = null;
			this.TargetAnimation = null;
			this.TargetAnimationTime = 0;
		}
		
		public void StopBlend(){
			this.BlendAnimationTime = 0;
			this.AnimationBlending = false;
			if(this.BlendAnimator != null){
				if(this.BlendAnimator.AnimationPlaying != null)
					this.BlendAnimator.AnimationPlaying.DispatchEvents(1f);
			 	this.BlendAnimator.Reset();
			}
		}
		
		public void ExitBlend(){
			this.ShouldExitBlend = true;
		}
		
		public JointTransform TransformFromJoint(Joint TargetJoint){
			return LastPose[TargetJoint.Name];
		}
	
		/**
		 * Increases the current animation time which allows the animation to
		 * progress. If the current animation has reached the end then the timer is
		 * reset, causing the animation to loop.
		 */
		private void IncreaseAnimationTime() {
			if(GameManager.InMenu || (!this.CurrentAnimation.Loop && AnimationTime > CurrentAnimation.Length) || Stop) 
				return;
			AnimationTime += Time.IndependantDeltaTime * this.CurrentAnimation.Speed * AnimationSpeed;
			
			this.CurrentAnimation.DispatchEvents(AnimationTime / CurrentAnimation.Length);
			
			if (AnimationTime > CurrentAnimation.Length){
				this.CurrentAnimation.Reset();
				if(this.CurrentAnimation.Loop)
					this.AnimationTime %= CurrentAnimation.Length;	
			}
		}
		
		private void IncreaseTargetAnimationTime() {
			if(GameManager.InMenu || Stop) 
				return;
			TargetAnimationTime += Engine.Time.IndependantDeltaTime * 4f * this.TargetAnimation.Speed;
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
		private Dictionary<String, Matrix4> CalculateCurrentAnimationPose() {
			KeyFrame[] Frames = this.GetPreviousAndNextFrames(AnimationTime);
			float Progression = this.CalculateProgression(Frames[0], Frames[1], this.AnimationTime);
			return this.InterpolatePoses(Frames[0], Frames[1], Progression);
		}
		
		private Dictionary<String, JointTransform> CalculateCurrentJointAnimationPose() {
			KeyFrame[] Frames = this.GetPreviousAndNextFrames(AnimationTime);
			float Progression = this.CalculateProgression(Frames[0], Frames[1], this.AnimationTime);
			return this.InterpolateJointPoses(Frames[0], Frames[1], Progression);
		}
		
		private Dictionary<String, Matrix4> CalculateTargetAnimationPose() {
			KeyFrame[] Frames = this.GetPreviousAndNextFrames(AnimationTime);
			float Progression = this.CalculateProgression(Frames[0], Frames[1], this.AnimationTime);
			
			Dictionary<string, JointTransform> Pose = new Dictionary<string, JointTransform>();
			foreach(KeyValuePair<string, JointTransform> Pair in Frames[0].Pose) {
				JointTransform PreviousTransform = Frames[0].Pose[Pair.Key];
				JointTransform NextTransform = Frames[1].Pose[Pair.Key];
				JointTransform CurrentTransform = JointTransform.Interpolate(PreviousTransform, NextTransform, Progression);
				Pose.Add(Pair.Key, CurrentTransform);
			}
			
			KeyFrame CurrentFrame = new KeyFrame(1f, Pose);

			return this.InterpolatePoses(CurrentFrame, TargetAnimation.KeyFrames[0], TargetAnimationTime);
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
		private void ApplyPoseToJoints(Dictionary<String, Matrix4> CurrentPose, Joint Joint, Matrix4 ParentTransform) {
			Matrix4 CurrentLocalTransform = CurrentPose[Joint.Name];
			Matrix4 CurrentTransform = CurrentLocalTransform * ParentTransform;
			for (int i = 0; i < Joint.Children.Count; i++){
				this.ApplyPoseToJoints(CurrentPose, Joint.Children[i], CurrentTransform);
			}
			CurrentTransform = Joint.InverseBindTransform * CurrentTransform;
			Joint.AnimatedTransform = CurrentTransform;
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
		private KeyFrame[] GetPreviousAndNextFrames(float Time) {
			KeyFrame[] AllFrames = CurrentAnimation.KeyFrames;
			KeyFrame PreviousFrame = AllFrames[0];
			KeyFrame NextFrame = AllFrames[0];
			for (int i = 1; i < AllFrames.Length; i++) {
				NextFrame = AllFrames[i];
				if (NextFrame.TimeStamp > Time) {
					break;
				}
				PreviousFrame = AllFrames[i];
			}
			return new KeyFrame[] { PreviousFrame, NextFrame };
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
		private float CalculateProgression(KeyFrame PreviousFrame, KeyFrame NextFrame, float AnimationTime) {
			if(PreviousFrame == NextFrame) return 0.5f;
			
			float TotalTime = NextFrame.TimeStamp - PreviousFrame.TimeStamp;
			float CurrentTime = AnimationTime - PreviousFrame.TimeStamp;
			return CurrentTime / TotalTime;
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
		private Dictionary<String, Matrix4> InterpolatePoses(KeyFrame PreviousFrame, KeyFrame NextFrame, float Progression) {
			Dictionary<string, Matrix4> CurrentPose = new Dictionary<string, Matrix4>();
			foreach(KeyValuePair<string, JointTransform> Pair in PreviousFrame.Pose) {
				JointTransform PreviousTransform = PreviousFrame.Pose[Pair.Key];
				JointTransform NextTransform = NextFrame.Pose[Pair.Key];
				JointTransform CurrentTransform = JointTransform.Interpolate(PreviousTransform, NextTransform, Progression);
				CurrentPose.Add(Pair.Key, CurrentTransform.LocalTransform);
			}
			return CurrentPose;
		}
		
		private Dictionary<String, JointTransform> InterpolateJointPoses(KeyFrame PreviousFrame, KeyFrame NextFrame, float Progression) {
			Dictionary<string, JointTransform> CurrentPose = new Dictionary<string, JointTransform>();
			foreach(KeyValuePair<string, JointTransform> Pair in PreviousFrame.Pose) {
				JointTransform PreviousTransform = PreviousFrame.Pose[Pair.Key];
				JointTransform NextTransform = NextFrame.Pose[Pair.Key];
				JointTransform CurrentTransform = JointTransform.Interpolate(PreviousTransform, NextTransform, Progression);
				CurrentPose.Add(Pair.Key, CurrentTransform);
			}
			return CurrentPose;
		}
	}
}
