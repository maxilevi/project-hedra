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
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Rogue
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class Fade : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Fade.png");

        public Fade() : base() 
        {
            base.ManaCost = 80f;
            base.MaxCooldown = 16f;
        }
        
        public override void Use(){
            base.MaxCooldown = 20f;
            RoutineManager.StartRoutine(FadeTime);
        }
        
        private IEnumerator FadeTime(){
            float PassedTime = 8f + Math.Min(base.Level * .75f, 10f), PTime = 0;
            SoundPlayer.PlayUISound(SoundType.DarkSound, 1f, .25f);
            while(PTime < PassedTime){
                
                Player.Model.Alpha = .4f;
                Player.Model.BaseTint = -new Vector4(.8f,.8f,.8f,0);
                Player.IsInvisible = true;
                
                if(Player.IsAttacking || Player.IsCasting){
                    Player.Model.Alpha = 1;
                    Player.Model.BaseTint = Vector4.Zero;
                    Player.IsInvisible = false;
                    yield break;
                }
                
                PTime += Engine.Time.DeltaTime;
                yield return null;
            }
            
            Player.Model.Alpha = 1;
            Player.Model.BaseTint = Vector4.Zero;
            Player.IsInvisible = false;
        }

        public override void Update()
        {
            
        }
        
        public override string Description => "Temporarily hide from your enemies.";
        public override string DisplayName => "Fade";
    }
}