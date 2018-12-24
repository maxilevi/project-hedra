
using System;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryItemRenderer
    {
        public const float ZOffsetFactor = 1.25f * 5f;
        public static readonly FBO Framebuffer;
        private readonly InventoryArray _array;
        private readonly int _length;
        private readonly int _offset;
        private readonly ObjectMesh[] _models;
        private readonly float[] _modelsHeights;
        private float _itemRotation;
        private float _itemCount;

        static InventoryItemRenderer()
        {
            Framebuffer = new FBO(GameSettings.Width, GameSettings.Height);
        }

        public InventoryItemRenderer(InventoryArray Array, int Offset, int Length)
        {
            this._array = Array;
            this._length = Length;
            this._offset = Offset;
            this._models = new ObjectMesh[_length];
            this._modelsHeights = new float[_length];
        }

        public void UpdateView()
        {
            var itemCount = 0;
            for (var i = 0; i < _length; i++)
            {
                if (_models[i] != null)
                {
                    _models[i].Dispose();
                }
                if (_array[i+ _offset] != null)
                {
                    _models[i] = this.BuildModel(_array[i + _offset], out _modelsHeights[i]);
                    itemCount++;
                }
            }
            _itemCount = itemCount;
        }

        public ObjectMesh BuildModel(Item Item, out float ModelHeight)
        {
            var model = CenterModel(Item.Model.Clone());
            ModelHeight = model.SupportPoint(Vector3.UnitY).Y - model.SupportPoint(-Vector3.UnitY).Y;
            var mesh = ObjectMesh.FromVertexData(model);
            mesh.BaseTint = EffectDescriber.EffectColorFromItem(Item);
            mesh.ApplyFog = false;
            DrawManager.Remove(mesh);
            return mesh;
        }

        public uint Draw(int Id)
        {
            return Draw(_models[Id], _array[Id + _offset], true, _modelsHeights[Id] * InventoryItemRenderer.ZOffsetFactor);
        }

        public uint Draw(ObjectMesh Mesh, Item Item, bool TiltIfWeapon = true, float ZOffset = 3.0f)
        {
            ZOffset = Math.Max(ZOffset, 3.0f);
            if (Mesh == null || Item == null) return GUIRenderer.TransparentTexture;
            var willTilt = TiltIfWeapon && Item.IsWeapon;
            Mesh.Rotation = new Vector3(0, _itemRotation, willTilt ? 45 : 0);
            _itemRotation += 25 * Time.DeltaTime / Math.Max(1,_itemCount);

            Renderer.PushShader();
            Renderer.PushFBO();
            Framebuffer.Bind();

            var meshPrematureCulling = Mesh.PrematureCulling;
            Mesh.PrematureCulling = false;

            var currentDayColor = ShaderManager.LightColor;
            ShaderManager.SetLightColorInTheSameThread(Vector3.One);

            var aspect = 1.33f;
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(10 * Mathf.Radian, aspect, 1, 1024f);
            Renderer.LoadProjection(projectionMatrix);

            var lookAt = Matrix4.LookAt(Vector3.UnitZ * ZOffset * (willTilt ? .75f : 1f), Vector3.Zero, Vector3.UnitY);
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
            Renderer.PopFBO();
            Renderer.PopShader();
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, Renderer.FBOBound);
            Renderer.BindShader(Renderer.ShaderBound);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);
            Mesh.PrematureCulling = meshPrematureCulling;
            return Framebuffer.TextureID[0];
        }

        private static VertexData CenterModel(VertexData Model)
        {
            Model.Center();
            return Model;
        }
    }
}
