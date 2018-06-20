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
using Hedra.Engine.EnvironmentSystem;
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
	    private readonly RenderableTexture _miniMapMarker;
	    private readonly FBO _mapFbo;
        private bool _show;
	    public bool HasMarker { get; private set; }
	    public Vector3 MarkedDirection { get; private set; }

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
            _miniMapMarker = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapQuest.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), DrawOrder.After);
            _panel.AddElement(_mapCursor);
            _panel.AddElement(_miniMap);
            _panel.AddElement(_miniMapRing);
            _panel.AddElement(_miniMapNorth);
            _panel.AddElement(_miniMapMarker);
            _panel.Disable();
        }

	    public void Mark(Vector3 Direction)
	    {
	        MarkedDirection = Direction;
	        HasMarker = true;
        }

	    public void Unmark()
	    {
	        HasMarker = false;
	    }
		
		public void DrawMap(){
		    GraphicsLayer.Enable(EnableCap.DepthTest);
		    GraphicsLayer.PushFBO();
		    GraphicsLayer.PushShader();
            _mapFbo.Bind();
            GL.ClearColor(SkyManager.FogManager.FogValues.U_BotColor.ToColor());
		    GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

		    var oldDistance = _player.View.Distance;
		    var oldPitch = _player.View.Pitch;
		    var oldFacing = _player.View.Yaw;

		    _player.View.Yaw = -1.55f;
		    _player.View.Distance = 100f;
		    _player.View.Pitch = -10f;
		    _player.View.BuildCameraMatrix();

		    var rotationMatrix =
		        Matrix4.CreateFromQuaternion(QuaternionMath.FromEuler(-_player.Model.Model.Rotation.Y * Vector3.UnitY * Mathf.Radian));
		    DrawManager.FrustumObject.SetFrustum(_player.View.ModelViewMatrix * rotationMatrix);

            GraphicsLayer.MatrixMode(MatrixMode.Projection);
		    var projMatrix = Matrix4.CreateOrthographic(1024, 1024, 1f, 2048);
		    GraphicsLayer.LoadMatrix(ref projMatrix);

		    var oldShadows = GameSettings.GlobalShadows;
		    var oldFancy = GameSettings.Fancy;
		    GameSettings.GlobalShadows = false;
		    GameSettings.Fancy = false;
            //DrawManager.FrustumObject.CalculateFrustum(projMatrix, DrawManager.FrustumObject.ModelViewMatrix);
            World.CullTest(DrawManager.FrustumObject);
            WorldRenderer.Render(World.DrawingChunks, ChunkBufferTypes.STATIC);
		    WorldRenderer.Render(World.DrawingChunks, ChunkBufferTypes.WATER);
		    GameSettings.Fancy = oldFancy;
		    GameSettings.GlobalShadows = oldShadows;

            _player.View.Pitch = oldPitch;
		    _player.View.Yaw = oldFacing;
		    _player.View.Distance = oldDistance;

		    _player.View.BuildCameraMatrix();
		    DrawManager.FrustumObject.SetFrustum(_player.View.ModelViewMatrix);

		    GraphicsLayer.Disable(EnableCap.DepthTest);
		    GraphicsLayer.Enable(EnableCap.Blend);
		    GL.ClearColor(Vector4.Zero.ToColor());
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
                _miniMapMarker.Disable();
                return;
            }

            if (!_miniMap.TextureElement.Enabled) return;
            _mapCursor.Enable();
            _miniMapRing.Enable();
            _miniMapNorth.Enable();
            _miniMapNorth.BaseTexture.TextureElement.Angle = -_player.Model.Model.Rotation.Y;

		    if (HasMarker)
		    {
		        Vector3 rot = Physics.DirectionToEuler(MarkedDirection);
		        _miniMapMarker.BaseTexture.TextureElement.Angle = -_player.Model.Model.Rotation.Y + rot.Y;
		        _miniMapMarker.Enable();
		    }
		    else
		    {
		        _miniMapMarker.Disable();
		    }

		    this.DrawMap();

            Shader.Bind();
            GraphicsLayer.Enable(EnableCap.Texture2D);
            GraphicsLayer.Enable(EnableCap.Blend);
            GraphicsLayer.Disable(EnableCap.DepthTest);

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

            GraphicsLayer.Enable(EnableCap.DepthTest);
            GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Disable(EnableCap.Texture2D);
            GraphicsLayer.Enable(EnableCap.CullFace);
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