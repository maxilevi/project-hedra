/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;

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
        public Joint RootJoint { get; }
        public int JointCount { get; }
        public Vector3 Position { get; set; }
        public bool Enabled { get; set; }
        public bool PrematureCulling { get; set; } = true;
        public bool ApplyFog { get; set; }
        public float Alpha { get; set; }
        public float DisposeTime { get; set; }
        public Vector4 Tint { get; set; }
        public Vector4 BaseTint { get; set; }
        public Vector3 Scale { get; set; }
        public Box CullingBox { get; set; }
        public bool Outline { get; set; }
        public bool UpdateWhenOutOfView { get; set; }
        public bool WasCulled { private get; set; }
        public Vector4 OutlineColor { get; set; }
        public Vector3 Max => CullingBox?.Max ?? Vector3.Zero;
        public Vector3 Min => CullingBox?.Min ?? Vector3.Zero;
        public Vector3[] JointIdsArray => _baseModelData.JointIds;
        public Vector3[] VerticesArray => _baseModelData.Vertices;
        private ModelData _baseModelData;
        private readonly List<ModelData> _addedModels;
        private readonly Matrix4x4[] _jointMatrices;
        private readonly Animator _animator;
        private readonly object _syncRoot;
        private Shader _shader = DefaultShader;
        private VAO<Vector3, Vector3, Vector3, Vector3, Vector3> Data { get; set; }
        private VBO<Vector3> _vertices, _normals, _jointIds, _vertexWeights;
        private VBO<Vector3> _colors;
        private VBO<uint> _indices;
        
        private bool _disposed;
        private Vector3 _rotation;
        private Vector3 _cacheRotation = -Vector3.One;
        private Matrix4x4 _rotationCache;
        private Vector3 _cacheScale;
        private Matrix4x4 _scaleCache;
        private Vector3 _cachePosition;
        private Matrix4x4 _positionCache;
        private Matrix4x4 _transformationMatrix = Matrix4x4.Identity;
        private bool _jointsDirty = true;
        private StackTrace _trace = new StackTrace();

        public AnimatedModel(ModelData Model, Joint RootJoint, int JointCount)
        {
            Model.AssertTriangulated();
            _baseModelData = Model;
            _addedModels = new List<ModelData>();
            Executer.ExecuteOnMainThread(delegate
            {
                _vertices = new VBO<Vector3>(Model.Vertices, Model.Vertices.Length * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                _colors = new VBO<Vector3>(Model.Colors, Model.Colors.Length * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                _normals = new VBO<Vector3>(Model.Normals, Model.Normals.Length * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                _indices = new VBO<uint>(Model.Indices, Model.Indices.Length * sizeof(uint),
                    VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
                _jointIds = new VBO<Vector3>(Model.JointIds, Model.JointIds.Length * HedraSize.Vector3,
                    VertexAttribPointerType.Float);
                _vertexWeights = new VBO<Vector3>(Model.VertexWeights,
                    Model.VertexWeights.Length * HedraSize.Vector3, VertexAttribPointerType.Float);
                Data = new VAO<Vector3, Vector3, Vector3, Vector3, Vector3>(_vertices, _colors, _normals,
                    _jointIds, _vertexWeights);
            });
            this.RootJoint = RootJoint;
            this.JointCount = JointCount;
            _animator = new Animator(this.RootJoint);
            _syncRoot = new object();
            this.RootJoint.CalculateInverseBindTransform(Matrix4x4.Identity);
            Alpha = 1.0f;
            Tint = Vector4.One;
            Scale = Vector3.One * 1.0f;
            ApplyFog = true;
            Enabled = true;
            _jointMatrices = new Matrix4x4[JointCount];
            DrawManager.Add(this);
        }

        public void AddModel(ModelData Model)
        {
            _addedModels.Add(Model);
            RebuildBuffers();
        }
        
        public void RemoveModel(ModelData Model)
        {
            _addedModels.Remove(Model);
            RebuildBuffers();
        }

        public void ClearModel()
        {
            _addedModels.Clear();
            _baseModelData = ModelData.Empty;
            RebuildBuffers();
        }
        
        private void RebuildBuffers()
        {
            var model = ModelData.Combine(_baseModelData, _addedModels.ToArray());
            Executer.ExecuteOnMainThread(delegate
            {
                if (_disposed) return;
                _vertices.Update(model.Vertices, model.Vertices.Length * HedraSize.Vector3);
                _colors.Update(model.Colors, model.Colors.Length * HedraSize.Vector4);
                _normals.Update(model.Normals, model.Normals.Length * HedraSize.Vector3);
                _indices.Update(model.Indices, model.Indices.Length * sizeof(uint));
                _jointIds.Update(model.JointIds, model.JointIds.Length * HedraSize.Vector3);
                _vertexWeights.Update(model.VertexWeights, model.VertexWeights.Length * HedraSize.Vector3);
            });
        }
        
        public void ReplaceColors(Vector3[] ColorArray)
        {
            Executer.ExecuteOnMainThread(delegate
            {
                if (ColorArray.Length != _colors.Count)
                    throw new ArgumentOutOfRangeException(
                        $"The new colors array can't have more data than the previous one.");

                _colors.Update(ColorArray, ColorArray.Length * HedraSize.Vector3);
            });
        }

        public void Dispose()
        {
            if (!BuffersCreated)
            {
                Executer.ExecuteOnMainThread(delegate
                {
                    _disposed = true;
                    Data.Dispose();
                    _jointIds.Dispose();
                    _indices.Dispose();
                    _normals.Dispose();
                    _colors.Dispose();
                    _vertices.Dispose();
                    _vertexWeights.Dispose();
                    DrawManager.Remove(this);
                });
            }
            else
            {
                _disposed = true;
                Data.Dispose();
                _jointIds.Dispose();
                _indices.Dispose();
                _normals.Dispose();
                _colors.Dispose();
                _vertices.Dispose();
                _vertexWeights.Dispose();
                DrawManager.Remove(this);
            }
        }

        public void Draw()
        {
            var viewMat = LocalPlayer.Instance.View.ModelViewMatrix;
            var projectionViewMat = viewMat * Culling.ProjectionMatrix;
            DrawModel(projectionViewMat, viewMat);
        }

        public void DrawModel(Matrix4x4 ProjectionViewMat, Matrix4x4 ViewMatrix)
        {
            if (!Enabled || _disposed || Data == null) return;
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);
            if (Alpha < 0.95f) Renderer.Enable(EnableCap.Blend);

            lock (_syncRoot)
            {
                DoDrawModel(ProjectionViewMat, ViewMatrix);
            }
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.Disable(EnableCap.Blend);
        }

        private void DoDrawModel(Matrix4x4 ProjectionViewMat, Matrix4x4 ViewMatrix)
        {
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
                Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureId[0]);
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

            _indices.Bind();
            Renderer.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            _indices.Unbind();
            
            if (Outline)
            {
                Renderer.Enable(EnableCap.Blend);
                Renderer.Disable(EnableCap.DepthTest);
                _shader["Outline"] = Outline ? 1 : 0;
                _shader["OutlineColor"] = OutlineColor;
                _shader["Time"] = Time.IndependentDeltaTime;
                _indices.Bind();
                Renderer.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                _indices.Unbind();
                Renderer.Enable(EnableCap.DepthTest);
            }
            _shader["Outline"] = 0;
            
            Data.Unbind();
            _shader.Unbind();
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

        public void ResetBlending()
        {
            _animator.ResetBlending();
        }

        public void Update()
        {
            if(!Enabled || (WasCulled && !UpdateWhenOutOfView)) return;
            if (_animator.Update())
            {
                UpdateJointTransforms(true);
            }
        }

        public void SetValues(AnimatedModel Model)
        {
            TransformationMatrix = Model.TransformationMatrix;
            Pause = Model.Pause;
            LocalRotation = Model.LocalRotation;
            Position = Model.Position;
            Scale = Model.Scale;
            Tint = Model.Tint;
            BaseTint = Model.BaseTint;
            Alpha = Model.Alpha;
            ApplyFog = Model.ApplyFog;
            AnimationSpeed = Model.AnimationSpeed;
            Outline = Model.Outline;
            Enabled = Model.Enabled;
        }

        public Matrix4x4[] JointTransforms => _jointMatrices;

        public void UpdateJointTransforms(bool Force = false)
        {
            if (Force || _jointsDirty)
            {
                _jointsDirty = false;
                AddJointsToArray(RootJoint, _jointMatrices);
            }
        }

        private void AddJointsToArray(Joint HeadJoint, Matrix4x4[] JointMatrices)
        {
            var scaleAndRotation = ScaleMatrix * RotationMatrix;
            var transformationAndPosition = TransformationMatrix * PositionMatrix;
            var queue = new Queue<Joint>();
            queue.Enqueue(HeadJoint);
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                var transform = current.AnimatedTransform == new Matrix4x4()
                    ? Matrix4x4.Identity
                    : current.AnimatedTransform;
                if(current.Index != -1)
                    JointMatrices[current.Index] = transform * scaleAndRotation * current.TransformationMatrix * transformationAndPosition;
                for (var i = 0; i < current.Children.Count; i++)
                {
                    queue.Enqueue(current.Children[i]);
                }
            }
        }

        public Matrix4x4 MatrixFromJoint(Joint TargetJoint)
        {
            UpdateJointTransforms();
            return JointTransforms[TargetJoint.Index];
        }

        public Vector3 TransformFromJoint(Vector3 Position, Joint TargetJoint)
        {
            var totalLocalPos = Vector3.Zero;
            var jointTransform = JointTransforms[TargetJoint.Index];
            var posePosition = Vector3.Transform(Position, jointTransform);
            totalLocalPos += posePosition * 1f; //WeightsArray[TargetJoint.Index][i];

            return totalLocalPos;
        }

        public Vector3 JointDefaultPosition(Joint TargetJoint)
        {
            var average = Vector3.Zero;
            float count = 0;
            for (var i = 0; i < _baseModelData.Vertices.Length; i++)
            {
                if ( (int) _baseModelData.JointIds[i].X == TargetJoint.Index 
                     || (int) _baseModelData.JointIds[i].Y == TargetJoint.Index 
                     || (int) _baseModelData.JointIds[i].Z == TargetJoint.Index)
                {
                    average += _baseModelData.Vertices[i];
                    count++;
                }
            }

            average /= count;
            return average;
        }

        public void SwitchShader(Shader NewShader)
        {
            lock(_syncRoot)
                _shader = NewShader;
        }

        private bool BuffersCreated => Data != null;

        public Matrix4x4 TransformationMatrix
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

        public Vector3 LocalRotation
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

        private Matrix4x4 RotationMatrix
        {
            get
            {
                if (LocalRotation == _cacheRotation) return _rotationCache;
                _rotationCache = Matrix4x4.CreateRotationX(LocalRotation.X * Mathf.Radian) * 
                        Matrix4x4.CreateRotationY(LocalRotation.Y * Mathf.Radian) *
                        Matrix4x4.CreateRotationZ(LocalRotation.Z * Mathf.Radian);
                _cacheRotation = LocalRotation;
                _jointsDirty = true;
                return _rotationCache;
            }
        }

        private Matrix4x4 ScaleMatrix
        {
            get
            {
                if (Scale == _cacheScale) return _scaleCache;
                _scaleCache = Matrix4x4.CreateScale(Scale);
                _cacheScale = Scale;
                _jointsDirty = true;
                return _scaleCache;
            }
        }

        private Matrix4x4 PositionMatrix
        {
            get
            {
                if (Position == _cachePosition) return _positionCache;
                _positionCache = Matrix4x4.CreateTranslation(Position);
                _cachePosition = Position;
                _jointsDirty = true;
                return _positionCache;
            }
        }
    }
}