using Hedra.AISystem;
using Hedra.AISystem.Mob;
using Hedra.AnimationEvents;
using Hedra.AnimationEvents.SkillEvents;
using Hedra.API;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.SkillSystem.Archer;
using Hedra.Engine.SkillSystem.Archer.Hunter;
using Hedra.Engine.SkillSystem.Archer.Scout;
using Hedra.Engine.SkillSystem.Mage;
using Hedra.Engine.SkillSystem.Mage.Druid;
using Hedra.Engine.SkillSystem.Mage.Necromancer;
using Hedra.Engine.SkillSystem.Rogue;
using Hedra.Engine.SkillSystem.Rogue.Assassin;
using Hedra.Engine.SkillSystem.Rogue.Ninja;
using Hedra.Engine.SkillSystem.Warrior;
using Hedra.Engine.SkillSystem.Warrior.Berserker;
using Hedra.Engine.SkillSystem.Warrior.Paladin;
using Hedra.Items;
using Hedra.ModelHandlers;
using Hedra.Sound;
using Hedra.WeaponSystem;

namespace Hedra
{
    public class HedraContent : Mod
    {
        private static HedraContent _instance;
        public override string Name => "Project Hedra";

        protected override void RegisterContent()
        {
            RegisterSounds();
            RegisterSkills();

            AddWeaponType("Sword", typeof(Sword));
            AddWeaponType("Axe", typeof(Axe));
            AddWeaponType("Hammer", typeof(Hammer));
            AddWeaponType("Claw", typeof(Claw));
            AddWeaponType("Katar", typeof(Katar));
            AddWeaponType("DoubleBlades", typeof(DoubleBlades));
            AddWeaponType("Bow", typeof(Bow));
            AddWeaponType("Staff", typeof(Staff));
            AddWeaponType("Knife", typeof(Knife));
            AddWeaponType("FarmingRake", typeof(FarmingRake));
            AddWeaponType("FishingRod", typeof(FishingRod));
            AddWeaponType("HoldableObject", typeof(HoldableObject));

            AddClassRestriction(Class.Warrior, "FarmingRake");
            var classes = new[] { Class.Warrior, Class.Mage, Class.Archer, Class.Rogue };
            for (var i = 0; i < classes.Length; ++i)
                AddClassRestriction(classes[i], "FishingRod");

            AddAIType("GiantBeetle", typeof(GiantBeetleAIComponent));
            AddAIType("RangedBeetle", typeof(RangedBeetleAIComponent));
            AddAIType("MeleeBeetle", typeof(MeleeBeetleAIComponent));
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
            AddAIType("SkeletonKamikaze", typeof(SkeletonKamikazeAIComponent));
            AddAIType("Adaptive", typeof(AdaptiveAIComponent));
            AddAIType("DungeonAdaptive", typeof(DungeonAdaptiveAIComponent));
            AddAIType("Squirrel", typeof(SquirrelAIComponent));
            AddAIType("Crab", typeof(CrabAIComponent));
            AddAIType("Fox", typeof(FoxAIComponent));
            AddAIType("Raccoon", typeof(RaccoonAIComponent));
            AddAIType("GroupHostile", typeof(GroupHostileAIComponent));
            AddAIType("Azathul", typeof(AzathulAIComponent));

            AddAnimationEvent("Growl", typeof(Growl));
            AddAnimationEvent("Quake", typeof(Quake));
            AddAnimationEvent("Slash", typeof(Slash));
            AddAnimationEvent("CastRaiseSkeleton", typeof(CastRaiseSkeleton));
            AddAnimationEvent("CastTerror", typeof(CastTerror));
            AddAnimationEvent("CastSiphonBlood", typeof(CastSiphonBlood));
            AddAnimationEvent("CastLeech", typeof(CastLeech));
            AddAnimationEvent("CastBlaze", typeof(CastBlaze));
            AddAnimationEvent("Knockback", typeof(Knockback));
            AddAnimationEvent("Laser", typeof(Laser));
            AddAnimationEvent("Spit", typeof(Spit));
            AddAnimationEvent("SpawnMushroomMinion", typeof(SpawnMushroomMinion));

            AddModelHandler("Ent", typeof(EntHandler));
            AddModelHandler("MushroomMob", typeof(MushroomMobHandler));
            AddModelHandler("Ghost", typeof(GhostHandler));
            AddModelHandler("Beetle", typeof(BeetleHandler));

            AddItemHandler("Recipe", typeof(RecipeHandler));
            AddItemHandler("Potion", typeof(PotionHandler));
            AddItemHandler("HoldingBag", typeof(HoldingBagHandler));

            AddEffectType("Fire", typeof(FireComponent));
            AddEffectType("FireWeakness", typeof(FireWeaknessComponent));
            AddEffectType("Poison", typeof(PoisonousComponent));
            AddEffectType("Toxic", typeof(ToxicComponent));
            AddEffectType("Freeze", typeof(FreezeComponent));
            AddEffectType("Bleed", typeof(BleedComponent));
            AddEffectType("Slow", typeof(SlowComponent));
            AddEffectType("Knock", typeof(KnockComponent));
        }

