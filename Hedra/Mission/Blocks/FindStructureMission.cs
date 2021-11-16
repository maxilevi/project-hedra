using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.StructureSystem;
using Hedra.Localization;
using Hedra.Numerics;

namespace Hedra.Mission.Blocks
{
    public class FindStructureMission : MissionBlock
    {
        private string _missionDescription;

        public override bool IsCompleted => (Position - Owner.Position).Xz().LengthSquared() <
                                            Math.Pow(Design.PlateauRadius * DistanceMultiplier, 2);

        public CacheItem DefaultIcon { get; set; }
        public IFindableStructureDesign Design { get; set; }
        public Vector3 Position { get; set; }
        public float DistanceMultiplier { get; set; } = 1;
        public override bool HasLocation => true;
        public override Vector3 Location => Position;
        public override string ShortDescription => Translations.Get("quest_find_structure_short", Design.DisplayName);

        public override string Description => _missionDescription ??
                                              Translations.Get("quest_find_structure_description", Giver.Name,
                                                  Design.DisplayName);

        public override DialogObject DefaultOpeningDialog => new DialogObject
        {
            Keyword = "quest_find_structure_dialog",
            Arguments = new object[]
            {
                Design.DisplayName
            }
        };

        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new ModelView(
                (Design.Icon ?? CacheManager.GetModel(DefaultIcon)).Clone().Scale(Vector3.One)
            );
        }

        public void SetDescription(string New)
        {
            _missionDescription = New;
        }
    }
}