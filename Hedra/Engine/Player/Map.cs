/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 03:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Enviroment;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Map.
	/// </summary>
	public class Map
	{
		private readonly LocalPlayer _player;
		private readonly bool Enabled = true;
        public const int HorizontalScale = 16;
	    public const int VerticalScale = 4;
	    public const int MapPieceWidth = 80;
		private bool _show;
		private bool _isUpdating;
		private Timer _updateTimer = new Timer(16f);
        private readonly MapPiece _mainPiece;
		private readonly ObjectMesh _questIcon;
	    private readonly ObjectMesh _cursor;
	    private float _size;
	    private float _targetSize;
	    private int _previousSeed;
		public Vector3 Offset;
	    private Vector3 _targetOffset;
	    private float _swordY;
		public Vector3 MapPosition;
	    private float _targetTime = float.MaxValue;
	    private Vector2 _prevDist;
	    public Vector3 LastUpdated;

        public Map(LocalPlayer Player){
		    Chunk emptyChunk;
			this._player = Player;
			
			this._questIcon = ObjectMesh.FromVertexData(AssetManager.PlyLoader("Assets/Env/QuestIcon.ply", Vector3.One * 20f));
			DrawManager.Remove(this._questIcon);
			this._cursor = ObjectMesh.FromVertexData(AssetManager.PlyLoader("Assets/UI/MapCursor.ply", Vector3.One * 15f));
			DrawManager.Remove(this._cursor);

            _mainPiece = new MapPiece
		    {
                Indexes = new Vector2(0, 0),
		        Chunk = new Chunk(0, 0)
		    };

            _mainPiece.Chunk.Initialize();

		    var meshBuffers = new ChunkMeshBuffer[2];
		    meshBuffers[(int) ChunkBufferTypes.STATIC] = new StaticMeshBuffer();
		    meshBuffers[(int) ChunkBufferTypes.WATER] = new WaterMeshBuffer();

            _mainPiece.Chunk.Mesh = new ChunkMesh(_mainPiece.Chunk.Position, meshBuffers, 1)
		    {
		        Enabled = true,
		        IsGenerated = true,
		        IsBuilded = true
		    };
		    _mainPiece.Chunk.IsGenerated = true;
            _mainPiece.Chunk.Landscape.BlocksSetted = true;
            _mainPiece.Chunk.Landscape.StructuresPlaced = true;

            TaskManager.Parallel(delegate
            {
                _mainPiece.Chunk.Blocks = new Block[MapPieceWidth][][];
                for (var i = 0; i < MapPieceWidth; i++)
                {
                    _mainPiece.Chunk.Blocks[i] = new Block[Chunk.ChunkHeight / VerticalScale][];
                    for (var j = 0; j < Chunk.ChunkHeight / VerticalScale; j++)
                    {
                        _mainPiece.Chunk.Blocks[i][j] = new Block[MapPieceWidth];
                        for (var k = 0; k < MapPieceWidth; k++)
                        {
                            _mainPiece.Chunk.Blocks[i][j][k] = this.GetDefaultBlock(i, j, k);
                        }
                    }
                }
                _mainPiece.Chunk.BuildMesh(_mainPiece.Chunk.Neighbours, false);
            });
        }

		public void Update(){
		
			if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

		    var underChunk = World.GetChunkAt(_player.Position);
		    if (underChunk != null && World.Seed != World.MenuSeed)
		    {
		        if ((LastUpdated - _player.Model.Position).LengthSquared > 48 * Chunk.BlockSize * 48 * Chunk.BlockSize)
		        {
		            LastUpdated = _player.Position;
                    //TaskManager.Parallel(this.UpdateMap);
		        }
		    }

		    if (float.MaxValue != _targetTime)
		    {
		        SkyManager.SetTime(Mathf.Lerp(SkyManager.DayTime, _targetTime, (float) Time.deltaTime * 2f));
		        if (Math.Abs(SkyManager.DayTime - _targetTime) < 10)
		        {
		            if (SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
		        }
		    }
		    if (Show)
		    {
		        SkyManager.FogManager.UpdateFogSettings(MapPieceWidth * 1.75f, MapPieceWidth * 2.0f);
            }
		    _size = Mathf.Lerp(_size, _targetSize, Time.unScaledDeltaTime * 4f);
			_questIcon.Scale = Vector3.One * _size;
			_cursor.Scale = Vector3.One * _size;
			
			if(Show){
                this._player.View.PositionDelegate = () => _player.Model.Model.Position + Offset;
            }
			this.Offset = Mathf.Lerp(Offset, _targetOffset,  Time.unScaledDeltaTime * 4f);
			
			this.MapPosition = _player.Position + Offset + Vector3.UnitY * 2f;
			this._cursor.Position = _player.Model.Position + Offset + _mainPiece.Chunk.GetHighestY(HorizontalScale/2, HorizontalScale/2) / HorizontalScale * Vector3.UnitY;
			
			if( World.QuestManager.Quest.IconPosition.X < (_player.Position + Vector3.UnitX * 0 * Chunk.ChunkWidth).X &&
			    World.QuestManager.Quest.IconPosition.Z < (_player.Position + Vector3.UnitZ * 0 * Chunk.ChunkWidth).Z &&
			    World.QuestManager.Quest.IconPosition.X > (_player.Position - Vector3.UnitX * 0 * Chunk.ChunkWidth).X &&
			    World.QuestManager.Quest.IconPosition.Z > (_player.Position - Vector3.UnitZ * 0 * Chunk.ChunkWidth).Z){
				
				Vector3 objPosition = World.QuestManager.Quest.IconPosition.Xz.ToVector3();
				this._questIcon.Position = _player.Position + Vector3.UnitY * 22f + Offset + (objPosition - _player.Position) / Chunk.ChunkWidth * HorizontalScale;
			}else{
				Vector3 direction = (World.QuestManager.Quest.IconPosition - _player.Position).NormalizedFast();
				this._questIcon.Position = _player.Position + Vector3.UnitY * 14f + Offset + direction.Xz.ToVector3() * 8f;
			}
			this._cursor.TargetRotation = new Vector3(0,this._player.Model.TargetRotation.Y,0);
			this._questIcon.Rotation = Vector3.UnitY * _swordY;
			_swordY += (float) Time.deltaTime * 40f;
		}
		
		public void Draw(){
		
			if(_size < 0.01f) return;

		    if (Math.Abs(_size - 1.0f) < 0.15f)
		    {
		        this.DrawMesh(_size, Vector3.Zero, MapPosition, Vector3.Zero);
		    }

		    this._questIcon.Mesh.MeshBuffers[0].Bind();
			GL.Uniform3(ObjectMeshBuffer.Shader.LightColorLocation, Vector3.One);
			
			//this._questIcon.Draw();
			this._cursor.Draw();

			GL.Uniform3(ObjectMeshBuffer.Shader.LightColorLocation, ShaderManager.LightColor);
			this._questIcon.Mesh.MeshBuffers[0].UnBind();			
		}
		
		public void DrawMesh(float Size, Vector3 Rotation, Vector3 Position, Vector3 PositionOffset){
            GraphicsLayer.PushMatrix();
            GraphicsLayer.Translate(Position);
            GraphicsLayer.Translate(Vector3.UnitY * -2f);
		    GraphicsLayer.Rotate(Rotation.Y, Vector3.UnitY);
            GraphicsLayer.Translate(-2f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f) + PositionOffset);
		    GraphicsLayer.Scale(new Vector3(1f, 1f, 1f) * .5f * Size);

		    _mainPiece.Chunk.Mesh.MeshBuffers[(int)ChunkBufferTypes.STATIC].Bind();

		    GL.Uniform3(BlockShaders.StaticShader.LightColorLocation, Vector3.One);
		    GL.Uniform3(BlockShaders.StaticShader.PlayerPositionUniform, Vector3.UnitY * -1000000f + 4f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f));
		    _mainPiece.Chunk.Mesh.Draw(ChunkBufferTypes.STATIC, Vector3.Zero, false);

		    GraphicsLayer.PopMatrix();

            GraphicsLayer.PushMatrix();
		    GraphicsLayer.Translate(Position);
		    GraphicsLayer.Rotate(Rotation.Y, Vector3.UnitY);
		    GraphicsLayer.Translate(-2f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f) + PositionOffset);
            GraphicsLayer.Scale(new Vector3(1f, 1f, 1f) * .5f * Size);

            _mainPiece.Chunk.Mesh.MeshBuffers[(int)ChunkBufferTypes.WATER].Bind();

		    GL.Uniform3(BlockShaders.WaterShader.LightColorLocation, Vector3.One);
		    GL.Uniform3(BlockShaders.WaterShader.PlayerPositionUniform, Vector3.UnitY * -1000000f + 4f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f));
		    _mainPiece.Chunk.Mesh.Draw(ChunkBufferTypes.WATER, Vector3.Zero, false);

		    _mainPiece.Chunk.Mesh.MeshBuffers[(int)ChunkBufferTypes.WATER].UnBind();
		    GraphicsLayer.PopMatrix();
        }
		
		private void UpdateMap(){

            for (var i = 0; i < MapPieceWidth; i++)
		    {
		        for (var j = 0; j < Chunk.ChunkHeight / VerticalScale; j++)
		        {
		            for (var k = 0; k < MapPieceWidth; k++)
		            {
		                _mainPiece.Chunk.Blocks[i][j][k] = this.GetDefaultBlock(i, j, k);
		            }
		        }
		    }

            var instanceElements = new List<InstanceData>();
		    var elements = new List<VertexData>();
            for (int i = World.Chunks.Count-1; i > -1; i--)
            {
                this.SetChunkData(_mainPiece, World.Chunks[i], instanceElements, elements);
            }
		    _mainPiece.Chunk.StaticBuffer.InstanceElements = instanceElements;
		    _mainPiece.Chunk.StaticBuffer.Elements = elements;

            _mainPiece.Chunk.BuildMesh(_mainPiece.Chunk.Neighbours, false);    
        }

        private void SetChunkData(MapPiece Slot, Chunk Data, List<InstanceData> InstanceElements, List<VertexData> Elements)
        {
            var playerChunk = World.GetChunkAt(_player.Position);
            if(playerChunk == null) return;

            var realOffset = new Vector2(
                (Data.OffsetX - playerChunk.OffsetX) / Chunk.BlockSize / HorizontalScale - MapPieceWidth * Slot.Indexes.X + MapPieceWidth * .5f,
                (Data.OffsetZ - playerChunk.OffsetZ) / Chunk.BlockSize / HorizontalScale - MapPieceWidth * Slot.Indexes.Y + MapPieceWidth * .5f
                );

            if (Data.Landscape == null || !Data.Landscape.BlocksSetted || realOffset.X < 0 || realOffset.X >= MapPieceWidth || realOffset.Y < 0 ||
                realOffset.Y >= MapPieceWidth) return;
            

            var realSize = Chunk.ChunkWidth / Chunk.BlockSize;
            for (var x = 0; x < realSize; x+=HorizontalScale)
            {
                for (var y = 0; y < Chunk.ChunkHeight; y+= VerticalScale)
                {
                    for (var z = 0; z < realSize; z+=HorizontalScale)
                    {
                        if(Data.Blocks[x][y][z].Type == BlockType.Water) continue;
                        Slot.Chunk.Blocks
                            [x / HorizontalScale + (int)realOffset.X]
                            [y / VerticalScale]
                            [z / HorizontalScale + (int)realOffset.Y].Type = Data.Blocks[x][y][z].Type;

                        Slot.Chunk.Blocks
                            [x / HorizontalScale + (int)realOffset.X]
                            [y / VerticalScale]
                            [z / HorizontalScale + (int)realOffset.Y].Density = Data.Blocks[x][y][z].Density;
                        Slot.Chunk.Blocks
                            [x / HorizontalScale + (int)realOffset.X]
                            [y / VerticalScale]
                            [z / HorizontalScale + (int)realOffset.Y].Noise3D = Data.Blocks[x][y][z].Noise3D;
                    }
                }
            }
            Slot.Chunk.Biome = Data.Biome;

            for (int i = 0; i < Data.StaticBuffer.InstanceElements.Count; i++)
            {
                var data = Data.StaticBuffer.InstanceElements[i];

                if (data.MeshCache == CacheManager.GetModel(CacheItem.Wheat)) continue;
                if (data.MeshCache == CacheManager.GetModel(CacheItem.Grass)) continue;
                if (data.MeshCache == CacheManager.GetModel(CacheItem.Cloud)) continue;

                if(Utils.Rng.Next(0, 3) != 1) return;

                var newData = new InstanceData
                {
                    ColorCache = data.ColorCache,
                    Colors = data.Colors,
                    MeshCache = data.MeshCache,
                    ExtraData = data.ExtraData,
                    ExtraDataCache = data.ExtraDataCache
                };

                Quaternion quat = data.TransMatrix.ExtractRotation();

                Matrix4 newMatrix = Matrix4.CreateScale(data.TransMatrix.ExtractScale() * .25f * .5f);
                newMatrix *= Matrix4.CreateFromQuaternion(quat);
                Vector3 transPosition = data.TransMatrix.ExtractTranslation();
                var newPosition = new Vector3( 
                    (transPosition.X - playerChunk.OffsetX) / HorizontalScale,
                    transPosition.Y / VerticalScale,
                    (transPosition.Z - playerChunk.OffsetZ) / HorizontalScale);
                newMatrix *= Matrix4.CreateTranslation(newPosition + 4f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f));

                newData.TransMatrix = newMatrix;
                InstanceElements.Add(newData);
            }

            for (var i = 0; i < Data.StaticBuffer.Elements.Count; i++)
            {
                var isCloud = true;
                if (Data.StaticBuffer.Elements[i] == null) continue;
                
				try{
					Data.StaticBuffer.Elements[i].Colors.ForEach( Color => { if(Color != Vector4.One) isCloud = false;} );
				}catch(Exception e){
					//It's just easier to let it implode silently
				}
				if(isCloud) continue;
				
                VertexData New = Data.StaticBuffer.Elements[i].Clone();

                New.Transform(Matrix4.CreateTranslation(-playerChunk.Position));
                New.Scale( Vector3.One * .5f);
                //New.Transform(Matrix4.CreateScale(new Vector3(1f / HorizontalScale, 1f / HorizontalScale, 1f / HorizontalScale)));
                New.Transform(Matrix4.CreateTranslation(4f * new Vector3(MapPieceWidth * .5f, 0, MapPieceWidth * .5f)));
                New.Scale(Vector3.One * .5f);

                Elements.Add(New);
            }
        }

	    public Block GetDefaultBlock(int i, int j, int k)
	    {
            if(j < 3)
	            return new Block(BlockType.Seafloor, .5f);

            if (j < 5)
                return new Block(BlockType.Water);

            return new Block(BlockType.Air);
	    }

        public bool Show{
			get{ return _show; }
			set{
				if(Scenes.SceneManager.Game.IsLoading || !Enabled)
					return;
				
				if(value)
				{
				    this._prevDist = new Vector2(SkyManager.FogManager.MinDistance, SkyManager.FogManager.MaxDistance);
                    TaskManager.Parallel(this.UpdateMap);
				    LastUpdated = _player.Position;
                    this._targetSize = 1.0f;
					this._player.View.MaxDistance = 100f;
				    this._player.View.MinDistance = 30f;
                    this._player.View.TargetDistance = 100f;
					this._player.View.MaxPitch = -0.2f;
					this._player.View.MinPitch = -0.8f;
					this._targetOffset = 2500f * Vector3.UnitY;
				    this._targetTime = 12000;            
                    SkyManager.PushTime();
				}else
				{
				    TaskManager.Delay(300, () =>
				        SkyManager.FogManager.UpdateFogSettings(_prevDist.X, _prevDist.Y));

					this._player.View.MaxDistance = Camera.DefaultMaxDistance;
				    this._player.View.MinDistance = Camera.DefaultMinDistance;
                    this._targetSize = 0f;
					this._targetOffset = Vector3.UnitY * 0f;
					this._player.View.MinPitch = Camera.DefaultMinPitch;
					this._player.View.MaxPitch = Camera.DefaultMaxPitch;
				    this._targetTime = SkyManager.PeekTime();
                    TaskManager.Delay(() => Offset.LengthFast < 0.01f, delegate
				    {
				        this._player.View.PositionDelegate = Camera.DefaultDelegate;
				        SkyManager.PopTime();
				        _targetTime = float.MaxValue;
				        Offset = Vector3.Zero;

                    });
				}
				Sound.SoundManager.PlayUISound(Sound.SoundType.OnOff, 1.0f, 0.6f);
				_show = value;
			}
		}
	}
	
	public class MapPiece{
		public Chunk Chunk;
		public Vector2 Indexes = Vector2.Zero;
	}
}
