/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/02/2017
 * Time: 03:29 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player.MapSystem
{
	/// <summary>
	/// Description of Minimap.
	/// </summary>
	public class Minimap : IRenderable
	{
		private readonly LocalPlayer _player;
		private readonly Panel _panel;
	    private bool _show;

        public Minimap(LocalPlayer Player){
			this._player = Player;
            this._panel = new Panel();

		}
		
		public void Update(){
			if (!Show) return;
		}
		
		public void DrawMap(){			

		}

		public void Draw(){

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