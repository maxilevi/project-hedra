using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.API;
using Hedra.Components;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Mission;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public class NPCCreatorProvider : INPCCreatorProvider
    {
        public Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type.ToString(), 1, DesiredPosition, null);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type, 1, DesiredPosition, null);
        }

        public Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng)
        {
            return SpawnVillager(DesiredPosition, Rng.Next(int.MinValue, int.MaxValue));
        }

        public Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed)
        {
            var rng = new Random(Seed);
            var types = new[]
            {
                HumanType.Innkeeper,
                HumanType.Blacksmith,
                HumanType.Clothier,
                HumanType.Scholar,
                HumanType.Bard
            };
            return SpawnVillager(types[rng.Next(0, types.Length)], DesiredPosition, Seed);
        }

        public Humanoid SpawnVillager(HumanType Type, Vector3 DesiredPosition, int Seed)
        {
            var rng = new Random(Seed);
            var villager = SpawnHumanoid(Type, DesiredPosition, new HumanoidConfiguration(HealthBarType.Friendly));
            villager.Seed = Seed;
            villager.SetWeapon(null);
            villager.Name = NameGenerator.PickMaleName(rng);
            villager.IsFriendly = true;
            villager.SearchComponent<DamageComponent>().Immune = true;
            villager.RemoveComponent<TradeComponent>();
            villager.RemoveComponent<TalkComponent>();
            return villager;
        }

        public IHumanoid SpawnQuestGiver(HumanType Type, Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            var npc = SpawnVillager(
                Type,
                Position,
                Rng.Next(int.MinValue, int.MaxValue)
            );
            ApplyQuestGiverStatus(npc, Position, Quest);
            return npc;
        }

        public IHumanoid SpawnQuestGiver(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            var npc = SpawnVillager(
                Position,
                Rng
            );
            ApplyQuestGiverStatus(npc, Position, Quest);
            return npc;
        }

        public Humanoid SpawnBandit(Vector3 Position, int Level, BanditOptions Options)
        {
            var availableClasses = new List<Class>();
            for (var i = 0; i < 4; i++)
            {
                var @class = (Class)(1 << i);
                if ((@class & Options.PossibleClasses) == @class)
                    availableClasses.Add(@class);
            }

            var classN = Utils.Rng.Next(0, availableClasses.Count);
            var classType = ClassDesign.FromString(availableClasses[classN]);
            var gender = (HumanGender) Utils.Rng.Next(0, 2);
            var customization = CustomizationData.FromClass(classType, gender);
            customization.FirstHairColor = NPCCreator.HairColors.Random(Utils.Rng);
            customization.SecondHairColor = NPCCreator.HairColors.Random(Utils.Rng);

            var behaviour =
                new HumanoidConfiguration(Options.Friendly ? HealthBarType.Friendly : HealthBarType.Hostile);
            var modelTemplate = Options.ModelType != null
                ? HumanoidLoader.HumanoidTemplater[Options.ModelType.Value.ToString()]
                : null;
            var template = HumanoidLoader.HumanoidTemplater[classType.ToString()].Clone();
            var isPlayableModel = modelTemplate == null;
            if (!isPlayableModel)
            {
                template.Models = modelTemplate.Models;
                template.Model = modelTemplate.Model;
            }
            else
            {
                template.Model = classType.ModelTemplate;
                template.Models = null;
            }

            var templateName = modelTemplate != null
                ? Translations.Has(modelTemplate.DisplayName.ToLowerInvariant())
                    ?
                    Translations.Get(modelTemplate.DisplayName.ToLowerInvariant())
                    : modelTemplate.DisplayName
                : Translations.Get("bandit");
            var human = SpawnHumanoid(classType.ToString(), template, Level, Position, behaviour);

            if (isPlayableModel)
                AddArmors(human, Utils.Rng);
            HumanoidFactory.AddAI(human, Options.Friendly);
            if (Options.Friendly)
                human.SearchComponent<DamageComponent>()
                    .Ignore(E => E is IPlayer || E == GameManager.Player.Companion.Entity);
            human.Name = !Options.Friendly ? templateName : NameGenerator.PickMaleName(Utils.Rng);
            human.IsFriendly = Options.Friendly;
            human.Customization = customization;
            Options.ApplyQuestStatus(human);
            human.UpdateWhenOutOfRange = Options.IsFromQuest;
            return human;
        }

        private static void AddArmors(IHumanoid Humanoid, Random Rng)
        {
            var addCompleteSet = Rng.Next(0, 16) == 1;
            if (addCompleteSet)
            {
                var helmet = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Helmet)
                {
                    SameTier = false
                });
                var chestplate = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Chestplate)
                {
                    SameTier = false
                });
                var pants = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Pants)
                {
                    SameTier = false
                });

                Humanoid.SetHelmet(helmet);
                Humanoid.SetChestplate(chestplate);
                Humanoid.SetPants(pants);
                /* Boots cause a mesh glitch, we should explore all the items */
                /*
                 Humanoid.SetBoots(boots);
                 var boots = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Boots)
                {
                    SameTier = false
                });
                 */   
            }
            else if(Rng.Next(0, 8) == 1)
            {
                var chestplate = ItemPool.Grab(new ItemPoolSettings(ItemTier.Unique, EquipmentType.Chestplate)
                {
                    SameTier = false
                });
                Humanoid.SetChestplate(chestplate);
            }
        }

        private Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition, HumanoidConfiguration Configuration)
        {
            return SpawnHumanoid(Type.ToString(), 1, DesiredPosition, Configuration);
        }

        private Humanoid SpawnHumanoid(string Type, int Level, Vector3 DesiredPosition,
            HumanoidConfiguration Configuration)
        {
            return SpawnHumanoid(Type, HumanoidLoader.HumanoidTemplater[Type], Level, DesiredPosition, Configuration);
        }

        private Humanoid SpawnHumanoid(string Type, HumanoidTemplate Template, int Level, Vector3 DesiredPosition,
            HumanoidConfiguration Configuration)
        {
            var human = HumanoidFactory.BuildHumanoid(Type, Template, Level, Configuration);
            human.Position = World.FindPlaceablePosition(human,
                new Vector3(DesiredPosition.X, Physics.HeightAtPosition(DesiredPosition.X, DesiredPosition.Z),
                    DesiredPosition.Z));
            human.Rotation = new Vector3(0, Utils.Rng.NextFloat(), 0) * 360f * Mathf.Radian;
            ApplySeasonHats(human, Type);
            return human;
        }

        private void ApplyQuestGiverStatus(IHumanoid Npc, Vector3 Position, IMissionDesign Quest)
        {
            Npc.Position = Position;
            Npc.AddComponent(new QuestGiverComponent(Npc, Quest));
        }


        private void ApplySeasonHats(Humanoid Human, string Type)
        {
            if (Season.IsChristmas)
                Human.SetHelmet(ItemPool.Grab(ItemType.ChristmasHat));
        }
    }
}