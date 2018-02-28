/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 04/09/2016
 * Time: 06:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of SpiderBossAIComponent.
	/// </summary>
	public class SpiderBossAIComponent : BossAIComponent
	{
		public Action AttackMode;
		public SpiderBossAIComponent(Entity Parent) : base (Parent){
			AttackMode = () => NoAbility();
			base.AILogic = delegate{};
			
		}
		
		public override void LateUpdate(){
			
			this.AttackMode();
		}	
	}
}
