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
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Enviroment;
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
        private const int MapSize = 6;
	    private const int ChunkSize = 64;
        private readonly LocalPlayer _player;
	    private readonly MapStateManager _stateManager;
	    private readonly MapItem _cursor;
	    private readonly List<MapItem> _icons;
	    private readonly MapBuilder _builder;
	    private ObjectMesh _baseMesh;
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
            this._baseMesh = new ObjectMesh(Vector3.Zero);
        }

		public void Update(){
		
			if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

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
                _icons[i].Position = new Vector3(_player.Position.X, _height, _player.Position.Z);
		    }
		    _baseMesh.Position = new Vector3(_player.Position.X, _height, _player.Position.Z);
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

	    private void UpdateMap()
	    {
	        _baseMesh?.Dispose();
	        var mapData = new VertexData();
	        for (var x = 0; x < MapSize; x++)
	        {
	            for (var z = 0; z < MapSize; z++)
	            {
	                var chunkMapData = new VertexData();
	                var scale = ChunkSize;
	                for (var i = 0; i < ChunkSize; i+=scale)
	                {
	                    for (var k = 0; k < ChunkSize; k+=scale)
	                    {
	                        var cubeData = new CubeData();
	                        cubeData.Scale(new Vector3(scale, ChunkSize, scale));
	                        cubeData.AddFace(Face.UP);
	                        cubeData.AddFace(Face.FRONT);
	                        cubeData.AddFace(Face.BACK);
	                        cubeData.AddFace(Face.RIGHT);
	                        cubeData.AddFace(Face.LEFT);
	                        var realX = (x - MapSize / 2f) * ChunkSize + i;
	                        var realZ = (z - MapSize / 2f) * ChunkSize + k;
	                        var worldPosition = new Vector3(realX + _player.Position.Z, 0, realZ + _player.Position.Z);
	                        var chunk = World.GetChunkAt(_player.Position + new Vector3(i / (float) scale, 0, k / (float) scale) * Chunk.BlockSize);

	                        if (chunk.Landscape.StructuresPlaced && x == 0 && z == 0)
	                        {
	                            var output = chunk.CreateTerrainMesh(null);
	                            output.StaticData.Transform(-new Vector3(chunk.OffsetX, 0, chunk.OffsetZ));
	                            output.StaticData.Scale(Vector3.One * (1f / 2f));
	                            chunkMapData += output.StaticData;
	                        }
	                        else
	                        {
	                            cubeData.Color =
	                                CubeData.CreateCubeColor(Utils.UniformVariateColor(Color.DodgerBlue.ToVector4(), 25,
	                                    Utils.Rng));
	                            cubeData.TransformVerts(new Vector3(realX, 0, realZ));
	                            chunkMapData += cubeData.ToVertexData();
                            }
	                    }
	                }
	                chunkMapData.Transform( Vector3.UnitY * (-10 + Utils.Rng.NextFloat() * 40f) );
                    mapData += chunkMapData;
                }
	        }
	        for (var i = 0; i < mapData.Vertices.Count; i++)
	        {
                mapData.Vertices[i] = new Vector3(
                    mapData.Vertices[i].X,
                    mapData.Vertices[i].Y + BiomeGenerator.SmallFrequency(mapData.Vertices[i].X, mapData.Vertices[i].Z) * 0f,//-80f,
                    mapData.Vertices[i].Z
                    );
	        }
	        _baseMesh = ObjectMesh.FromVertexData(mapData);
	        _baseMesh.DontCull = true;
	        _baseMesh.ApplyNoiseTexture = true;
	    }

	    public bool Show{
			get{ return _show; }
			set{
				if(GameManager.IsLoading) return;
				
				if(value)
				{
                    _stateManager.CaptureState();
                    this.UpdateMap();
                    this._targetSize = 1.0f;
				    this._player.View.Check = false;
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
