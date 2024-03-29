/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 03:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using Hedra.Structures;
using Silk.NET.Input;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.MapSystem
{
    /// <summary>
    ///     Description of Map.
    /// </summary>
    public class Map : PlayerInterface, IDisposable
    {
        private const int MapViewSize = 8;
        private const int MapSize = 8;
        private const int ChunkSize = 4;
        private const float FogDistance = 140f;
        private readonly List<MapBaseItem> _baseItems;
        private readonly ObjectMesh _baseMesh;
        private readonly MapBuilder _builder;
        private readonly ObjectMesh _cursor;
        private readonly List<MapItem> _icons;
        private readonly object _iconsLock;
        private readonly ObjectMesh _marker;
        private readonly List<ObjectMesh> _markers;
        private readonly MapMeshBuilder _meshBuilder;
        private readonly Panel _panel;
        private readonly LocalPlayer _player;
        private readonly ObjectMesh _questMarker;
        private readonly MapStateManager _stateManager;
        private float _height;
        private float _mapDitherRadius;
        private int _previousSeed;
        private bool _show;
        private float _size;
        private float _targetHeight;
        private float _targetSize;
        private readonly TimeHandler _timeHandler;
        private Chunk _underChunk;
        private int lastChunkAmount = -1;

        public Map(LocalPlayer Player)
        {
            _timeHandler = new TimeHandler(12000);
            _iconsLock = new object();
            _player = Player;
            _panel = new Panel();
            _icons = new List<MapItem>();
            _stateManager = new MapStateManager(Player);
            _builder = new MapBuilder();
            _baseItems = new List<MapBaseItem>();
            _meshBuilder = new MapMeshBuilder(_player, MapSize, ChunkSize);
            _cursor = ObjectMesh.FromVertexData(AssetManager.PLYLoader("Assets/UI/MapCursor.ply", Vector3.One * 20f),
                false);
            _markers = new[]
            {
                _marker = ObjectMesh.FromVertexData(AssetManager.PLYLoader("Assets/UI/MapMarker.ply", Vector3.One * 5f),
                    false),
                _questMarker = ObjectMesh.FromVertexData(
                    AssetManager.PLYLoader("Assets/Env/ExclamationMark.ply", Vector3.One * 5f) +
                    CreateBaseVertexData(3f), false)
            }.ToList();
            _markers.ForEach(M =>
            {
                M.ApplyNoiseTexture = true;
                M.ApplyFog = false;
            });

            var hint = new GUIText(Translation.Create("mark_waypoint"),
                Vector2.UnitY * .8f, Color.White,
                FontCache.GetBold(16f));
            var underline = new GUIText("_______________",
                Vector2.UnitY * .75f, Color.FromRgb(30, 30, 30),
                FontCache.GetBold(16f));
            _panel.AddElement(hint);
            _panel.AddElement(underline);
            EventDispatcher.RegisterMouseDown(this, delegate(object Sender, MouseButtonEventArgs Args)
            {
                if (!_show) return;
                if (Args.Button == MouseButton.Left)
                {
                    _player.Minimap.Mark(_player.View.CrossDirection.Xz().ToVector3().NormalizedFast());
                    _player.Orientation = _player.Minimap.MarkedDirection;
                    _player.Model.TargetRotation = Physics.DirectionToEuler(_player.Minimap.MarkedDirection);
                }
                else if (Args.Button == MouseButton.Right)
                {
                    _player.Minimap.UnMark();
                }

                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            });
        }

        public override Key OpeningKey => Controls.Map;

        public override bool Show
        {
            get => _show;
            set
            {
                if (GameManager.IsLoading || _player.Trade.Show) return;

                if (value)
                {
                    _stateManager.CaptureState();
                    UpdateChunks();
                    ClearIcons();
                    _timeHandler.Apply();
                    TaskScheduler.Parallel(UpdateIcons);
                    SkyManager.UpdateDayColors = false;
                    WorldRenderer.EnableCulling = false;
                    _targetSize = 1.0f;
                    _player.Loader.ShouldUpdateFog = false;
                    _player.View.MaxDistance = 100f;
                    _player.View.MinDistance = 0f;
                    _player.View.TargetDistance = 100f;
                    _player.View.MaxPitch = -0.2f;
                    _player.View.MinPitch = -1.4f;
                    _player.View.WheelSpeed = 5f;
                    _player.View.AllowClipping = true;
                    _targetHeight = 4096;
                    _height = _targetHeight;
                    _player.Toolbar.Listen = false;
                    _cursor.Enabled = true;
                    _panel.Enable();
                }
                else
                {
                    _stateManager.ReleaseState();
                    _panel.Disable();
                    _player.Minimap.Show = true;
                    _targetSize = 0f;
                    _targetHeight = 0;
                    _player.View.AllowClipping = false;
                    _cursor.Enabled = false;
                    _timeHandler.Remove();
                    TaskScheduler.When(() => _height < Camera.DefaultDelegate().Y, delegate
                    {
                        _player.View.PositionDelegate = Camera.DefaultDelegate;
                        if (!_show) _height = 0f;
                    });
                    _player.Loader.UpdateFog(true);
                }

                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
                _show = value;
            }
        }

        protected override bool HasExitAnimation => false;

        public void Dispose()
        {
            EventDispatcher.UnregisterMouseDown(this);
        }

        public void Update()
        {
            if (World.Seed == World.MenuSeed && Show) Show = false;

            _size = Mathf.Lerp(_size, _targetSize, Time.IndependentDeltaTime * 4f);
            _height = Mathf.Lerp(_height, _targetHeight, Time.IndependentDeltaTime * 2f);
            UpdateFogAndTime();

            var mapPosition = _player.Model.ModelPosition.Xz().ToVector3();
            lock (_iconsLock)
            {
                for (var i = 0; i < _icons.Count; i++)
                {
                    _icons[i].Mesh.LocalRotation = new Vector3(_icons[i].Mesh.LocalRotation.X,
                        _icons[i].Mesh.LocalRotation.Y + Time.DeltaTime * 0f, _icons[i].Mesh.LocalRotation.Z);
                    _icons[i].Mesh.Position = new Vector3(mapPosition.X, _targetHeight - 7, mapPosition.Z);
                }
            }

            for (var i = 0; i < _baseItems.Count; i++)
                if (_baseItems[i].Mesh != null)
                    _baseItems[i].Mesh.Position = new Vector3(mapPosition.X, _targetHeight, mapPosition.Z);
            _markers.ForEach(M => M.Enabled = _show);
            if (_show)
            {
                _markers.ForEach(M => M.Position = mapPosition + Vector3.UnitY * (_targetHeight + 25f));

                /* ReSharper disable once AssignmentInConditionalExpression */
                if (_marker.Enabled &= _player.Minimap.HasMarker)
                    _marker.Position += _player.Minimap.MarkedDirection * FogDistance;

                /* ReSharper disable once AssignmentInConditionalExpression */
                if (_questMarker.Enabled &= _player.Minimap.HasQuestMarker)
                {
                    var markedQuestPosition = _player.Minimap.MarkedQuestPosition();
                    _questMarker.Position +=
                        Mathf.Min(
                            Mathf.Max(Vector2.One, (markedQuestPosition - mapPosition).Xz()).NormalizedFast() *
                            FogDistance,
                            ToMapCoordinates(markedQuestPosition.Xz())
                        ).ToVector3();
                }

                _cursor.Position = mapPosition + Vector3.UnitY * (_targetHeight + 45);
                _cursor.LocalRotation = _player.Model.LocalRotation;
                WorldRenderer.Scale = Mathf.Lerp(Vector3.One,
                    Vector3.One * (ChunkSize / (float)Chunk.Width), 1f) + Vector3.One * 0.002f;
                var worldOffset = Vector3.UnitY * (_targetHeight + Chunk.Height / 2);
                WorldRenderer.BakedOffset = -(mapPosition + worldOffset);
                WorldRenderer.Offset = mapPosition + worldOffset;
                WorldRenderer.WaterSmoothness = ChunkSize / (float)Chunk.Width;
                UpdateChunks();
            }
        }

        private void UpdateFogAndTime()
        {
            _timeHandler.Update();
            if (!Show) return;
            _player.View.PositionDelegate = () =>
                _player.Model.Position.Xz().ToVector3() + Math.Max(_height, _player.Model.Position.Y) * Vector3.UnitY;
            SkyManager.FogManager.UpdateFogSettings(FogDistance * .95f, FogDistance);
            SkyManager.Sky.TopColor = Vector4.One;
            SkyManager.Sky.BotColor = Color.CadetBlue.AsVector4();
        }

        public void Draw()
        {
            if (_size < 0.01f) return;

            for (var i = 0; i < _baseItems.Count; i++)
                if (_baseItems[i].Mesh != null)
                {
                    _baseItems[i].Mesh.Alpha = _size;
                    _baseItems[i].Mesh.Draw();
                }

            lock (_iconsLock)
            {
                for (var i = 0; i < _icons.Count; i++)
                {
                    _icons[i].Mesh.Alpha = _size;
                    _icons[i].Draw();
                }
            }
        }

        private void UpdateIcons()
        {
            for (var x = 0; x < MapViewSize; x++)
            for (var z = 0; z < MapViewSize; z++)
            {
                var coords = new Vector2(
                    x - MapViewSize / 2,
                    z - MapViewSize / 2
                );
                for (var i = 0; i < MapSize; i++)
                for (var j = 0; j < MapSize; j++)
                {
                    var playerPos = World.ToChunkSpace(GameManager.Player.Position);
                    var pos = playerPos + new Vector2(coords.X, coords.Y) * Chunk.Width * MapSize +
                              new Vector2((i - MapSize / 2) * Chunk.Width, (j - MapSize / 2) * Chunk.Width);
                    var chunk = World.GetChunkAt(pos.ToVector3());
                    if (!MapBaseItem.UsableChunk(chunk))
                    {
                        var biome = World.BiomePool.GetRegion(pos.ToVector3());
                        var sample = MapBuilder.Sample(pos.ToVector3(), biome);
                        if (sample != null)
                        {
                            var realPos = new Vector2(coords.X, coords.Y) * ChunkSize * MapSize
                                          + new Vector2((i - MapSize / 2) * ChunkSize,
                                              (j - MapSize / 2) * ChunkSize);
                            var icon = sample.Icon != null ? sample.Icon.Clone() : VertexData.Load("Assets/Env/Objects/UnknownIcon.ply", Vector3.One * 1f);
                            var mapItem = new MapItem(icon + CreateBaseVertexData());
                            mapItem.Mesh.ApplyNoiseTexture = true;
                            mapItem.Mesh.LocalRotation = new Vector3(0, Utils.Rng.Next(0, 4) * 90f, 0);
                            mapItem.Mesh.LocalPosition = realPos.ToVector3() + Vector3.UnitY * 12 
                                                                             + new Vector3(Utils.Rng.NextSingle(), 0, Utils.Rng.NextSingle()) * (2f - 1f) * 8f;
                            mapItem.Mesh.Scale = Vector3.One * 2f;
                            lock (_iconsLock)
                            {
                                _icons.Add(mapItem);
                            }
                        }
                    }
                }
            }
        }

        private static Vector2 ToMapCoordinates(Vector2 WorldCoordinates)
        {
            var result = WorldCoordinates - World.ToChunkSpace(GameManager.Player.Position);
            return result / MapSize / Chunk.Width * ChunkSize * MapSize;
        }

        private static VertexData CreateBaseVertexData(float Scalar = 1f)
        {
            var scale = 3f * Scalar;
            var baseData = new CubeData();
            baseData.AddFace(Face.ALL);
            baseData.Scale(new Vector3(scale, 1f * Scalar, scale));
            baseData.TransformVerts(-Vector3.UnitY * 1f - new Vector3(scale, 0, scale) * .5f);
            baseData.Color = CubeData.CreateCubeColor(Color.DarkSlateGray.AsVector4());
            return baseData.ToVertexData();
        }

        private void ClearIcons()
        {
            lock (_iconsLock)
            {
                for (var i = 0; i < _icons.Count; i++) _icons[i].Dispose();

                _icons.Clear();
            }
        }

        private void UpdateMap()
        {
            var newChunk = World.GetChunkAt(_player.Position);
            var forceUpdate = false;
            if (newChunk != _underChunk)
            {
                _underChunk = newChunk;
                forceUpdate = true;
            }

            for (var x = 0; x < MapViewSize + 1; x++)
            for (var z = 0; z < MapViewSize + 1; z++)
            {
                var coords = new Vector2(
                    x - MapViewSize / 2,
                    z - MapViewSize / 2
                );
                var item = FindByCoordinates(coords);
                if (item == null || NeedsUpdate(item, coords) || forceUpdate) GenerateMesh(coords);
            }
        }

        private bool NeedsUpdate(MapBaseItem Item, Vector2 Coordinates)
        {
            for (var x = 0; x < MapSize; x++)
            for (var z = 0; z < MapSize; z++)
            {
                var playerPos = World.ToChunkSpace(_player.Position);
                var chunkPosition = playerPos + Coordinates * Chunk.Width * MapSize
                                              + new Vector2((x - MapSize / 2) * Chunk.Width,
                                                  (z - MapSize / 2) * Chunk.Width);
                var chunk = World.GetChunkByOffset(chunkPosition);
                if (Item.HasChunk[x * MapSize + z] != MapBaseItem.UsableChunk(chunk)) return true;
            }

            return false;
        }

        private void UpdateChunks()
        {
            var currentChunkAmount = _player.Loader.ActiveChunks;
            if (lastChunkAmount == currentChunkAmount) return;

            UpdateMap();
            lastChunkAmount = currentChunkAmount;
        }

        private void GenerateMesh(Vector2 Coords)
        {
            var baseItem = FindByCoordinates(Coords);
            if (baseItem == null)
            {
                baseItem = new MapBaseItem(MapSize, null)
                {
                    Coordinates = Coords
                };
                _baseItems.Add(baseItem);
            }

            GenerateMesh(baseItem, Coords);
        }

        private void GenerateMesh(MapBaseItem BaseItem, Vector2 Coords)
        {
            TaskScheduler.Parallel(delegate
            {
                var item = _meshBuilder.BuildItem(Coords);
                Executer.ExecuteOnMainThread(delegate
                {
                    item.Mesh.Position = Vector3.Zero;
                    BaseItem.Mesh?.Dispose();
                    BaseItem.Mesh = item.Mesh;
                    BaseItem.HasChunk = item.HasChunk;
                    BaseItem.Coordinates = Coords;
                    BaseItem.WasBuilt = item.WasBuilt;
                });
            });
        }

        private MapBaseItem FindByCoordinates(Vector2 Coordinates)
        {
            for (var i = 0; i < _baseItems.Count; i++)
                if (_baseItems[i].Coordinates == Coordinates)
                    return _baseItems[i];
            return null;
        }
    }
}