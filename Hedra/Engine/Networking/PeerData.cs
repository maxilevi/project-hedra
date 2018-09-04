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
		
		public static Humanoid NewHuman(IPEndPoint IP)
		{
			return null;
		}
	}
}
