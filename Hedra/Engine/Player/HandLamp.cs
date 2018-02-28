/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/01/2017
 * Time: 01:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Lamp.
	/// </summary>
	public class HandLamp
	{
		public Humanoid Human;
		public PointLight Light;
		public float LightModifier = 0;
		public Vector3 LightColor = new Vector3(1,.6f,.5f);
		
		public HandLamp(Humanoid Human){
			this.Human = Human;
		}
		
		public void Update(){
			if(Light != null && (Light.Position != Human.Position || Light.Color != LightColor * LightModifier) ){
				Light.Position = Human.Position;
				Light.Color = LightColor * LightModifier;
				ShaderManager.UpdateLight(Light);
			}
		}
		
		private bool m_Enabled = false;
		public bool Enabled{
			get{ return m_Enabled;}
			set{
				m_Enabled = value;
				//Lazy init
				if(value && Light == null) Light = ShaderManager.GetAvailableLight();
				LightModifier =  (Enabled) ? 1 : 0;
				Human.Model.SetLamp(Enabled);
			}
		}
		
		public void Dispose(){
			//Free the light!
			this.Enabled = false;
			if(Light != null)
				Light.Locked = false;
		}
	}
}
