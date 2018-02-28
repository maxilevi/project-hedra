/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/12/2016
 * Time: 10:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
	/// <summary>
	/// Description of FireComponent.
	/// </summary>
	public class BleedComponent : EntityComponent, IEffectComponent
    {
		public int Chance { get; set; } = 20;
        public float TotalStrength { get; set; } = 30;
		public float BaseTime { get; set; } = 5;
		
		private float _cooldown;
		private bool _canBleed = true;
		private float _bleedTime = 0;
		private float _time = 0;
		private float _pTime = 0;
		private Entity _victim;
		private float _oldSpeed = 0;
		
		public BleedComponent(Entity Parent) : base(Parent) {
            Parent.OnAttacking += new OnAttackEventHandler(this.Apply);
		}
		
		public override void Update(){
			_cooldown -= Engine.Time.FrameTimeSeconds;
		}
		
		public void Apply(Entity Victim, float Amount){
			if(_cooldown > 0 || !_canBleed) return;
			
			bool shouldBleed = Utils.Rng.NextFloat() <= Chance * 0.01 ? true : false;
			if(shouldBleed){
				_bleedTime =  BaseTime + Utils.Rng.NextFloat() * 4 -2f;
				if(Victim.SearchComponent<BleedingComponent>() == null){
					Victim.AddComponent(new BleedingComponent(Victim, Parent, _bleedTime, TotalStrength));
				}
			}
		}
	}
}
