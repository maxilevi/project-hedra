using System;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Mission.Blocks
{
    public class FindStructureMission : MissionBlock
    {
        private string _missionDescription;
        public override bool IsCompleted => (Position - Owner.Position).Xz().LengthSquared() <
                                            Math.Pow(Design.PlateauRadius, 2);
        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new ModelView(
                (Design.Icon ?? CacheManager.GetModel(DefaultIcon)).Clone().Scale(Vector3.One)
            );
        }

        public CacheItem DefaultIcon { get; set; }
        public IFindableStructureDesign Design { get; set; }
        public Vector3 Position { get; set; }
        public override bool HasLocation => true;
        public override Vector3 Location => Position;
        public override string ShortDescription => Translations.Get("quest_find_structure_short", Design.DisplayName);
        public override string Description => _missionDescription ?? Translations.Get("quest_find_structure_description", Giver.Name, Design.DisplayName);

        public void SetDescription(string New)
        {
            _missionDescription = New;
        }
        
        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_find_structure_dialog",
            Arguments = new object[]
            {
                Design.DisplayName
            }
        };
    }
}