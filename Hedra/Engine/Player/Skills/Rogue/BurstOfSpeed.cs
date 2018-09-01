/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Rogue
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class BurstOfSpeed : CappedSkill
	{
		private const float BaseSpeed = .25f;
		private const float BaseCooldown = 12f;
		private const float CooldownCap = 6f;
		private const float DurationCap = 5f;
		private const float SpeedCap = 1.75f;
		private const float BaseEffectDuration = 2;
		private const float BaseManaCost = 80f;

		public override float MaxCooldown => Math.Max(BaseCooldown - 0.40f * Level, CooldownCap);
		private float Speed => Math.Min(Level * 0.1f + BaseSpeed, SpeedCap);
		private float EffectDuration => Math.Max(BaseEffectDuration + 0.2f * Level, DurationCap);
		public override float ManaCost => BaseManaCost + Level * 5f;
		public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/BurstOfSpeed.png");
		protected override int MaxLevel => 15;
		
		public override void Use()
		{
			Player.AddBonusSpeedForSeconds(Speed, EffectDuration);
		}

		public override string Description => "Temporarily obtain a speed burst.";
	}
}