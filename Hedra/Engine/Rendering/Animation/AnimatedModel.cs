/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    /// Description of AnimatedModel.
    /// </summary>
    public class AnimatedModel : IDisposable, IRenderable, ICullableModel
    {
        public static readonly Shader DefaultShader =
            Shader.Build("Shaders/AnimatedModel.vert", "Shaders/AnimatedModel.frag");
        public static readonly Shader DeathShader = AnimatedModelShader.GenerateDeathShader();
        
        public VAO<Vector3, Vector3, Vector3, Vector3, Vector3> Data { get; private set; }
        public Vector3[] WeightsArray { get; }
        public Vector3[] JointIdsArray { get; }
        public Vector3[] VerticesArray { get; }
        public Joint RootJoint { get; }
        public int JointCount { get; }
        public Vector3 Position { get; set; }
        public bool Enabled { get; set; }
        public bool ApplyFog { get; set; }
        public float Alpha { get; set; }
        public float DisposeTime { get; set; }
        public Vector4 Tint { get; set; }
        public Vector4 BaseTint { get; set; }
        public Vector3 Scale { get; set; }
        public Box CullingBox { get; set; }
        private readonly Matrix4[] _jointMatrices;
        private readonly Animator _animator;
        private Shader _shader = DefaultShader;
        private VBO<Vector3> _vertices, _normals, _jointIds, _vertexWeights;
        private VBO<Vector3> _colors;
        private VBO<uint> _indices;
        private bool _disposed;
        private Vector3 _rotation;
        private Vector3 _cacheRotation = -Vector3.One;
        private Matrix4 _rotationCache;
        private Vector3 _cacheScale;
        private Matrix4 _scaleCache;
        private Vector3 _cachePosition;
        private Matrix4 _positionCache;
        private Matrix4 _transformationMatrix = Matrix4.Identity;
        private bool _jointsDirty = true;

        public AnimatedModel(ModelData Data, Joint RootJoint, int JointCount)
        {
            WeightsArray = Data.VertexWeights;
            JointIdsArray = Data.JointIds;
            VerticesArray = Data.Vertices;

            Executer.ExecuteOnMainThread(delegate
            {
                _vertices = new VBO<Vector3>(Data.Vertices, Data.Vertices.Length * Vector3.SizeInBytes,
                    VertexAttribPointerType.Float);
                _colors = new VBO<Vector3>(Data.Colors, Data.Colors.Length * Vector3.SizeInBytes,
                    VertexAttribPointerType.Float);
                _normals = new VBO<Vector3>(Data.Normals, Data.Normals.Length * Vector3.SizeInBytes,
                    VertexAttribPointerType.Float);
                _indices = new VBO<uint>(Data.Indices, Data.Indices.Length * sizeof(uint),
                    VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
                _jointIds = new VBO<Vector3>(Data.JointIds, Data.JointIds.Length * Vector3.SizeInBytes,
                    VertexAttribPointerType.Float);
                _vertexWeights = new VBO<Vector3>(Data.VertexWeights,
                    Data.VertexWeights.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);

                this.Data = new VAO<Vector3, Vector3, Vector3, Vector3, Vector3>(_vertices, _colors, _normals,
                    _jointIds, _vertexWeights);
            });
            this.RootJoint = RootJoint;
            this.JointCount = JointCount;
            _animator = new Animator(this.RootJoint);
            this.RootJoint.CalculateInverseBindTransform(Matrix4.Identity);
            Alpha = 1.0f;
            Tint = Vector4.One;
            Scale = Vector3.One * 1.0f;
            ApplyFog = true;
            Enabled = true;
            _jointMatrices = new Matrix4[JointCount];
            DrawManager.Add(this);
        }

        public void Dispose()
        {
            if (!BuffersCreated)
            {
                Executer.ExecuteOnMainThread(delegate
                {
                    _disposed = true;
                    Data?.Dispose();
                    _jointIds?.Dispose();
                    _indices?.Dispose();
                    _normals?.Dispose();
                    _colors?.Dispose();
                    _vertices?.Dispose();
                    _vertexWeights?.Dispose();
                    DrawManager.Remove(this);
                });
            }
            else
            {
                _disposed = true;
                Data?.Dispose();
                _jointIds?.Dispose();
                _indices?.Dispose();
                _normals?.Dispose();
                _colors?.Dispose();
                _vertices?.Dispose();
                _vertexWeights?.Dispose();
                DrawManager.Remove(this);
            }
        }

        public void Draw()
        {
            var viewMat = LocalPlayer.Instance.View.ModelViewMatrix;
            var projectionViewMat = viewMat * DrawManager.FrustumObject.ProjectionMatrix;
            DrawModel(projectionViewMat, viewMat);
        }

        private void DrawModel(Matrix4 ProjectionViewMat, Matrix4 ViewMatrix)
        {
            if (!Enabled || _disposed || Data == null) return;
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);
            if (Alpha < 0.95f) Renderer.Enable(EnableCap.Blend);

            _shader.Bind();

            if (_shader == DeathShader && CompatibilityManager.SupportsGeometryShaders)
            {
                Renderer.Enable(EnableCap.Blend);
                DeathShader["viewMatrix"] = ViewMatrix;
                DeathShader["disposeTime"] = DisposeTime;
            }

            _shader["jointTransforms"] = JointTransforms;
            _shader["projectionViewMatrix"] = ProjectionViewMat;
            if (_shader == DefaultShader || !CompatibilityManager.SupportsGeometryShaders)
            {
                _shader["PlayerPosition"] = GameManager.Player.Position;
            }

            if (GameSettings.Shadows)
            {
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
                _shader["ShadowTex"] = 0;
                if (_shader == DefaultShader || !CompatibilityManager.SupportsGeometryShaders)
                {
                    _shader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
                }
            }

            _shader["UseShadows"] = GameSettings.Shadows ? 1.0f : 0.0f;
            _shader["UseFog"] = ApplyFog ? 1 : 0;
            _shader["Alpha"] = Alpha;
            _shader["Tint"] = Tint + BaseTint;

            Data.Bind();

            Renderer.BindBuffer(BufferTarget.ElementArrayBuffer, _indices.ID);
            Renderer.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Data.Unbind();

            _shader.Unbind();

            Renderer.Disable(EnableCap.Blend);
        }

        public void PlayAnimation(Animation Animation)
        {
            _animator.PlayAnimation(Animation);
        }

        public void BlendAnimation(Animation ToBlend)
        {
            _animator.BlendAnimation(ToBlend);
        }
        
        public void Reset()
        {
            _animator.Reset();
        }

        public void Update()
        {
            if (_animator.Update())
            {
                UpdateJointTransforms(true);
            }
        }

        public Matrix4[] JointTransforms => _jointMatrices;

        private void UpdateJointTransforms(bool Force = false)
        {
            if (Force || _jointsDirty)
            {
                _jointsDirty = false;
                AddJointsToArray(RootJoint, _jointMatrices);
            }
        }

        private void AddJointsToArray(Joint HeadJoint, Matrix4[] JointMatrices)
        {
            JointMatrices[HeadJoint.Index] = HeadJoint.AnimatedTransform * ScaleMatrix * RotationMatrix *
                                             HeadJoint.TransformationMatrix * TransformationMatrix * PositionMatrix;
            for (var i = 0; i < HeadJoint.Children.Count; i++)
            {
                AddJointsToArray(HeadJoint.Children[i], JointMatrices);
            }
        }

        public Matrix4 MatrixFromJoint(Joint TargetJoint)
        {
            UpdateJointTransforms();
            return JointTransforms[TargetJoint.Index];
        }

        public Vector3 TransformFromJoint(Vector3 Position, Joint TargetJoint)
        {
            var totalLocalPos = Vector3.Zero;
            var jointTransform = JointTransforms[TargetJoint.Index];
            var posePosition = Vector3.TransformPosition(Position, jointTransform);
            totalLocalPos += posePosition * 1f; //WeightsArray[TargetJoint.Index][i];

            return totalLocalPos;
        }

        public Vector3 JointDefaultPosition(Joint TargetJoint)
        {
            var average = Vector3.Zero;
            float count = 0;
            for (var i = 0; i < VerticesArray.Length; i++)
            {
                if ( (int) JointIdsArray[i].X == TargetJoint.Index || (int) JointIdsArray[i].Y == TargetJoint.Index ||
                     (int) JointIdsArray[i].Z == TargetJoint.Index)
                {
                    average += VerticesArray[i];
                    count++;
                }
            }

            average /= count;
            return average;
        }

        public void SwitchShader(Shader NewShader)
        {
            _shader = NewShader;
        }

        public void ReplaceColors(Vector3[] ColorArray)
        {
            Executer.ExecuteOnMainThread(delegate
            {
                if (ColorArray.Length != _colors.Count)
                    throw new ArgumentOutOfRangeException(
                        $"The new colors array can't have more data than the previous one.");


                _colors.Update(ColorArray, ColorArray.Length * Vector3.SizeInBytes);
            });
        }

        private bool BuffersCreated => Data != null;

        public Matrix4 TransformationMatrix
        {
            get => _transformationMatrix;
            set
            {
                if (_transformationMatrix != value)
                {
                    _jointsDirty = true;
                    _transformationMatrix = value;
                }
            }
        }

        public Animation AnimationPlaying => _animator.AnimationPlaying;
        
        public Animation AnimationBlending => _animator.BlendingAnimation;
        
        public float AnimationSpeed
        {
            get => _animator.AnimationSpeed;
            set => _animator.AnimationSpeed = value;
        }

        public bool Pause
        {
            get => _animator.Stop;
            set => _animator.Stop = value;
        }

        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                var newValue = value;
                if (float.IsNaN(value.X))
                    newValue = new Vector3(0, newValue.Y, newValue.Z);
                if (float.IsNaN(value.Y))
                    newValue = new Vector3(newValue.X, 0, newValue.Z);
                if (float.IsNaN(value.Z))
                    newValue = new Vector3(newValue.X, newValue.Y, 0);

                _rotation = newValue;
            }
        }

        private Matrix4 RotationMatrix
        {
            get
            {
                if (Rotation == _cacheRotation) return _rotationCache;
                _rotationCache = Matrix4.CreateRotationX(Rotation.X * Mathf.Radian) *
                                 Matrix4.CreateRotationY(Rotation.Y * Mathf.Radian) *
                                 Matrix4.CreateRotationZ(Rotation.Z * Mathf.Radian);
                _cacheRotation = Rotation;
                _jointsDirty = true;
                return _rotationCache;
            }
        }

        private Matrix4 ScaleMatrix
        {
            get
            {
                if (Scale == _cacheScale) return _scaleCache;
                _scaleCache = Matrix4.CreateScale(Scale);
                _cacheScale = Scale;
                _jointsDirty = true;
                return _scaleCache;
            }
        }

        private Matrix4 PositionMatrix
        {
            get
            {
                if (Position == _cachePosition) return _positionCache;
                _positionCache = Matrix4.CreateTranslation(Position);
                _cachePosition = Position;
                _jointsDirty = true;
                return _positionCache;
            }
        }
    }
}