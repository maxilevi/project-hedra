/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/09/2017
 * Time: 02:43 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of DropShadow.
	/// </summary>
	public class DropShadow
	{
		public Vector3 Scale {get; set;}
		public bool Enabled {get; set;}
		public bool DepthTest {get; set;}
		public Func<bool> DeleteWhen {get; set;}
		public float Opacity {get; set;}
		public Matrix3 Rotation {get; set;}
	    public bool Moved { get; private set; }
        public bool IsCosmeticShadow { get; set; }

	    private Vector3 _position;
	    public Vector3 Position
	    {
	        get => _position;
	        set
	        {
	            _position = value;
	            Moved = true;
	        }
	    }

        public DropShadow(){
			this.Opacity = 1f;
			this.Enabled = true;
			this.Scale = Vector3.One * 2f;
			this.Rotation = Matrix3.Identity;
			DrawManager.DropShadows.Add(this);
		}
		
		public bool ShouldDraw => (GameManager.Player.Position - Position).Xz.LengthSquared < 128*128 && Enabled;
	}
}