        private void RegisterSkills()
        {
            AddSkill("Agility", typeof(Agility));
            AddSkill("FlameArrow", typeof(FlameArrow));
            AddSkill("Kick", typeof(Kick));
            AddSkill("IceArrow", typeof(IceArrow));
            AddSkill("PoisonArrow", typeof(PoisonArrow));
            AddSkill("Puncture", typeof(Puncture));
            AddSkill("Jab", typeof(Jab));

            AddSkill("LearnKnife", typeof(LearnKnife));
            AddSkill("Concealment", typeof(Concealment));
            AddSkill("Focus", typeof(Focus));
            AddSkill("Raven", typeof(Raven));
            AddSkill("Scavenge", typeof(Scavenge));
            AddSkill("SpikeTrap", typeof(SpikeTrap));
            AddSkill("SteadyAim", typeof(SteadyAim));
            AddSkill("SteelArrows", typeof(SteelArrows));

            AddSkill("HotPursuit", typeof(HotPursuit));
            AddSkill("Nimbleness", typeof(Nimbleness));
            AddSkill("Retreat", typeof(Retreat));
            AddSkill("Rush", typeof(Rush));
            AddSkill("Swiftness", typeof(Swiftness));

            AddSkill("BurstOfSpeed", typeof(BurstOfSpeed));
            AddSkill("LearnClaw", typeof(LearnClaw));
            AddSkill("LearnKatar", typeof(LearnKatar));
            AddSkill("RoundSlash", typeof(RoundSlash));
            AddSkill("Shuriken", typeof(Shuriken));
            AddSkill("Venom", typeof(Venom));
            AddSkill("ShadowWarrior", typeof(ShadowWarrior));

            AddSkill("FanOfKnives", typeof(FanOfKnives));
            AddSkill("SnakeSpirit", typeof(SnakeSpirit));
            AddSkill("MartialArtsTraining", typeof(MartialArtsTraining));
            AddSkill("MartialArtsMaster", typeof(MartialArtsMaster));
            AddSkill("TigerStrike", typeof(TigerStrike));
            AddSkill("TripleShuriken", typeof(TripleShuriken));
            AddSkill("FinishingBlow", typeof(FinishingBlow));

            AddSkill("Fade", typeof(Fade));
            AddSkill("Treason", typeof(Treason));
            AddSkill("Stealth", typeof(Stealth));
            AddSkill("CleanCut", typeof(CleanCut));
            AddSkill("MarkedForDeath", typeof(MarkedForDeath));
            AddSkill("Cutthroat", typeof(Cutthroat));
            AddSkill("NightStalker", typeof(NightStalker));
            AddSkill("QuietSteps", typeof(QuietSteps));

            AddSkill("Bash", typeof(Bash));
            AddSkill("Intercept", typeof(Intercept));
            AddSkill("Resistance", typeof(Resistance));
            AddSkill("Whirlwind", typeof(Whirlwind));
            AddSkill("NoEscape", typeof(NoEscape));
            AddSkill("Leap", typeof(Leap));
            AddSkill("Thorns", typeof(Thorns));

            AddSkill("Faith", typeof(Faith));
            AddSkill("FireEnchant", typeof(FireEnchant));
            AddSkill("HolyAttack", typeof(HolyAttack));
            AddSkill("LearnHammer", typeof(LearnHammer));
            AddSkill("Conversion", typeof(Conversion));
            AddSkill("Prayer", typeof(Prayer));
            AddSkill("Smite", typeof(Smite));

            AddSkill("Berserk", typeof(Berserk));
            AddSkill("Frenzy", typeof(Frenzy));
            AddSkill("GroundStomp", typeof(GroundStomp));
            AddSkill("IronSkin", typeof(IronSkin));
            AddSkill("LearnAxe", typeof(LearnAxe));
            AddSkill("Sacrifice", typeof(Sacrifice));
            AddSkill("BattleCry", typeof(BattleCry));

            AddSkill("FireRelease", typeof(FireRelease));
            AddSkill("Blaze", typeof(Blaze));
            AddSkill("EnergyShield", typeof(EnergyShield));
            AddSkill("Teleport", typeof(Blink));
            AddSkill("FireMastery", typeof(FireMastery));
            AddSkill("Meditation", typeof(Meditation));
            AddSkill("Inferno", typeof(Inferno));

            AddSkill("BloodExchange", typeof(BloodExchange));
            AddSkill("Leech", typeof(Leech));
            AddSkill("DarkReaping", typeof(DarkReaping));
            AddSkill("RaiseSkeleton", typeof(RaiseSkeleton));
            AddSkill("SiphonBlood", typeof(SiphonBlood));
            AddSkill("SkeletonMastery", typeof(SkeletonMastery));
            AddSkill("Terror", typeof(Terror));


            AddSkill("ArcticBlast", typeof(ArcticBlast));
            AddSkill("BearCompanion", typeof(BearCompanion));
            AddSkill("CompanionMastery", typeof(CompanionMastery));
            AddSkill("GroundFissure", typeof(GroundFissure));
            AddSkill("HawkCompanion", typeof(HawkCompanion));
            AddSkill("OakSpirit", typeof(OakSpirit));
            AddSkill("Werewolf", typeof(Werewolf));
            AddSkill("WindOfChange", typeof(WindOfChange));
            AddSkill("WolfCompanion", typeof(WolfCompanion));
        }

