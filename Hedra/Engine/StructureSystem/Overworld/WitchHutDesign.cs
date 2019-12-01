using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Engine.Scenes;
using Hedra.Framework;
using Hedra.Framework;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHutDesign : SimpleCompletableStructureDesign<WitchHut>
    {
        public override int PlateauRadius => 160;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WitchHutIcon);
        protected override int StructureChance => StructureGrid.WitchHut;
        protected override CacheItem? Cache => CacheItem.WitchHut;
        protected override BlockType PathType => BlockType.Grass;
        protected override bool NoPlantsZone => true;
        public override string DisplayName => Translations.Get("structure_witch_hut");
        public override bool CanSpawnInside => false;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            Structure.Waypoints = WaypointLoader.Load($"Assets/Env/Structures/WitchHut/WitchHut0-Pathfinding.ply", WitchHutCache.Scale, Rotation * Translation);
            var door0 = AddDoor(WitchHutCache.Hut0Door0, WitchHutCache.Hut0Door0Position, Rotation, Structure, WitchHutCache.Hut0Door0InvertedRotation, WitchHutCache.Hut0Door0InvertedPivot);
            var door1 = AddDoor(WitchHutCache.Hut0Door1, WitchHutCache.Hut0Door1Position, Rotation, Structure, WitchHutCache.Hut0Door1InvertedRotation, WitchHutCache.Hut0Door1InvertedPivot);
            PlacePlants(Structure, Translation, Rotation, Rng);
            var npc = AddNPCs(Structure, Rotation * Translation, Rng);
            AddDelivery((WitchHut)Structure.WorldObject, Rng);
            AddReward((WitchHut) Structure.WorldObject, Rng);
            door0.InvokeInteraction(npc);
            door1.InvokeInteraction(npc);
        }

        private static void AddDelivery(WitchHut Structure, Random Rng)
        {
            var possibleItems = new[]
            {
                ItemType.Peas,
                ItemType.Cabbage,
                ItemType.Carrot,
                ItemType.Mushroom,
                ItemType.Onion
            };
            var possibleAmounts = new[] {1, 2, 3};
            var item = ItemPool.Grab(possibleItems[Rng.Next(0, possibleItems.Length)]);
            item.SetAttribute(CommonAttributes.Amount, possibleAmounts[Rng.Next(0, possibleAmounts.Length)]);
            //Structure.PickupItem = item;
        }

        private static void AddReward(WitchHut Hut, Random Rng)
        {
            if (Rng.Next(0, 5) == 1) return;
            /*Hut.RewardItem = ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Potion)
            {
                Seed = Unique.RandomSeed(Rng),
                SameTier = false
            });*/
        }
        
        private static IHumanoid AddNPCs(CollidableStructure Structure, Matrix4x4 Transformation, Random Rng)
        {
            var enemies = new List<IEntity>();
            IHumanoid female = null, male = null;
            if (Rng.Next(0, 8) != 1)
            {
                female = World.WorldBuilding.SpawnHumanoid(
                    HumanType.Witch,
                    Vector3.Transform(WitchHutCache.Hut0Witch0Position, Transformation)
                );
                HumanoidFactory.AddAI(female, false);
            }
            if (female == null || Rng.Next(0, 8) != 1)
            {
                male = World.WorldBuilding.SpawnHumanoid(
                    HumanType.Witch,
                    Vector3.Transform(WitchHutCache.Hut0Witch1Position, Transformation)
                );
                HumanoidFactory.AddAI(male, false);
            }

            if (female != null && male != null)
            {
                female.SearchComponent<DamageComponent>().Ignore(E => E == male);
                male.SearchComponent<DamageComponent>().Ignore(E => E == female);
            }
            if(female != null) enemies.Add(female);
            if(male != null) enemies.Add(male);
            ((WitchHut) Structure.WorldObject).Enemies = enemies.ToArray();
            return male ?? female;
        }
        
        private void PlacePlants(CollidableStructure Structure, Matrix4x4 Translation, Matrix4x4 Rotation, Random Rng)
        {
            DecorationsPlacer.PlaceWhenWorldReady(Structure.Position,
                P =>
                {
                    var rotatedOffset = Vector3.Transform(WitchHutCache.PlantOffset, Rotation);
                    var designs = new HarvestableDesign[]
                    {
                        new CabbageDesign(),
                        new OnionDesign(),
                        new CarrotDesign(),
                        new PeasDesign(),
                        new MushroomDesign(), 
                        new TomatoDesign(),
                    };

                    using (var allocator = new HeapAllocator(Allocator.Megabyte))
                    {
                        for (var i = 0; i < designs.Length; ++i)
                        {
                            AddPlantLine(
                                allocator,
                                Vector3.Transform(WitchHutCache.PlantRows[i], Rotation * Translation),
                                rotatedOffset,
                                designs[i],
                                WitchHutCache.PlantWidths[i],
                                Rng
                            );
                        }
                    }
                }, () => Structure.Disposed);
        }
        
        private void AddPlantLine(IAllocator Allocator, Vector3 Position, Vector3 UnitOffset, HarvestableDesign Design, int Count, Random Rng)
        {
            var offset = UnitOffset;
            for (var i = 0; i < Count; ++i)
            {
                if(Rng.Next(0, 7) != 1)
                    AddPlant(Allocator, Position + offset * (float)Math.Pow(-1, i), Design, Rng);
                offset += UnitOffset * 2;
            }
        }

        protected override WitchHut Create(Vector3 Position, float Size)
        {
            return new WitchHut(Position);
        }

        protected override string GetDescription(WitchHut Structure)
        {
            return Translations.Get("quest_complete_structure_description_witch_hut_kill", Structure.EnemiesLeft); /*
                "quest_complete_structure_description_witch_hut",
                $"{Structure.PickupItem.GetAttribute<int>(CommonAttributes.Amount)} {Structure.PickupItem.DisplayName}",
                DisplayName,
                Structure.EnemiesLeft*/
        }

        protected override string GetShortDescription(WitchHut Structure)
        {
            return Translations.Get("quest_complete_structure_short_witch_hut", DisplayName);
        }

        protected override float BuildRotationAngle(Random Rng)
        {
            return Rng.Next(0, 4) * 90;
        }
    }
}