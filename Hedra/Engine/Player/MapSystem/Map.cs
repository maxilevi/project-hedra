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
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
	/// <summary>
	/// Description of Map.
	/// </summary>
	public class Map
	{
	    private const int MapViewSize = 8;
        private const int MapSize = 8;
	    private const int ChunkSize = 4;
        private readonly LocalPlayer _player;
	    private readonly MapStateManager _stateManager;
	    private readonly List<MapItem> _icons;
	    private readonly MapBuilder _builder;
	    private readonly MapMeshBuilder _meshBuilder;
        private readonly List<MapBaseItem> _baseItems;
		private bool _show;
	    private float _size;
	    private float _targetSize;
	    private int _previousSeed;
		private float _height;
	    private float _targetHeight;
	    private float _targetTime = float.MaxValue;
	    private int lastChunkAmount = -1;
	    private Vector2 _markedChunkPosition;

        public Map(LocalPlayer Player){
			this._player = Player;
            this._icons = new List<MapItem>();
            this._stateManager = new MapStateManager(Player);
            this._builder = new MapBuilder();
            this._baseItems = new List<MapBaseItem>();
            this._meshBuilder = new MapMeshBuilder(_player, MapSize, ChunkSize);
        }

		public void Update(){
			if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

		    this._size = Mathf.Lerp(_size, _targetSize, Time.unScaledDeltaTime * 4f);
		    this._height = Mathf.Lerp(_height, _targetHeight, Time.unScaledDeltaTime * 4f);
            this.UpdateFogAndTime();

            var mapPosition = _player.Model.Model.Position.Xz.ToVector3();
		    lock (_icons)
		    {
		        for (var i = 0; i < _icons.Count; i++)
		        {
		            _icons[i].Mesh.Position = new Vector3(mapPosition.X, _height + 10f, mapPosition.Z);
		        }
		    }
		    for (var i = 0; i < _baseItems.Count; i++)
		    {
		        if (_baseItems[i].Mesh != null)
		        {
		            _baseItems[i].Mesh.Position = new Vector3(mapPosition.X, _height - ChunkSize * 2f + 25f, mapPosition.Z);
		        }
		    }
		    if (_show)
		    {
                this._player.Minimap.Show = false;
		        WorldRenderer.Scale = Mathf.Lerp(Vector3.One, Vector3.One * (ChunkSize / (float)Chunk.Width), _size) + Vector3.One * 0.002f;
                WorldRenderer.BakedOffset = -(mapPosition + Vector3.UnitY * _height);
                WorldRenderer.Offset = mapPosition + Vector3.UnitY * (_height+80f);
		        this.UpdateChunks();
            }
		}

	    private void UpdateFogAndTime()
	    {
	        if (float.MaxValue != _targetTime)
	        {
	            SkyManager.SetTime(Mathf.Lerp(SkyManager.DayTime, _targetTime, (float)Time.deltaTime * 2f));
	            if (Math.Abs(SkyManager.DayTime - _targetTime) < 10)
	            {
	                if (SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
	            }
	        }
	        if (!Show) return;
	        this._player.View.PositionDelegate = () => _player.Model.Model.Position.Xz.ToVector3() + _height * Vector3.UnitY;
            SkyManager.Skydome.TopColor = Vector4.One;
	        SkyManager.Skydome.BotColor = Color.CadetBlue.ToVector4();
	        SkyManager.FogManager.UpdateFogSettings(140 * .95f, 140);
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
	        this.ClearIcons();
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
	                                var mapItem = new MapItem(sample.Icon.Clone());
                                    mapItem.Mesh.Rotation = new Vector3(0, Utils.Rng.Next(0, 8) * 45f, 0);
	                                mapItem.Mesh.LocalPosition = realPos.ToVector3();
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
	        lock (_icons)
	        {
	            for (int i = 0; i < _icons.Count; i++)
	            {
	                _icons[i].Dispose();
	            }
                _icons.Clear();
	        }
	    }

	    private void UpdateChunkBounds()
	    {
            for (var i = 0; i < _baseItems.Count; i++)
	        {
	            if ( Math.Abs(_baseItems[i].Coordinates.X) > MapViewSize 
                    || Math.Abs(_baseItems[i].Coordinates.Y) > MapViewSize )
	            {
	                _baseItems[i].Mesh.Dispose();
                    _baseItems.RemoveAt(i);
	            }   
	        }   
	    }

	    private void UpdateMap()
	    {
            for (var x = 0; x < MapViewSize+1; x++)
            for (var z = 0; z < MapViewSize+1; z++)
            {
                var coords = new Vector2(
                    x - MapViewSize / 2,
                    z - MapViewSize / 2
                    );
                var item = this.FindByCoordinates(coords);
                if (item == null || this.NeedsUpdate(item, coords))
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

	        this.UpdateChunkBounds();
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
            TaskManager.Parallel(delegate
	        {
	            var prevMesh = BaseItem.Mesh;
	            var item = _meshBuilder.BuildItem(Coords);
                BaseItem.Mesh = item.Mesh;
	            BaseItem.HasChunk = item.HasChunk;
	            BaseItem.Coordinates = Coords;
	            BaseItem.WasBuilt = item.WasBuilt;
	            ThreadManager.ExecuteOnMainThread(delegate
	            {
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

        public bool Show{
			get{ return _show; }
			set{
				if(GameManager.IsLoading) return;

                if (value)
				{
                    _stateManager.CaptureState();
				    this.UpdateChunks();
				    TaskManager.Parallel(this.UpdateIcons);
				    SkyManager.UpdateDayColors = false;
                    WorldRenderer.EnableCulling = false;                         
                    this._targetSize = 1.0f;
                    this._player.View.MaxDistance = 100f;
				    this._player.View.MinDistance = 0f;
				    this._player.View.TargetDistance = 50f;
                    this._player.View.MaxPitch = -0.2f;
					this._player.View.MinPitch = -1.4f;
				    this._player.View.WheelSpeed = 5f;
					this._targetHeight = 2500f;
				    this._targetTime = 12000;
				    this._player.Minimap.Show = false;
                    SkyManager.PushTime();                   
				}else
				{
                    _stateManager.ReleaseState();
				    this._player.Minimap.Show = true;
                    this._targetSize = 0f;
					this._targetHeight = _player.Model.Model.Position.Y;
                    this._targetTime = SkyManager.PeekTime();
                    TaskManager.Delay(() => Math.Abs(_height - _targetHeight) < 0.01f, delegate
				    {
				        this._player.View.PositionDelegate = Camera.DefaultDelegate;
				        SkyManager.PopTime();
				        _targetTime = float.MaxValue;
                        _height = 0f;

                    });
				    SkyManager.FogManager.UpdateFogSettings(_player.Loader.MinFog, _player.Loader.MaxFog);
                }
				Sound.SoundManager.PlayUISound(Sound.SoundType.OnOff, 1.0f, 0.6f);
				_show = value;
			}
		}
	}
}
