/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class TripleShuriken : Shuriken
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TripleShuriken.png");

        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
        }

        public override string Description => "Throw your a series of shurikens at your foes.";    
        public override string DisplayName => "Triple Shuriken";
    }
}