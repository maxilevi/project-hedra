/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/09/2016
 * Time: 12:00 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of ClaimableStrucuture.
	/// </summary>
	
	public delegate void OnInteraction(LocalPlayer Player);	
	
	public abstract class ClaimableStructure : BaseStructure
	{
		public int ClaimDistance { get; set; } = 32;
		public event OnInteraction OnClaimedEvent;
	    protected bool Claimed;

		public abstract void Claim(LocalPlayer Player);
		
		protected void RaiseOnClaimed(LocalPlayer Player)
		{
		    OnClaimedEvent?.Invoke(Player);
		}
	}
}
