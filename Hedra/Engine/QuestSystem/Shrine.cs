/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 18/09/2016
 * Time: 11:56 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using Hedra.Engine.Management;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Shrine.
	/// </summary>
	
	public class Shrine : ClaimableStructure, IUpdatable
	{
		
		
		public Shrine() : base()
		{
			UpdateManager.Add(this);
		}
		
		public void Update(){
			
		}
		
		public override void Claim(LocalPlayer Player){
			base.RaiseOnClaimed(Player);
		}
	}
}
