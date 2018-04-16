﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 03:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
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
        private const int ViewSize = 1000;
	    private const int MapViewSize = 8;
        private const int MapSize = 4;
	    private const int ChunkSize = 128;
        private readonly LocalPlayer _player;
	    private readonly MapStateManager _stateManager;
	    private readonly MapItem _cursor;
	    private readonly List<MapItem> _icons;
	    private readonly MapBuilder _builder;
	    private readonly MapInputHandler _mapInputHandler;
	    private readonly MapMeshBuilder _meshBuilder;
	    private readonly List<MapBaseItem> _baseItems;
		private bool _show;
	    private float _size;
	    private float _targetSize;
	    private int _previousSeed;
		private float _height;
	    private float _targetHeight;
	    private float _targetTime = float.MaxValue;

        public Map(LocalPlayer Player){
			this._player = Player;
            this._icons = new List<MapItem>();
            this._stateManager = new MapStateManager(Player);
            this._builder = new MapBuilder();
            this._cursor = new MapItem(AssetManager.PlyLoader("Assets/UI/MapCursor.ply", Vector3.One * 15f));
            this._baseItems = new List<MapBaseItem>();
            this._mapInputHandler = new MapInputHandler(Player);
            this._meshBuilder = new MapMeshBuilder(_player, MapSize, ChunkSize);
            this._mapInputHandler.OnMove += this.UpdateChunkBounds;
            this._mapInputHandler.OnMove += this.UpdateMap;
        }

		public void Update(){
			if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

		    this._mapInputHandler.Update();
		    this._size = Mathf.Lerp(_size, _targetSize, Time.unScaledDeltaTime * 4f);
		    this._height = Mathf.Lerp(_height, _targetHeight, Time.unScaledDeltaTime * 4f);
		    this.UpdateFogAndTime();	

		    for (var i = 0; i < _icons.Count; i++)
		    {
		        _icons[i].Rotation = new Vector3(
                    _icons[i].Rotation.X,
                    _icons[i].Rotation.Y + (float)Time.deltaTime * 40f,
                    _icons[i].Rotation.Z
                    );
                _icons[i].Mesh.LocalPosition = new Vector3(_player.Position.X, _height, _player.Position.Z);
		    }
		    for (var i = 0; i < _baseItems.Count; i++)
		    {
		        _baseItems[i].Mesh.Position = new Vector3(_player.Position.X, _height, _player.Position.Z)
		                             - _mapInputHandler.Position;
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
	        this._player.View.PositionDelegate = () => _player.Model.Model.Position + _height * Vector3.UnitY;
	        SkyManager.FogManager.UpdateFogSettings(ViewSize * 1.75f, ViewSize * 2.0f);
	    }

	    public void Draw()
	    {
	        if (_size < 0.01f) return;

	        for (var i = 0; i < _icons.Count; i++)
	        {
	            _icons[i].Draw();
	        }
            _cursor.Draw();
	    }

	    private void UpdateChunkBounds()
	    {
            var relativePosition = RelativeViewerPosition;
            for (var i = 0; i < _baseItems.Count; i++)
	        {
	            if ( Math.Abs(_baseItems[i].Mesh.LocalRotationPoint.X - relativePosition.X) > MapViewSize 
                    || Math.Abs(_baseItems[i].Mesh.LocalRotationPoint.Z - relativePosition.Y) > MapViewSize )
	            {
	                _baseItems[i].Mesh.Dispose();
                    _baseItems.RemoveAt(i);
	            }   
	        }   
	    }

	    private void UpdateMap()
	    {
	        var relativePosition = RelativeViewerPosition;
            for (var x = -1; x < MapViewSize+1; x++)
            for (var z = -1; z < MapViewSize+1; z++)
            {
                var coords = new Vector2(
                    x - MapViewSize / 2 + relativePosition.X,
                    z - MapViewSize / 2 + relativePosition.Y
                    );
                if (!this.ContainsCoordinates(coords))
                {
                    var newMesh = new ObjectMesh(Vector3.Zero)
                    {
                        LocalRotationPoint = coords.ToVector3()
                    };
                    _baseItems.Add(new MapBaseItem(newMesh));
                    var item = _baseItems[_baseItems.Count - 1];
                    TaskManager.Parallel(delegate
                    {   
                        item.Mesh?.Dispose();
                        item.Mesh = _meshBuilder.BuildMesh(coords);
                        item.Mesh.LocalRotationPoint = coords.ToVector3();
                       /* for (var i = 0; i < MapSize; i++)
                        {
                            for (var j = 0; j < MapSize; j++)
                            {
                                var sample = _builder.SampleChunk(
                                    new Vector2(coords.X, coords.Y) * Chunk.Width * MapSize,
                                    8);
                                if(sample > 0)
                                {
                                    var mapItem = new MapItem(CacheManager.GetModel(CacheItem.AttentionIcon).Clone());
                                    mapItem.Mesh.Scale = Vector3.One * 16f;
                                    mapItem.Position = new Vector3(coords.X, 0, coords.Y) * Chunk.Width * MapSize;
                                    _icons.Add(mapItem);                                                                       
                                }
                            }
                        }*/
                    });
                }
            }
	    }

        private Vector2 RelativeViewerPosition => new Vector2( 
            (int) (_mapInputHandler.Position.X / MapSize / ChunkSize),
            (int) (_mapInputHandler.Position.Z / MapSize / ChunkSize)
            );

	    private bool ContainsCoordinates(Vector2 Coordinates)
	    {
	        for (var i = 0; i < _baseItems.Count; i++)
	        {
	            if (_baseItems[i].Mesh.LocalRotationPoint.Xz == Coordinates) return true;
	        }
	        return false;   
	    }

	    public bool Show{
			get{ return _show; }
			set{
				if(GameManager.IsLoading) return;

			    _mapInputHandler.Enabled = value;
				if(value)
				{
                    _stateManager.CaptureState();
                    this.UpdateChunkBounds();
				    for (var i = 0; i < _baseItems.Count; i++)
				    {
			            _baseItems[i].Mesh.Dispose();
				        _baseItems.RemoveAt(i);			        
				    }
                    this.UpdateMap();
                    this._targetSize = 1.0f;
				    this._player.Movement.Check = false;
                    this._player.View.MaxDistance = 100f;
				    this._player.View.MinDistance = 30f;
                    this._player.View.TargetDistance = 100f;
					this._player.View.MaxPitch = -0.2f;
					this._player.View.MinPitch = -0.8f;
					this._targetHeight = 2500f;
				    this._targetTime = 12000;            
                    SkyManager.PushTime();                   
				}else
				{
                    _stateManager.ReleaseState();
                    this._targetSize = 0f;
					this._targetHeight = 0f;
                    this._targetTime = SkyManager.PeekTime();
                    TaskManager.Delay(() => Math.Abs(_height) < 0.01f, delegate
				    {
				        this._player.View.PositionDelegate = Camera.DefaultDelegate;
				        SkyManager.PopTime();
				        _targetTime = float.MaxValue;
                        _height = 0f;

                    });
				}
				Sound.SoundManager.PlayUISound(Sound.SoundType.OnOff, 1.0f, 0.6f);
				_show = value;
			}
		}
	}
}
