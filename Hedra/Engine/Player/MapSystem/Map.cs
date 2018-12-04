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
using System.Drawing;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.MapSystem
{
    /// <summary>
    /// Description of Map.
    /// </summary>
    public class Map : PlayerInterface, IDisposable
    {
        private const int MapViewSize = 8;
        private const int MapSize = 8;
        private const int ChunkSize = 4;
        private const float FogDistance = 140f;
        private readonly LocalPlayer _player;
        private readonly MapStateManager _stateManager;
        private readonly List<MapItem> _icons;
        private readonly MapBuilder _builder;
        private readonly MapMeshBuilder _meshBuilder;
        private readonly List<MapBaseItem> _baseItems;
        private readonly ObjectMesh _cursor;
        private readonly ObjectMesh _marker;
        private readonly Panel _panel;
        private bool _show;
        private float _size;
        private float _targetSize;
        private int _previousSeed;
        private float _height;
        private float _targetHeight;
        private float _targetTime = float.MaxValue;
        private int lastChunkAmount = -1;
        private float _mapDitherRadius;
        private Chunk _underChunk;

        public Map(LocalPlayer Player)
        {
            this._player = Player;
            this._panel = new Panel();
            this._icons = new List<MapItem>();
            this._stateManager = new MapStateManager(Player);
            this._builder = new MapBuilder();
            this._baseItems = new List<MapBaseItem>();
            this._meshBuilder = new MapMeshBuilder(_player, MapSize, ChunkSize);
            this._cursor = ObjectMesh.FromVertexData(AssetManager.PLYLoader("Assets/UI/MapCursor.ply", Vector3.One * 20f), false);
            this._marker = ObjectMesh.FromVertexData(AssetManager.PLYLoader("Assets/UI/MapMarker.ply", Vector3.One * 5f), false);
            _marker.ApplyFog = false;

            var hint = new GUIText("CLICK TO MARK A WAYPOINT",
                Vector2.UnitY * .8f, Color.White,
                FontCache.Get(AssetManager.BoldFamily, 16f, FontStyle.Bold));
            var underline = new GUIText("＿＿＿＿＿＿＿＿",
                Vector2.UnitY * .75f, Color.FromArgb(255, 30, 30, 30),
                FontCache.Get(AssetManager.BoldFamily, 16f, FontStyle.Bold));
            _panel.AddElement(hint);
            _panel.AddElement(underline);
            EventDispatcher.RegisterMouseDown(this, delegate(object Sender, MouseButtonEventArgs Args)
            {
                if(!_show) return;
                if (Args.Button == MouseButton.Left)
                {
                    _player.Minimap.Mark(_player.View.CrossDirection.Xz.ToVector3().NormalizedFast());
                    _player.Orientation = _player.Minimap.MarkedDirection;
                    _player.Model.TargetRotation = Physics.DirectionToEuler(_player.Minimap.MarkedDirection);
                }
                else if(Args.Button == MouseButton.Right) _player.Minimap.Unmark();
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            });
        }

        public void Update()
        {
            if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

            this._size = Mathf.Lerp(_size, _targetSize, Time.IndependantDeltaTime * 4f);
            this._height = Mathf.Lerp(_height, _targetHeight, Time.IndependantDeltaTime * 2f);
            this.UpdateFogAndTime();

            var mapPosition = _player.Model.ModelPosition.Xz.ToVector3();
            for (var i = 0; i < _icons.Count; i++)
            { 
                _icons[i].Mesh.Rotation = new Vector3(_icons[i].Mesh.Rotation.X, _icons[i].Mesh.Rotation.Y + (float) Time.DeltaTime * 0f, _icons[i].Mesh.Rotation.Z);
                _icons[i].Mesh.Position = new Vector3(mapPosition.X, _targetHeight + 10f, mapPosition.Z);
            }          
            for (var i = 0; i < _baseItems.Count; i++)
            {
                if (_baseItems[i].Mesh != null)
                {
                    _baseItems[i].Mesh.Position = new Vector3(mapPosition.X, _targetHeight - ChunkSize * 2f + 25f, mapPosition.Z);
                }
            }
            if (_show)
            {
                _marker.Enabled = _player.Minimap.HasMarker;
                _marker.Position = mapPosition + Vector3.UnitY * (_targetHeight + 25f) + _player.Minimap.MarkedDirection * FogDistance;
                _cursor.Position = mapPosition + Vector3.UnitY * (_targetHeight + 45f);
                _cursor.Rotation = _player.Model.Rotation;
                WorldRenderer.Scale = Mathf.Lerp(Vector3.One,
                    Vector3.One * (ChunkSize / (float)Chunk.Width), 1f) + Vector3.One * 0.002f;
                WorldRenderer.BakedOffset = -(mapPosition + Vector3.UnitY * _targetHeight);
                WorldRenderer.Offset = mapPosition + Vector3.UnitY * (_targetHeight + 80f);
                WorldRenderer.WaterSmoothness = ChunkSize / (float)Chunk.Width;
                this.UpdateChunks();
            }
        }

        private void UpdateFogAndTime()
        {
            if (float.MaxValue != _targetTime)
            {
                SkyManager.SetTime(Mathf.Lerp(SkyManager.DayTime, _targetTime, (float)Time.DeltaTime * 2f));
                if (Math.Abs(SkyManager.DayTime - _targetTime) < 10)
                {
                    if (SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
                }
            }
            if (!Show) return;
            this._player.View.PositionDelegate = () => _player.Model.Position.Xz.ToVector3() + Math.Max(_height, _player.Model.Position.Y) * Vector3.UnitY;
            SkyManager.FogManager.UpdateFogSettings(FogDistance * .95f, FogDistance);
            SkyManager.Sky.TopColor = Vector4.One;
            SkyManager.Sky.BotColor = Color.CadetBlue.ToVector4();
        }

        public void Draw()
        {
            if (_size < 0.01f) return;

            for (var i = 0; i < _baseItems.Count; i++)
            {
                if (_baseItems[i].Mesh != null)
                {
                    _baseItems[i].Mesh.Alpha = _size;
                    _baseItems[i].Mesh.Draw();
                }
            }
            
            for (var i = 0; i < _icons.Count; i++)
            {
                _icons[i].Mesh.Alpha = _size;
                _icons[i].Draw();
            }
        }

        private void UpdateIcons()
        {
            for (var x = 0; x < MapViewSize; x++)
            {
                for (var z = 0; z < MapViewSize; z++)
                {
                    var coords = new Vector2(
                        x - MapViewSize / 2,
                        z - MapViewSize / 2
                    );
                    for (var i = 0; i < MapSize; i++)
                    {
                        for (var j = 0; j < MapSize; j++)
                        {
                            var playerPos = World.ToChunkSpace(GameManager.Player.Position);
                            var pos = playerPos + new Vector2(coords.X, coords.Y) * Chunk.Width * MapSize +
                                      new Vector2((i - MapSize / 2) * Chunk.Width, (j - MapSize / 2) * Chunk.Width);
                            var chunk = World.GetChunkAt(pos.ToVector3());
                            if (!MapBaseItem.UsableChunk(chunk))
                            {
                                var biome = World.BiomePool.GetRegion(pos.ToVector3());
                                var sample = _builder.Sample(pos.ToVector3(), biome);
                                if (sample?.Icon != null)
                                {
                                    var realPos = new Vector2(coords.X, coords.Y) * ChunkSize * MapSize
                                                  + new Vector2((i - MapSize / 2) * ChunkSize,
                                                      (j - MapSize / 2) * ChunkSize);
                                    var icon = sample.Icon.Clone();
                                    var baseData = new CubeData();
                                    var scale = 3f;
                                    baseData.AddFace(Face.ALL);
                                    baseData.Scale(new Vector3(scale, 1f, scale));
                                    baseData.TransformVerts(-Vector3.UnitY * 1f - new Vector3(scale, 0, scale) * .5f);
                                    baseData.Color = CubeData.CreateCubeColor(Color.DarkSlateGray.ToVector4());
                                    var mapItem = new MapItem(icon + baseData.ToVertexData());
                                    mapItem.Mesh.ApplyNoiseTexture = true;
                                    mapItem.Mesh.Rotation = new Vector3(0, Utils.Rng.Next(0, 4) * 90f, 0);
                                    mapItem.Mesh.LocalPosition = realPos.ToVector3() + Vector3.UnitY * 12;
                                    mapItem.Mesh.Scale = Vector3.One * 2f;
                                    lock (_icons) _icons.Add(mapItem);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ClearIcons()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                _icons[i].Dispose();
            }
            _icons.Clear();       
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
            for (var x = 0; x < MapViewSize+1; x++)
            for (var z = 0; z < MapViewSize+1; z++)
            {
                var coords = new Vector2(
                    x - MapViewSize / 2,
                    z - MapViewSize / 2
                    );
                var item = this.FindByCoordinates(coords);
                if (item == null || this.NeedsUpdate(item, coords) || forceUpdate)
                {
                    this.GenerateMesh(coords);
                }
            }
        }

        private bool NeedsUpdate(MapBaseItem Item, Vector2 Coordinates)
        {
            for (var x = 0; x < MapSize; x++)
            {
                for (var z = 0; z < MapSize; z++)
                {
                    var playerPos = World.ToChunkSpace(_player.Position);
                    var chunkPosition = playerPos + Coordinates * Chunk.Width * MapSize
                                        + new Vector2((x - MapSize / 2) * Chunk.Width, (z - MapSize / 2) * Chunk.Width);
                    var chunk = World.GetChunkByOffset(chunkPosition);
                    if (Item.HasChunk[x * MapSize + z] != MapBaseItem.UsableChunk(chunk)) return true;
                }
            }
            return false;
        }

        private void UpdateChunks()
        {
            var currentChunkAmount = _player.Loader.ActiveChunks;
            if (lastChunkAmount == currentChunkAmount) return;

            this.UpdateMap();
            lastChunkAmount = currentChunkAmount;
        }

        private void GenerateMesh(Vector2 Coords)
        {
            var baseItem = this.FindByCoordinates(Coords);
            if (baseItem == null)
            {
                var newMesh = new ObjectMesh();
                baseItem = new MapBaseItem(MapSize, newMesh)
                {
                    Coordinates = Coords
                };
                _baseItems.Add(baseItem);
            }
            this.GenerateMesh(baseItem, Coords);
        }

        private void GenerateMesh(MapBaseItem BaseItem, Vector2 Coords)
        {
            TaskScheduler.Parallel(delegate
            {
                var prevMesh = BaseItem.Mesh;
                var item = _meshBuilder.BuildItem(Coords);
                Executer.ExecuteOnMainThread(delegate
                {
                    item.Mesh.Position = prevMesh.Position;
                    BaseItem.Mesh = item.Mesh;
                    BaseItem.HasChunk = item.HasChunk;
                    BaseItem.Coordinates = Coords;
                    BaseItem.WasBuilt = item.WasBuilt;
                    if (prevMesh?.Mesh != null) prevMesh.Dispose();
                });
            });
        }

        private MapBaseItem FindByCoordinates(Vector2 Coordinates)
        {
            for (var i = 0; i < _baseItems.Count; i++)
            {
                if (_baseItems[i].Coordinates == Coordinates) return _baseItems[i];
            }
            return null;
        }

        public override Key OpeningKey => Key.M;

        public override bool Show
        {
            get => _show;
            set
            {
                if(GameManager.IsLoading || _player.Trade.Show) return;

                if (value)
                {
                    _stateManager.CaptureState();
                    this.UpdateChunks();
                    this.ClearIcons();
                    TaskScheduler.Parallel(this.UpdateIcons);
                    SkyManager.UpdateDayColors = false;
                    WorldRenderer.EnableCulling = false;                  
                    this._targetSize = 1.0f;
                    this._player.Loader.ShouldUpdateFog = false;
                    this._player.View.MaxDistance = 100f;
                    this._player.View.MinDistance = 0f;
                    this._player.View.TargetDistance = 100f;
                    this._player.View.MaxPitch = -0.2f;
                    this._player.View.MinPitch = -1.4f;
                    this._player.View.WheelSpeed = 5f;
                    this._targetHeight = 2500f;
                    this._height = _targetHeight;
                    this._targetTime = 12000;
                    this._player.Toolbar.Listen = false;
                    _panel.Enable();
                    SkyManager.PushTime();                   
                }
                else
                {
                    _stateManager.ReleaseState();
                    _panel.Disable();
                    this._player.Minimap.Show = true;
                    this._targetSize = 0f;
                    this._targetHeight = 0;
                    this._targetTime = SkyManager.PeekTime();
                    TaskScheduler.When(() => _height < Camera.DefaultDelegate().Y, delegate
                    {

                        this._player.View.PositionDelegate = Camera.DefaultDelegate;
                        SkyManager.PopTime();
                        _targetTime = float.MaxValue;
                        if(!_show) _height = 0f;

                    });
                    _player.Loader.UpdateFog(Force: true);
                }
                SoundPlayer.PlayUISound(SoundType.ButtonHover, 1.0f, 0.6f);
                _show = value;
            }
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterMouseDown(this);
        }
    }
}
