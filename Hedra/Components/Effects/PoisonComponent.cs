/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Components.Effects
{
    /// <summary>
    /// Description of BurningComponent.
    /// </summary>
    public class PoisonComponent : EntityComponent
    {
        private float TotalTime, Time, TotalDamage;
        private int PTime;
        private IEntity Damager = null;
        
        public PoisonComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent)
        {
            this.TotalTime = TotalTime;
            this.TotalDamage = TotalDamage;
            this.Damager = Damager;
            RoutineManager.StartRoutine(UpdatePoison);
        }
        
        public override void Update(){}

        private IEnumerator UpdatePoison()
        {
            Parent.Model.BaseTint = Colors.PoisonGreen *new Vector4(1,3,1,1);
            while(TotalTime > PTime && !Parent.IsDead && !Disposed){
                
                Time += Core.Time.DeltaTime;
                if(Time >= 1){
                    PTime++;
                    Time = 0;
                    Parent.Damage(TotalDamage / TotalTime, Damager, out float Exp);
                    if(Damager is Humanoid humanoid)
                        humanoid.XP += Exp;
                }
                
                yield return null;
            }
            Parent.Model.BaseTint = Vector4.Zero;
            this.Dispose();
            Parent.RemoveComponent(this);
        }
    }
}
