/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/02/2017
 * Time: 03:29 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Player.MapSystem
{
    /// <summary>
    /// Description of Minimap.
    /// </summary>
    public class Minimap : IRenderable, IAdjustable
    {
        private static readonly Shader Shader = Shader.Build("Shaders/GUI.vert", "Shaders/MinimapGUI.frag");
        private readonly LocalPlayer _player;
        private readonly Panel _panel;
        private readonly Texture _miniMap;
        private readonly RenderableTexture _mapCursor;
        private readonly RenderableTexture _miniMapRing;
        private readonly RenderableTexture _miniMapNorth;
        private readonly RenderableTexture _miniMapMarker;
        private readonly RenderableTexture _miniMapQuestMarker;
        private readonly FBO _mapFbo;
        private bool _show;
        public bool HasMarker { get; private set; }
        public bool HasQuestMarker { get; private set; }
        public Vector3 MarkedQuestPosition { get; private set; }
        public Vector3 MarkedDirection { get; private set; }
        private Vector3 _lastPosition;
        private int _previousActiveChunks;

        public Minimap(LocalPlayer Player)
        {
            this._player = Player;
            this._panel = new Panel();
            const float mapScale = .75f;
            _mapFbo = new FBO(256, 256);
            _miniMap = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMap.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMap.png").As1920x1080() * mapScale);
            DrawManager.UIRenderer.Remove(_miniMap.TextureElement);
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
            _mapCursor = new RenderableTexture(
                new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapCursor.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMap.png").As1920x1080() * .1f * mapScale),
                DrawOrder.After
            );
            _miniMapRing = new RenderableTexture(
                new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapRing.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMapRing.png").As1920x1080() * mapScale),
                DrawOrder.After
            );
            _miniMapNorth = new RenderableTexture(
                new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapNorth.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMapNorth.png").As1920x1080() * mapScale),
                DrawOrder.After
            );
            _miniMapMarker = new RenderableTexture(
                new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapMarker.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMapMarker.png").As1920x1080() * mapScale),
                DrawOrder.After
            );
            _miniMapQuestMarker = new RenderableTexture(
                new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapQuest.png"), new Vector2(.8f, .75f), Graphics2D.SizeFromAssets("Assets/UI/MiniMapQuest.png").As1920x1080() * mapScale),
                DrawOrder.After
            );
            var mapTranslation = Translation.Create("map_label");
            mapTranslation.Concat(() => $" - {Controls.Map}");
            var mapMsg = new GUIText(mapTranslation, _miniMapRing.Position - _miniMapRing.Scale.Y * Vector2.UnitY * 1.5f, Color.FromArgb(200, 255, 255, 255), FontCache.Get(AssetManager.BoldFamily, 14));
            Controls.OnControlsChanged += () => mapTranslation.UpdateTranslation();
            
            _panel.AddElement(mapMsg);
            _panel.AddElement(_mapCursor);
            _panel.AddElement(_miniMap);
            _panel.AddElement(_miniMapRing);
            _panel.AddElement(_miniMapNorth);
            _panel.AddElement(_miniMapMarker);
            _panel.AddElement(_miniMapQuestMarker);
            _panel.Disable();
        }

        public void Adjust()
        {
            _miniMap.TextureElement.Adjust();
        }
        
        public void Mark(Vector3 Direction)
        {
            MarkedDirection = Direction;
            HasMarker = true;
        }
        
        public void MarkQuest(Vector3 Position)
        {
            MarkedQuestPosition = Position;
            HasQuestMarker = true;
        }

        public void UnMark()
        {
            MarkedDirection = Vector3.Zero;
            HasMarker = false;
        }
        
        public void UnMarkQuest()
        {
            HasQuestMarker = false;
        }

        public void Reset()
        {
            UnMarkQuest();
            UnMark();
        }

        private void DrawMap()
        {
            if ((_lastPosition - _player.Position.Xz.ToVector3()).LengthSquared > 2 || _previousActiveChunks != _player.Loader.ActiveChunks)
            {
                Renderer.Enable(EnableCap.DepthTest);
                Renderer.PushFBO();
                Renderer.PushShader();
                _mapFbo.Bind();
                Renderer.ClearColor(SkyManager.FogManager.FogValues.U_BotColor);
                Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                var oldDistance = _player.View.Distance;
                var oldPitch = _player.View.Pitch;
                var oldFacing = _player.View.Yaw;
                var oldHeight = _player.View.CameraHeight;

                _player.View.Yaw = -1.55f;
                _player.View.Distance = 100f;
                _player.View.Pitch = -10f;
                _player.View.CameraHeight = Vector3.UnitY * (800 - _player.Position.Y);
                _player.View.BuildCameraMatrix();

                Culling.BuildFrustum(_player.View.ModelViewMatrix);

                var projMatrix = Matrix4.CreateOrthographic(1024, 1024, 1f, 2048);
                var trans = Culling.ModelViewMatrix.ExtractTranslation();
                Culling.BuildFrustum(projMatrix, Culling.ModelViewMatrix);

                var oldShadows = GameSettings.GlobalShadows;
                var oldFancy = GameSettings.Quality;
                var oldCulling = GameSettings.OcclusionCulling;
                GameSettings.OcclusionCulling = false;
                GameSettings.GlobalShadows = false;
                GameSettings.Quality = false;
                World.CullTest();
                WorldRenderer.Render(World.DrawingChunks, World.ShadowDrawingChunks, WorldRenderType.Static);
                WorldRenderer.Render(World.DrawingChunks, World.ShadowDrawingChunks, WorldRenderType.Water);
                GameSettings.Quality = oldFancy;
                GameSettings.GlobalShadows = oldShadows;
                GameSettings.OcclusionCulling = oldCulling;

                _player.View.Pitch = oldPitch;
                _player.View.Yaw = oldFacing;
                _player.View.Distance = oldDistance;
                _player.View.CameraHeight = oldHeight;

                _player.View.BuildCameraMatrix();
                Culling.BuildFrustum(_player.View.ModelViewMatrix);

                Renderer.Disable(EnableCap.DepthTest);
                Renderer.Enable(EnableCap.Blend);
                Renderer.ClearColor(Vector4.Zero);
                Renderer.PopFBO();
                Renderer.PopShader();
                Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, Renderer.FBOBound);
                Renderer.Viewport(0, 0, GameSettings.Width, GameSettings.Height);
                Renderer.BindShader(Renderer.ShaderBound);
                _lastPosition = _player.Position.Xz.ToVector3();
                _previousActiveChunks = _player.Loader.ActiveChunks;
            }
        }

        public void Draw()
        {
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
            _miniMapNorth.BaseTexture.TextureElement.Angle = -_player.Model.LocalRotation.Y;

            UpdateRingObject(_miniMapMarker, MarkedDirection, HasMarker);
            UpdateRingObject(_miniMapQuestMarker, (MarkedQuestPosition - _player.Position).Xz.NormalizedFast().ToVector3(), HasQuestMarker);
            if((MarkedQuestPosition - _player.Position).Xz.LengthSquared < Chunk.Width * .5f * Chunk.Width * .5f)
                _miniMapQuestMarker.Disable();

            this.DrawMap();

            Shader.Bind();
            Renderer.Enable(EnableCap.Blend);
            Renderer.Disable(EnableCap.DepthTest);

            DrawManager.UIRenderer.SetupQuad();

            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, _mapFbo.TextureId[0]);
            Shader["Fill"] = 1;

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, _miniMap.TextureElement.TextureId);
            Shader["Texture"] = 0;

            Shader["Scale"] = _miniMap.TextureElement.Scale;
            Shader["Position"] = _miniMap.TextureElement.AdjustedPosition;
            Shader["Flipped"] = _miniMap.TextureElement.IdPointer == null && !_miniMap.TextureElement.Flipped ? 0 : 1;
            Shader["Opacity"] = _miniMap.TextureElement.Opacity;
            Shader["Grayscale"] = _miniMap.TextureElement.Grayscale ? 1 : 0;
            Shader["Tint"] = _miniMap.TextureElement.Tint;
            Shader["Rotation"] = Matrix3.CreateRotationZ(-_player.Model.LocalRotation.Y * Mathf.Radian);

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.CullFace);
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Shader.Unbind();
        }

        private void UpdateRingObject(RenderableTexture Texture, Vector3 Direction, bool Condition)
        {
            if (Condition)
            {
                Texture.BaseTexture.TextureElement.Angle = -_player.Model.LocalRotation.Y + Physics.DirectionToEuler(Direction).Y;
                Texture.Enable();
            }
            else
            {
                Texture.Disable();
            }
        }
        
        public bool Show
        {
            get => _show;
            set{
                _show = value;
                if(_show) _panel.Enable();
                else _panel.Disable();
            }
        }

        public void Dispose()
        {
            _mapFbo.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }
    }
}