/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class TripleShuriken : Shuriken
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TripleShuriken.png");
        protected override float AnimationSpeed => 1.75f;
        private int _times;

        protected override void OnEnable()
        {
            _times = 0;
        }

        protected override void OnDisable()
        {
            var currentTimes = _times;
            if(_times < 2)
                DoUse();
            _times = ++currentTimes;
        }

        public override string Description => Translations.Get("triple_shuriken_desc");    
        public override string DisplayName => Translations.Get("triple_shuriken_skill");
    }
}