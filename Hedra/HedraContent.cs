using Hedra.AISystem;
using Hedra.AnimationEvents;
using Hedra.API;

namespace Hedra
{
    public class HedraContent : Mod
    {
        public override string Name => "Project Hedra";
        
        public override void RegisterContent()
        {
            AddAIType("GiantBeetle", typeof(GiantBeetleAIComponent));
            AddAIType("GorillaWarrior", typeof(GorillaWarriorAIComponent));
            AddAIType("Friendly", typeof(FriendlyAIComponent));
            AddAIType("Hostile", typeof(HostileAIComponent));
            AddAIType("Neutral", typeof(NeutralAIComponent));
            AddAIType("Troll", typeof(TrollAIComponent));
            AddAIType("Sheep", typeof(SheepAIComponent));
            AddAIType("Pug", typeof(PugAIComponent));
            AddAIType("Cow", typeof(CowAIComponent));
            AddAIType("Pig", typeof(PigAIComponent));
            AddAIType("Goat", typeof(GoatAIComponent));
            
            AddAnimationEvent("Growl", typeof(Growl));
            AddAnimationEvent("Quake", typeof(Quake));
            AddAnimationEvent("Slash", typeof(Slash));
        }

        public static void Load()
        {
            (new HedraContent()).RegisterContent();
        }
    }
}