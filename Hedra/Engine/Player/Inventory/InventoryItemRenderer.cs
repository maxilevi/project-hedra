
using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryItemRenderer
    {
        public const float ZOffsetFactor = 1.15f;
        private readonly InventoryArray _array;
        private readonly int _length;
        private readonly int _offset;
        private readonly EntityMesh[] _models;
        private readonly float[] _modelsHeights;
        private float _itemRotation;
        private float _itemCount;

        public InventoryItemRenderer(InventoryArray Array, int Offset, int Length)
        {
            this._array = Array;
            this._length = Length;
            this._offset = Offset;
            this._models = new EntityMesh[_length];
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
                    var model = _array[i + _offset].Model;
                    _modelsHeights[i] = model.SupportPoint(Vector3.UnitY).Y - model.SupportPoint(-Vector3.UnitY).Y;
                    _models[i] = EntityMesh.FromVertexData(model);
                    _models[i].UseFog = false;
                    DrawManager.Remove(_models[i]);
                    itemCount++;
                }
            }
            _itemCount = itemCount;
        }

        public uint Draw(int Id)
        {
            return Draw(_models[Id], _array[Id + _offset], true, _modelsHeights[Id] * InventoryItemRenderer.ZOffsetFactor);
        }

        public uint Draw(EntityMesh Mesh, Item Item, bool TiltIfWeapon = true, float ZOffset = 3.0f)
        {
            ZOffset = Math.Max(ZOffset, 3.0f);
            if (Mesh == null || Item == null) return GUIRenderer.TransparentTexture;

            Mesh.AnimationRotation = new Vector3(0, _itemRotation, TiltIfWeapon && Item.IsWeapon ? 45 : 0);
            _itemRotation += 25 * (float)Time.deltaTime / Math.Max(1,_itemCount);

            GraphicsLayer.PushShader();
            GraphicsLayer.PushFBO();
            UserInterface.InventoryFbo.Bind();

            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(50 * Mathf.Radian, 1.33f, 1, 1024f);
            GraphicsLayer.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);

            var offset = Item.IsWeapon
                ? Vector3.UnitY * 0.4f - Vector3.UnitX * 0.4f
                : Vector3.UnitY * 0.25f;
            var lookAt = Matrix4.LookAt(Vector3.UnitZ * ZOffset, offset, Vector3.UnitY);
            GraphicsLayer.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookAt);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            Mesh.Draw();

            /*GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, ItemsFBO.BufferID);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MultisampleItemsFBO.BufferID);
			GL.BlitFramebuffer(0,GameSettings.Height,GameSettings.Width,0, 0,GameSettings.Height,GameSettings.Width,0
			                   , ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
			
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);*/

            GraphicsLayer.PopFBO();
            GraphicsLayer.PopShader();
            GraphicsLayer.BindFramebuffer(FramebufferTarget.Framebuffer, GraphicsLayer.FBOBound);
            GraphicsLayer.BindShader(GraphicsLayer.ShaderBound);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            return UserInterface.InventoryFbo.TextureID[0];
        }
    }
}
