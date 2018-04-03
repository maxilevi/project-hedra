/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/02/2017
 * Time: 12:47 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Net;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of PeerData.
	/// </summary>
	public class PeerData
	{
		public Humanoid Human;
		public bool Packet0x2Sent = false;
		public bool Packet0x6Sent = false;
		public float LastPing = 0;
		
		public static Humanoid NewHuman(IPEndPoint IP){
			var Human = new Humanoid();
			Human.Model = new HumanModel(Human);
			Human.Removable = false;
			Human.Model.Fog = true;
			
			var Dmg = Human.SearchComponent<DamageComponent>();
			if(NetworkManager.IsHost){
				Dmg.OnDamageEvent += delegate(DamageEventArgs Args) {
					if(Args.Damager == LocalPlayer.Instance){
					//Reverse the gui and the dmg
					Args.Victim.Health += Args.Amount;
					for(int i = Dmg.DamageLabels.Count-1; i > -1; i--){
						if(Dmg.DamageLabels[i].Texture is GUIText){
							if( (Dmg.DamageLabels[i].Texture as GUIText).Text == ((int)Args.Amount).ToString() ){
								Dmg.DamageLabels[i].Dispose();
								Dmg.DamageLabels.RemoveAt(i);
							}
						}
					}
				}
					if(Args.Damager != LocalPlayer.Instance)
						NetworkManager.SendPacket0x15(Args.Amount, IP);
				};
			}else{
				Dmg.Immune = true;
			}
			Log.WriteLine(Human.Physics.BaseHeight);
			//Human.Physics.UsePhysics = false;
			
			World.AddEntity(Human);
			return Human;
		}
	}
}