        private static void RegisterSounds()
        {
            Log.WriteLine("Loading sounds...");
            TaskScheduler.Parallel(delegate
            {
                SoundPlayer.LoadSound(SoundType.ButtonClick, "$DataFile$/Sounds/HoverButton.ogg");
                SoundPlayer.LoadSound(SoundType.WaterSplash, "$DataFile$/Sounds/WaterSplash.ogg");
                SoundPlayer.LoadSound(SoundType.ButtonHover, "$DataFile$/Sounds/OnOff.ogg");
                SoundPlayer.LoadSound(SoundType.SwooshSound, "$DataFile$/Sounds/Swoosh.ogg");
                SoundPlayer.LoadSound(SoundType.HitSound, "$DataFile$/Sounds/Hit.ogg");
                SoundPlayer.LoadSound(SoundType.NotificationSound, "$DataFile$/Sounds/ItemCollect.ogg");
                SoundPlayer.LoadSound(SoundType.ArrowHit, "$DataFile$/Sounds/Hit.ogg");
                SoundPlayer.LoadSound(SoundType.BowSound, "$DataFile$/Sounds/Bow.ogg");
                SoundPlayer.LoadSound(SoundType.DarkSound, "$DataFile$/Sounds/DarkSound.ogg");
                SoundPlayer.LoadSound(SoundType.SlashSound, "$DataFile$/Sounds/Slash.ogg");
                SoundPlayer.LoadSound(SoundType.Jump, "$DataFile$/Sounds/Jump.ogg");
                SoundPlayer.LoadSound(SoundType.TransactionSound, "$DataFile$/Sounds/Money.ogg");
                SoundPlayer.LoadSound(SoundType.FoodEat, "$DataFile$/Sounds/Eat.ogg");
                SoundPlayer.LoadSound(SoundType.HorseRun, "$DataFile$/Sounds/Horse.ogg");
                SoundPlayer.LoadSound(SoundType.Fireplace, "$DataFile$/Sounds/Fireplace.ogg");
                SoundPlayer.LoadSound(SoundType.HumanRun, "$DataFile$/Sounds/Run.ogg");
                SoundPlayer.LoadSound(SoundType.HumanRunWood, "$DataFile$/Sounds/RunWood.ogg");
                SoundPlayer.LoadSound(SoundType.HitGround, "$DataFile$/Sounds/HitGround.ogg");
                SoundPlayer.LoadSound(SoundType.Dodge, "$DataFile$/Sounds/Roll.ogg");
                SoundPlayer.LoadSound(SoundType.LongSwoosh, "$DataFile$/Sounds/LongSwoosh.ogg");
                SoundPlayer.LoadSound(SoundType.GlassBreak, "$DataFile$/Sounds/GlassBreak.ogg");
                SoundPlayer.LoadSound(SoundType.GlassBreakInverted, "$DataFile$/Sounds/GlassBreakInverted.ogg");
                SoundPlayer.LoadSound(SoundType.HumanSleep, "$DataFile$/Sounds/HumanSleep.ogg");
                SoundPlayer.LoadSound(SoundType.TalkSound, "$DataFile$/Sounds/ItemCollect.ogg");
                SoundPlayer.LoadSound(SoundType.GroundQuake, "$DataFile$/Sounds/GroundQuake.ogg");
                SoundPlayer.LoadSound(SoundType.BeetleSpitSound, "$DataFile$/Sounds/BeetleSpit.ogg");
                SoundPlayer.LoadSound(SoundType.GorillaGrowl, "$DataFile$/Sounds/GorillaGrowl.ogg");
                SoundPlayer.LoadSound(SoundType.PreparingAttack, "$DataFile$/Sounds/PreparingAttack.ogg");
                SoundPlayer.LoadSound(SoundType.River, "$DataFile$/Sounds/River.ogg");
                SoundPlayer.LoadSound(SoundType.Underwater, "$DataFile$/Sounds/Underwater.ogg");
                SoundPlayer.LoadSound(SoundType.Sheep, "$DataFile$/Sounds/Sheep.ogg");
                SoundPlayer.LoadSound(SoundType.Goat, "$DataFile$/Sounds/Goat.ogg");
                SoundPlayer.LoadSound(SoundType.Door, "$DataFile$/Sounds/Door.ogg");
                SoundPlayer.LoadSound(SoundType.ItemEquip, "$DataFile$/Sounds/ItemEquip.ogg");
                SoundPlayer.LoadSound(SoundType.BoatMove, "$DataFile$/Sounds/BoatMove.ogg");
                SoundPlayer.LoadSound(SoundType.BearTrap, "$DataFile$/Sounds/BearTrap.ogg");
                SoundPlayer.LoadSound(SoundType.HealSound, "$DataFile$/Sounds/HealEffect.ogg");
                SoundPlayer.LoadSound(SoundType.TeleportSound, "$DataFile$/Sounds/Teleport.ogg");
                SoundPlayer.MarkAsReady();
                Log.WriteLine("Finished loading sounds.");
            });
        }

        public static void Register()
        {
            (_instance ?? (_instance = new HedraContent())).Load();
        }
    }
}