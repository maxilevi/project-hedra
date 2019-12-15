using System;
using System.Linq;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class CollectMission : ItemMission
    {
        private string _newDescription;
        private string _newShortDescription;
        
        public override void Setup()
        {
        }

        public override string Description =>
            _newDescription ?? Translations.Get("quest_collect_description", Giver.Name, Items.Select(I => I.ToString(Owner)).Aggregate((S1, S2) => $"{S1}{Environment.NewLine}{S2}"));
        public override string ShortDescription => 
            _newShortDescription ?? Translations.Get("quest_collect_short", Giver.Name, Items.Select(I => I.ToString()).Aggregate((S1, S2) => $"{S1}{S2}"));

        public void SetDescription(string Text)
        {
            _newDescription = Text;
        }

        public void SetShortDescription(string Text)
        {
            _newShortDescription = Text;
        }
        
        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_collect_dialog",
            Arguments = new object[]
            {
                Items.Select(I => I.ToString()).Aggregate((S1, S2) => $"{S1}, {S2}").ToUpperInvariant()
            }
        };
    }
}