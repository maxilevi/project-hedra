/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 11/05/2016
 * Time: 07:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Item
{
	/// <summary>
	/// Description of MaterialInfo.
	/// </summary>
	public struct MaterialInfo
	{
		public float AttackPower;
		public float AttackSpeed;
		public float MovementSpeed;
		public EffectType Effect;
		
		public MaterialInfo(float AP, float AS, float MS, EffectType Effect){
			this.AttackPower = AP;
			this.AttackSpeed = AS;
			this.MovementSpeed = MS;
			this.Effect = Effect;
		}
	}
}
