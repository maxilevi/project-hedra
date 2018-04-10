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
using System.Windows.Forms;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player.MapSystem
{
	/// <summary>
	/// Description of Map.
	/// </summary>
	public class Map
	{
        private const int ViewSize = 100;
        private readonly LocalPlayer _player;
	    private readonly MapStateManager _stateManager;
	    private readonly MapItem _cursor;
	    private readonly List<MapItem> _icons;
	    private readonly MapBuilder _builder;
		private bool _show;
	    private float _size;
	    private float _targetSize;
	    private int _previousSeed;
		private Vector3 _offset;
	    private Vector3 _targetOffset;
	    private float _swordY;
	    private float _targetTime = float.MaxValue;

        public Map(LocalPlayer Player){
			this._player = Player;
            this._icons = new List<MapItem>();
            this._stateManager = new MapStateManager(Player);
            this._builder = new MapBuilder();
            this._cursor = new MapItem(AssetManager.PlyLoader("Assets/UI/MapCursor.ply", Vector3.One * 15f));
        }

		public void Update(){
		
			if(World.Seed == World.MenuSeed && this.Show) this.Show = false;

		    this._size = Mathf.Lerp(_size, _targetSize, Time.unScaledDeltaTime * 4f);
		    this._offset = Mathf.Lerp(_offset, _targetOffset, Time.unScaledDeltaTime * 4f);
		    this.UpdateFogAndTime();	

		    for (var i = 0; i < _icons.Count; i++)
		    {
		        _icons[i].Rotation = new Vector3(
                    _icons[i].Rotation.X,
                    _icons[i].Rotation.Y + (float)Time.deltaTime * 40f,
                    _icons[i].Rotation.Z
                    );
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
	        this._player.View.PositionDelegate = () => _player.Model.Model.Position + _offset;
	        SkyManager.FogManager.UpdateFogSettings(ViewSize * 1.75f, ViewSize * 2.0f);
	    }

	    public void Draw()
	    {

	        if (_size < 0.01f) return;
	        for (var i = 0; i < _icons.Count; i++)
	        {
	            _icons[i].Draw();
	        }
	    }

	    public bool Show{
			get{ return _show; }
			set{
				if(GameManager.IsLoading) return;
				
				if(value)
				{
                    _stateManager.CaptureState();
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
                    _stateManager.ReleaseState();
                    this._targetSize = 0f;
					this._targetOffset = Vector3.UnitY * 0f;
				    this._targetTime = SkyManager.PeekTime();
                    TaskManager.Delay(() => _offset.LengthFast < 0.01f, delegate
				    {
				        this._player.View.PositionDelegate = Camera.DefaultDelegate;
				        SkyManager.PopTime();
				        _targetTime = float.MaxValue;
				        _offset = Vector3.Zero;

                    });
				}
				Sound.SoundManager.PlayUISound(Sound.SoundType.OnOff, 1.0f, 0.6f);
				_show = value;
			}
		}
	}
}
