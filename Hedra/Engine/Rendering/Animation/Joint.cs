/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    /// Description of Joint.
    /// </summary>
    public class Joint
    {
        public readonly int Index;// ID
        public readonly string Name;
        public readonly List<Joint> Children = new List<Joint>();
    
        /**
         * The animated transform is the transform that gets loaded up to the shader
         * and is used to deform the vertices of the "skin". It represents the
         * transformation from the joint's bind position (original position in
         * model-space) to the joint's desired animation pose (also in model-space).
         * This matrix is calculated by taking the desired model-space transform of
         * the joint and multiplying it by the inverse of the starting model-space
         * transform of the joint.
         * 
         * @return The transformation matrix of the joint which is used to deform
         *         associated vertices of the skin in the shaders.
         */
        public Matrix4 AnimatedTransform {get; set;}
        public Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;
        
        /**
         * This returns the inverted model-space bind transform. The bind transform
         * is the original model-space transform of the joint (when no animation is
         * applied). This returns the inverse of that, which is used to calculate
         * the animated transform matrix which gets used to transform vertices in
         * the shader.
         * 
         * @return The inverse of the joint's bind transform (in model-space).
         */
        public Matrix4 InverseBindTransform {get; set;}
    
        private readonly Matrix4 LocalBindTransform;
        
        /**
         * @param index
         *            - the joint's index (ID).
         * @param name
         *            - the name of the joint. This is how the joint is named in the
         *            collada file, and so is used to identify which joint a joint
         *            transform in an animation keyframe refers to.
         * @param bindLocalTransform
         *            - the bone-space transform of the joint in the bind position.
         */
        public Joint(int Index, string Name, Matrix4 BindLocalTransform) {
            this.Index = Index;
            this.Name = Name;
            this.LocalBindTransform = BindLocalTransform;
        }
    
        /**
         * Adds a child joint to this joint. Used during the creation of the joint
         * hierarchy. Joints can have multiple children, which is why they are
         * stored in a list (e.g. a "hand" joint may have multiple "finger" children
         * joints).
         * 
         * @param child
         *            - the new child joint of this joint.
         */
        public void AddChild(Joint Child) {
            this.Children.Add(Child);
        }
        
        public Joint GetChild(string Name){
            if(this.Name == Name) return this;
            
            for(int i = 0; i < Children.Count; i++){
                Joint NewJoint = Children[i].GetChild(Name);
                if(NewJoint != null) return NewJoint;
            }
            
            return null;
        }
    
        /**
         * This is called during set-up, after the joints hierarchy has been
         * created. This calculates the model-space bind transform of this joint
         * like so: </br>
         * </br>
         * {@code bindTransform = parentBindTransform * localBindTransform}</br>
         * </br>
         * where "bindTransform" is the model-space bind transform of this joint,
         * "parentBindTransform" is the model-space bind transform of the parent
         * joint, and "localBindTransform" is the bone-space bind transform of this
         * joint. It then calculates and stores the inverse of this model-space bind
         * transform, for use when calculating the final animation transform each
         * frame. It then recursively calls the method for all of the children
         * joints, so that they too calculate and store their inverse bind-pose
         * transform.
         * 
         * @param parentBindTransform
         *            - the model-space bind transform of the parent joint.
         */
        public void CalculateInverseBindTransform(Matrix4 ParentBindTransform) {
            Matrix4 BindTransform = LocalBindTransform * ParentBindTransform;
            InverseBindTransform = BindTransform.Inverted();
            
            for(int i = 0; i < Children.Count; i++){
                Children[i].CalculateInverseBindTransform(BindTransform);
            }
        }
    }
}
