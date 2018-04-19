/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/02/2017
 * Time: 03:29 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player.MapSystem
{
	/// <summary>
	/// Description of Minimap.
	/// </summary>
	public class Minimap : IRenderable
	{
	    private static readonly Shader Shader = Shader.Build("Shaders/GUI.vert", "Shaders/MinimapGUI.frag");
        private readonly LocalPlayer _player;
		private readonly Panel _panel;
	    private readonly Texture _miniMap;
	    private readonly RenderableTexture _mapCursor;
	    private readonly RenderableTexture _miniMapRing;
	    private readonly RenderableTexture _miniMapNorth;
	    private readonly RenderableTexture _miniMapQ;
	    private readonly FBO _mapFbo;
        private bool _show;

        public Minimap(LocalPlayer Player){
            this._player = Player;
            this._panel = new Panel();
            _mapFbo = new FBO(GameSettings.Width, GameSettings.Height);
            _miniMap = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMap.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * new Vector2(1f, 1f));
            DrawManager.UIRenderer.Remove(_miniMap.TextureElement);
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
            _mapCursor = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapCursor.png"), new Vector2(.8f, .7f), new Vector2(0.008f, 0.019f) * 1.3f), DrawOrder.After);
            _miniMapRing = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapRing.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), DrawOrder.After);
            _miniMapNorth = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapNorth.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), DrawOrder.After);
            _miniMapQ = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapQuest.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), DrawOrder.After);
            _panel.AddElement(_mapCursor);
            _panel.AddElement(_miniMap);
            _panel.AddElement(_miniMapRing);
            _panel.AddElement(_miniMapNorth);
            _panel.AddElement(_miniMapQ);
            _panel.Disable();
        }
		
		public void DrawMap(){
		    GL.Enable(EnableCap.DepthTest);
		    GraphicsLayer.PushFBO();
		    GraphicsLayer.PushShader();
            _mapFbo.Bind();
		    GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

		    var oldDistance = _player.View.Distance;
		    var oldPitch = _player.View.Pitch;
		    var oldFacing = _player.View.Yaw;

		    _player.View.Yaw = -1.55f;
		    _player.View.Distance = 100f;
		    _player.View.Pitch = -10f;
		    _player.View.RebuildMatrix();

		    var rotationMatrix =
		        Matrix4.CreateFromQuaternion(QuaternionMath.ToQuaternion(-_player.Model.Model.Rotation.Y * Vector3.UnitY * Mathf.Radian));
		    DrawManager.FrustumObject.SetFrustum(_player.View.Matrix * rotationMatrix);

            GraphicsLayer.MatrixMode(MatrixMode.Projection);
		    var projMatrix = Matrix4.CreateOrthographic(1024, 1024, 1f, 2048);
		    GraphicsLayer.LoadMatrix(ref projMatrix);

		    var oldShadows = GameSettings.GlobalShadows;
		    GameSettings.GlobalShadows = false;
            DrawManager.FrustumObject.CalculateFrustum(projMatrix, DrawManager.FrustumObject.ModelViewMatrix);
            World.CullTest(DrawManager.FrustumObject);
            WorldRenderer.Render(World.DrawingChunks, ChunkBufferTypes.STATIC);
		    WorldRenderer.Render(World.DrawingChunks, ChunkBufferTypes.WATER);
		    GameSettings.GlobalShadows = oldShadows;

		    _player.View.Pitch = oldPitch;
		    _player.View.Yaw = oldFacing;
		    _player.View.Distance = oldDistance;

		    _player.View.RebuildMatrix();
		    DrawManager.FrustumObject.SetFrustum(_player.View.Matrix);

		    GL.Disable(EnableCap.DepthTest);
		    GL.Enable(EnableCap.Blend);
		    GraphicsLayer.PopFBO();
		    GraphicsLayer.PopShader();
		    GraphicsLayer.BindFramebuffer(FramebufferTarget.Framebuffer, GraphicsLayer.FBOBound);
		    GraphicsLayer.BindShader(GraphicsLayer.ShaderBound);
        }

		public void Draw(){
            if (!GameSettings.ShowMinimap)
            {
                _mapCursor.Disable();
                _miniMapRing.Disable();
                _miniMapNorth.Disable();
                _miniMapQ.Disable();
                return;
            }

            if (!_miniMap.TextureElement.Enabled) return;
            _mapCursor.Enable();
            _miniMapRing.Enable();
            _miniMapNorth.Enable();
            _miniMapNorth.BaseTexture.TextureElement.Angle = _player.Model.Model.Rotation.Y;

            var inverted = false;
            Vector3 toObjDirection = (_player.Model.Model.Position - World.QuestManager.Quest.IconPosition).NormalizedFast().Xz.ToVector3();
            Vector3 rot = Physics.DirectionToEuler(toObjDirection);
            _miniMapQ.BaseTexture.TextureElement.Angle = _player.Model.Model.Rotation.Y - rot.Y + 180;

            this.DrawMap();

            Shader.Bind();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _mapFbo.TextureID[0]);
            Shader["Fill"] = 1;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _miniMap.TextureElement.TextureId);
            Shader["Texture"] = 0;

            Shader["Scale"] = _miniMap.TextureElement.Scale;
            Shader["Position"] = _miniMap.TextureElement.Position;
            Shader["Flipped"] = _miniMap.TextureElement.IdPointer == null && !_miniMap.TextureElement.Flipped ? 0 : 1;
            Shader["Opacity"] = _miniMap.TextureElement.Opacity;
            Shader["Grayscale"] = _miniMap.TextureElement.Grayscale ? 1 : 0;
            Shader["Tint"] = _miniMap.TextureElement.Tint;

            DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            Shader.UnBind();
        }
		
		public bool Show{
			get{ return _show; }
			set{
				_show = value;
				if(_show) _panel.Enable();
				else _panel.Disable();
			}
		}
	}
}