/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.SkillSystem;
using Hedra.Game;
using Hedra.Localization;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    /// <summary>
    /// Description of TreeBlueprint.
    /// </summary>
    public class AbilityTreeBlueprint
    {
        private readonly string _nameTranslationKey;
        private readonly string _descriptionTranslationKey;
        public string DisplayName => Translations.Get(_nameTranslationKey);
        public string Description => Translations.Get(_descriptionTranslationKey);
        public bool IsSpecialization => _descriptionTranslationKey != null;
        public uint Icon { get; set; }
        public string Identifier => _nameTranslationKey;
        public readonly TreeItem[][] Items = new TreeItem[AbilityTree.Columns][];

        public AbilityTreeBlueprint(AbilityTreeTemplate AbilityTreeTemplate)
        {
            _nameTranslationKey = AbilityTreeTemplate.Name.ToLowerInvariant();
            _descriptionTranslationKey = AbilityTreeTemplate.Description?.ToLowerInvariant();
            for(var i = 0; i < Items.Length; i++)
            {
                Items[i] = new TreeItem[AbilityTree.Rows];
                for(var j = 0; j < Items[i].Length; j++)
                {
                    var skill = AbilityTreeTemplate.Get(i, j);
                    Items[i][j] = skill != null ? new TreeItem
                    {
                        AbilityType = SkillFactory.Instance.Get(skill),
                        Image = GameManager.Player.Toolbar.Skills.First(S => S.GetType() == SkillFactory.Instance.Get(skill)).IconId,
                    } : new TreeItem();
                }
            }
        }

        public bool Has(Type Skill)
        {
            return Items.Any(I => I.Any(T => T.AbilityType == Skill));
        }
    }
}
