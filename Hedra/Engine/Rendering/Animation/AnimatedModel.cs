/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.Animation
{ 
	/// <summary>
	/// Description of AnimatedModel.
	/// </summary>
	public class AnimatedModel : IDisposable, IRenderable, ICullableModel
	{
        //Skin
	    public static Shader DefaultShader = Shader.Build("Shaders/AnimatedModel.vert", "Shaders/AnimatedModel.frag");
	    public static Shader DeathShader = AnimatedModelShader.GenerateDeathShader();
        private Shader Shader = DefaultShader;
        private VBO<Vector3> Vertices, Normals, JointIds, VertexWeights;
		private VBO<Vector3> Colors;
		private VBO<uint> Indices;
        private bool Disposed = false;
	    public VAO<Vector3, Vector3, Vector3, Vector3, Vector3> Data { get; private set; }
	    public Vector3[] WeightsArray { get; private set; }
	    public Vector3[] JointIdsArray { get; private set; }
	    public Vector3[] VerticesArray { get; private set; }
        //Skeleton
        public Joint RootJoint {get;}
		public int JointCount {get;}
		public Animator Animator {get;}
		public Vector3 Position {get; set;}
		public bool Enabled {get; set;} 
		public bool ApplyFog {get; set;}
		public float Alpha {get; set;}
	    public float DisposeTime {get; set;}
	    public Vector4 Tint {get; set;}
		public Vector4 BaseTint {get; set;}
		public Vector3 Scale {get; set;}
        //
	    public Box CullingBox { get; set; }
	    public bool DontCull { get; set; }
        /**
		 * Creates a new entity capable of animation. The inverse bind transform for
		 * all joints is calculated in this constructor. The bind transform is
		 * simply the original (no pose applied) transform of a joint in relation to
		 * the model's origin (model-space). The inverse bind transform is simply
		 * that but inverted.
		 * 
		 * @param model
		 *            - the VAO containing the mesh data for this entity. This
		 *            includes vertex positions, normals, texture coords, IDs of
		 *            joints that affect each vertex, and their corresponding
		 *            weights.
		 * @param texture
		 *            - the diffuse texture for the entity.
		 * @param rootJoint
		 *            - the root joint of the joint hierarchy which makes up the
		 *            "skeleton" of the entity.
		 * @param jointCount
		 *            - the number of joints in the joint hierarchy (skeleton) for
		 *            this entity.
		 * 
		 */
        public AnimatedModel(ModelData Data, Joint RootJoint, int JointCount) {

            this.WeightsArray = Data.VertexWeights;
            this.JointIdsArray = Data.JointIds;
            this.VerticesArray = Data.Vertices;

            ThreadManager.ExecuteOnMainThread(delegate
            {
                this.Vertices = new VBO<Vector3>(Data.Vertices, Data.Vertices.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                this.Colors = new VBO<Vector3>(Data.Colors, Data.Colors.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                this.Normals = new VBO<Vector3>(Data.Normals, Data.Normals.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                this.Indices = new VBO<uint>(Data.Indices, Data.Indices.Length * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
                this.JointIds = new VBO<Vector3>(Data.JointIds, Data.JointIds.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
                this.VertexWeights = new VBO<Vector3>(Data.VertexWeights, Data.VertexWeights.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);

                this.Data = new VAO<Vector3, Vector3, Vector3, Vector3, Vector3>(Vertices, Colors, Normals, JointIds, VertexWeights);
            });
			this.RootJoint = RootJoint;
			this.JointCount = JointCount;
			this.Animator = new Animator(this);
			this.RootJoint.CalculateInverseBindTransform(Matrix4.Identity);
			this.Alpha = 1.0f;
			this.Tint = Vector4.One;
			this.Scale = Vector3.One * 1.0f;
            this.ApplyFog = true;
            this.Enabled = true;
			DisposeManager.Add(this);
			DrawManager.Add(this);
		}
		
		/**
		 * Deletes the OpenGL objects associated with this entity, namely the model
		 * (VAO) and texture.
		 */
		public void Dispose() {
            if (!BuffersCreated)
            {
                ThreadManager.ExecuteOnMainThread(delegate
                {
                    this.Disposed = true;
                    Data?.Dispose();
                    JointIds?.Dispose();
                    Indices?.Dispose();
                    Normals?.Dispose();
                    Colors?.Dispose();
                    Vertices?.Dispose();
                    VertexWeights?.Dispose();
                    DrawManager.Remove(this);
                });
            }
            else {
                this.Disposed = true;
                Data?.Dispose();
                JointIds?.Dispose();
                Indices?.Dispose();
                Normals?.Dispose();
                Colors?.Dispose();
                Vertices?.Dispose();
                VertexWeights?.Dispose();
                DrawManager.Remove(this);
            }
		}
		
		public void Draw(){
	
			Matrix4 ViewMat = LocalPlayer.Instance.View.ModelViewMatrix;
			Matrix4 ProjectionViewMat = ViewMat * DrawManager.FrustumObject.ProjectionMatrix;
			this.DrawModel(ProjectionViewMat, ViewMat);
		}
		
		public void DrawModel(Matrix4 ProjectionViewMat, Matrix4 ViewMatrix){
			if(!Enabled || Disposed || Data == null) return;
			GraphicsLayer.Enable(EnableCap.DepthTest);
		    GraphicsLayer.Disable(EnableCap.Blend);
            if (Alpha < 0.95f) GraphicsLayer.Enable(EnableCap.Blend);

            Shader.Bind();

		    if (Shader == DeathShader && CompatibilityManager.SupportsGeometryShaders)
		    {
		        GraphicsLayer.Enable(EnableCap.Blend);
		        DeathShader["viewMatrix"] = ViewMatrix;
		        DeathShader["disposeTime"] = DisposeTime;
            }

            Shader["jointTransforms"] = JointTransforms;
			Shader["projectionViewMatrix"] = ProjectionViewMat;
		    if (Shader != DeathShader || !CompatibilityManager.SupportsGeometryShaders)
		    {
		        DefaultShader["PlayerPosition"] = GameManager.Player.Position;
		    }

		    if(GameSettings.Shadows){
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
				Shader["ShadowTex"] = 0;
		        if (Shader != DeathShader || !CompatibilityManager.SupportsGeometryShaders)
		        {
		            DefaultShader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
		        }
		    }
			Shader["UseShadows"] = GameSettings.Shadows ? 1.0f : 0.0f;
			Shader["UseFog"] = ApplyFog ? 1 : 0;
			Shader["Alpha"] = Alpha;
			Shader["Tint"] = Tint + BaseTint;
			
			Data.Bind();

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.ID);
			GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
			
			Data.Unbind();

			Shader.UnBind();

			GraphicsLayer.Disable(EnableCap.Blend);
		}
		
		/**
		 * Instructs this entity to carry out a given animation. To do this it
		 * basically sets the chosen animation as the current animation in the
		 * {@link Animator} object.
		 * 
		 * @param animation
		 *            - the animation to be carried out.
		 */
		public void PlayAnimation(Animation Animation) {
			Animator.PlayAnimation(Animation);
		}
		
		public void BlendAnimation(Animation ToBlend){
			Animator.BlendAnimation(ToBlend);
		}
	
		/**
		 * Updates the animator for this entity, basically updating the animated
		 * pose of the entity. Must be called every frame.
		 */
		public void Update() {
			Animator.Update();
		}
	
		/**
		 * Gets an array of the all important model-space transforms of all the
		 * joints (with the current animation pose applied) in the entity. The
		 * joints are ordered in the array based on their joint index. The position
		 * of each joint's transform in the array is equal to the joint's index.
		 * 
		 * @return The array of model-space transforms of the joints in the current
		 *         animation pose.
		 */
		public Matrix4[] JointTransforms{
			get{ 
				Matrix4[] JointMatrices = new Matrix4[JointCount];
				this.AddJointsToArray(RootJoint, JointMatrices);
				return JointMatrices;
			}
		}
	
		/**
		 * This adds the current model-space transform of a joint (and all of its
		 * descendants) into an array of transforms. The joint's transform is added
		 * into the array at the position equal to the joint's index.
		 * 
		 * @param headJoint
		 *            - the current joint being added to the array. This method also
		 *            adds the transforms of all the descendents of this joint too.
		 * @param jointMatrices
		 *            - the array of joint transforms that is being filled.
		 */
		private void AddJointsToArray(Joint HeadJoint, Matrix4[] JointMatrices) {
			JointMatrices[HeadJoint.Index] = HeadJoint.AnimatedTransform * ScaleMatrix * RotationMatrix * TransformationMatrix * PositionMatrix;
			for(int i = 0; i < HeadJoint.Children.Count; i++){
				this.AddJointsToArray(HeadJoint.Children[i], JointMatrices);
			}
		}
		
		public Matrix4 MatrixFromJoint(Joint TargetJoint)
        {
			return JointTransforms[TargetJoint.Index];
		}
		
		public Vector3 TransformFromJoint(Vector3 Position, Joint TargetJoint)
        {
			
			Vector3 TotalLocalPos = Vector3.Zero;
			Matrix4 JointTransform = JointTransforms[ TargetJoint.Index ];
			Vector3 PosePosition = Vector3.TransformPosition(Position, JointTransform);
			TotalLocalPos += PosePosition * 1f;//WeightsArray[TargetJoint.Index][i];
			
			return TotalLocalPos;
		}
		
		public Vector3 JointDefaultPosition(Joint TargetJoint)
        {
			Vector3 Average = Vector3.Zero;
			float Count = 0;
			for(var i = 0; i < VerticesArray.Length; i++)
            {
				if(JointIdsArray[i].X == TargetJoint.Index || JointIdsArray[i].Y == TargetJoint.Index || JointIdsArray[i].Z == TargetJoint.Index){
					Average += VerticesArray[i];
					Count++;
				}
			}
			Average /= Count;
			return Average;
		}

	    public void SwitchShader(Shader NewShader)
	    {
	        this.Shader = NewShader;
	    }

	    public void ReplaceColors(Vector3[] ColorArray)
	    {
	        ThreadManager.ExecuteOnMainThread(delegate
	        {
                if (ColorArray.Length != this.Colors.Count)
                    throw new ArgumentOutOfRangeException("The new colors array can't have more data than the previous one.");


	            this.Colors.Update(ColorArray, ColorArray.Length * Vector3.SizeInBytes);
	        });
	    }

	    private bool BuffersCreated => Data != null;

        /// <summary>
        /// A transformation matrix used for additional scaling or rotation. Applied before the translation.
        /// </summary>
	    public Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;

	    public float AnimationSpeed
	    {
	        get => Animator.AnimationSpeed;
	        set => Animator.AnimationSpeed = value;
	    }

	    public bool Pause
	    {
	        get => Animator.Stop;
	        set => Animator.Stop = value;
	    }

        private Vector3 _rotation;
		public Vector3 Rotation {
			get{ return _rotation; }
			set{
				Vector3 newValue = value;
				if(float.IsNaN(value.X))
					newValue = new Vector3(0, newValue.Y, newValue.Z);
				if(float.IsNaN(value.Y))
					newValue = new Vector3(newValue.X, 0, newValue.Z);
				if(float.IsNaN(value.Z))
					newValue = new Vector3(newValue.X, newValue.Y, 0);
				
				_rotation = newValue;
			}
		}
		
		private Vector3 _cacheRotation = -Vector3.One;
		private Matrix4 _rotationCache;
		private Matrix4 RotationMatrix{
			get{
				if(this.Rotation != _cacheRotation){
					_rotationCache = Matrix4.CreateRotationX( Rotation.X * Mathf.Radian ) *
									Matrix4.CreateRotationY( Rotation.Y * Mathf.Radian ) *
									Matrix4.CreateRotationZ( Rotation.Z * Mathf.Radian );
					_cacheRotation = this.Rotation;
				}
				return _rotationCache;
			}
		}
		
		private Vector3 _cacheScale;
		private Matrix4 _scaleCache;
		private Matrix4 ScaleMatrix{
			get{
				if(this.Scale != _cacheScale){
					_scaleCache = Matrix4.CreateScale(Scale);
					_cacheScale = this.Scale;
				}
				return _scaleCache; 
			}
		}
		
		private Vector3 _cachePosition;
		private Matrix4 _positionCache;
		private Matrix4 PositionMatrix{
			get{
				if(this.Position != _cachePosition){
					_positionCache = Matrix4.CreateTranslation(Position);
					_cachePosition = this.Position;
				}
				return _positionCache;
			}
		}
	}
}
