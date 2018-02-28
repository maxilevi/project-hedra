/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 07:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of IInteractable.
	/// </summary>
	public interface IClaimable
	{
		void Interact(LocalPlayer Player);
	}
}
