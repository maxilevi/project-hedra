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
	internal class BleedComponent : EntityComponent, IEffectComponent
    {
		public int Chance { get; set; } = 10;
        public float Damage { get; set; } = 30;
		public float Duration { get; set; } = 5;		
		
		public BleedComponent(Entity Parent) : base(Parent) {
            Parent.OnAttacking += this.Apply;
		}
        		
		public override void Update(){}

        public void Apply(Entity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() <= Chance * 0.01)
            {
                if (Victim.SearchComponent<BleedingComponent>() == null)
                    Victim.AddComponent(new BleedingComponent(Victim, Parent, Duration + Utils.Rng.NextFloat() * 4 - 2f, Damage));
            }
        }

        public override void Dispose()
        {
            Parent.OnAttacking -= this.Apply;
        }
    }
}
