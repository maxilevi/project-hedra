/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/01/2017
 * Time: 01:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Lamp.
	/// </summary>
	public class HandLamp
	{
		public Humanoid Human { get; set; }
        public PointLight Light { get; set; }
        public float LightModifier { get; set; }
        public Vector3 LightColor { get; set; } = new Vector3(1,.6f,.5f);
	    private bool _enabled;

        public HandLamp(Humanoid Human){
			this.Human = Human;
		}
		
		public void Update(){
		    if (Light == null || (Light.Position == Human.Position && Light.Color == LightColor * LightModifier)) return;
		    Light.Position = Human.Position;
		    Light.Color = LightColor * LightModifier;
		    Light.Radius = PointLight.DefaultRadius * 4f;
		    ShaderManager.UpdateLight(Light);
		}
		
		public bool Enabled{
			get{ return _enabled;}
			set{
				_enabled = value;
				if(value && Light == null) Light = ShaderManager.GetAvailableLight();
				LightModifier =  Enabled ? 1 : 0;
				Human.Model.SetLamp(Enabled);
			}
		}
		
		public void Dispose(){
			this.Enabled = false;
			if(Light != null)
				Light.Locked = false;
		}
	}
}
