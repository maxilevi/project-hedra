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
using Hedra.Core;
using Hedra.Engine.Localization;
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
        protected override float AnimationSpeed => 1.75f;
        private int _times;

        public override void Use()
        {
            _times = 0;
            base.Use();
        }

        protected override void OnDisable()
        {
            var currentTimes = _times;
            if(_times < 2)
                Use();
            _times = ++currentTimes;
        }

        private void Shoot()
        {
            var dir = Player.View.LookingDirection;
            ShootShuriken(dir);
            TaskScheduler.After(.1f, () =>
            {
            //    ShootShuriken(dir);
            });         
            TaskScheduler.After(.2f, () =>
            {
            //    ShootShuriken( dir);
            });
        }

        public override string Description => Translations.Get("triple_shuriken_desc");    
        public override string DisplayName => Translations.Get("triple_shuriken_skill");
    }
}