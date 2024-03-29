using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryItemRenderer
    {
        public const float ZOffsetFactor = 5f;
        public static readonly FBO Framebuffer;
        private static float _itemRotation;
        private readonly int _length;
        private readonly VertexData[] _modelCache;
        private readonly ObjectMesh[] _models;
        private readonly Vector3[] _modelsSize;
        private readonly int _offset;
        private InventoryArray _array;

        static InventoryItemRenderer()
        {
            Framebuffer = new FBO(256, 256);
        }

        public InventoryItemRenderer(InventoryArray Array, int Offset, int Length)
        {
            _array = Array;
            _length = Length;
            _offset = Offset;
            _models = new ObjectMesh[_length];
            _modelsSize = new Vector3[_length];
            _modelCache = new VertexData[_length];
        }

        public void SetArray(InventoryArray New)
        {
            _array = New;
        }

        public void UpdateView()
        {
            var itemCount = 0;
            for (var i = 0; i < _length; i++)
            {
                if (_array[i + _offset] != null && _array[i + _offset].Model == _modelCache[i]) continue;
                if (_models[i] != null)
                {
                    _models[i].Dispose();
                    _models[i] = null;
                    _modelCache[i] = null;
                }

                if (_array[i + _offset] != null)
                {
                    _models[i] = BuildModel(_array[i + _offset].Model, out _modelsSize[i]);
                    _modelCache[i] = _array[i + _offset].Model;
                    itemCount++;
                }
            }
        }

        public static ObjectMesh BuildModel(VertexData Model, out Vector3 ModelSizes)
        {
            var model = CenterModel(Model.Clone());
            ModelSizes = new Vector3(
                model.SupportPoint(Vector3.UnitX).X - model.SupportPoint(-Vector3.UnitX).X,
                model.SupportPoint(Vector3.UnitY).Y - model.SupportPoint(-Vector3.UnitY).Y,
                model.SupportPoint(Vector3.UnitZ).Z - model.SupportPoint(-Vector3.UnitZ).Z
            );
            var mesh = ObjectMesh.FromVertexData(model);
            //mesh.BaseTint = EffectDescriber.EffectColorFromItem(Item);
            mesh.ApplyFog = false;
            DrawManager.RemoveObjectMesh(mesh);
            return mesh;
        }

        public uint Draw(int Id)
        {
            return Draw(_models[Id], _array[Id + _offset], true, _modelsSize[Id]);
        }

        public static uint Draw(ObjectMesh Mesh, bool IsWeapon, bool TiltIfWeapon, Vector3 Size)
        {
            var zoffset = ZOffsetFactor * Size.Length();
            return Draw(Mesh, IsWeapon, TiltIfWeapon, zoffset);
        }

        public static uint Draw(ObjectMesh Mesh, Item Item, bool TiltIfWeapon, Vector3 Size)
        {
            if (Item == null) return GUIRenderer.TransparentTexture;
            return Draw(Mesh, Item.IsWeapon, TiltIfWeapon, Size);
        }

        private static uint Draw(ObjectMesh Mesh, bool IsWeapon, bool TiltIfWeapon = true, float ZOffset = 3.0f)
        {
            ZOffset = Math.Max(ZOffset, 3.0f);
            if (Mesh == null) return GUIRenderer.TransparentTexture;
            var willTilt = TiltIfWeapon && IsWeapon;
            Mesh.Rotation = new Vector3(0, _itemRotation, willTilt ? 45 : 0);

            var previousBound = Renderer.ShaderBound;
            var previousFBO = Renderer.FBOBound;
            Framebuffer.Bind();

            var meshPrematureCulling = Mesh.PrematureCulling;
            Mesh.PrematureCulling = false;

            var previousShadows = GameSettings.GlobalShadows;
            GameSettings.GlobalShadows = false;

            var currentDayColor = ShaderManager.LightColor;
            ShaderManager.SetLightColorInTheSameThread(Vector3.One);

            var aspect = 1.33f;
            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(10 * Mathf.Radian, aspect, 1, 1024f);
            Renderer.LoadProjection(projectionMatrix);

            var lookAt = Matrix4x4.CreateLookAt(Vector3.UnitZ * ZOffset * (willTilt ? .75f : 1f), Vector3.Zero,
                Vector3.UnitY);
            Renderer.LoadModelView(lookAt);

            Renderer.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            Mesh.Draw();

            /*
            Renderer.BindFramebuffer(FramebufferTarget.DrawFramebuffer, ItemsFBO.BufferID);
            Renderer.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MultisampleItemsFBO.BufferID);
            Renderer.BlitFramebuffer(0,GameSettings.Height,GameSettings.Width,0, 0,GameSettings.Height,GameSettings.Width,0
                               , ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            
            Renderer.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            Renderer.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
          */
            ShaderManager.SetLightColorInTheSameThread(currentDayColor);
            GameSettings.GlobalShadows = previousShadows;
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
            Renderer.Viewport(0, 0, GameSettings.DeviceWidth, GameSettings.DeviceHeight);
            Renderer.BindShader(previousBound);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);
            Mesh.PrematureCulling = meshPrematureCulling;
            return Framebuffer.TextureId[0];
        }

        public static void Update()
        {
            _itemRotation += 25 * Time.DeltaTime;
        }

        private static VertexData CenterModel(VertexData Model)
        {
            Model.Center();
            return Model;
        }

        public static void Dispose()
        {
            Framebuffer.Dispose();
        }
    }
}