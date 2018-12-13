using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public abstract class Builder<T> where T : IBuildingParameters
    {
        protected virtual bool LookAtCenter => true;
        protected virtual bool GraduateColor => true;
        protected CollidableStructure Structure { get; }
        private Village VillageObject { get; }
        
        protected Builder(CollidableStructure Structure)
        {
            this.Structure = Structure;
            this.VillageObject = (Village) Structure.WorldObject;
        }

        public virtual bool Place(T Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .75f);
        }

        /* Called via reflection */
        public virtual BuildingOutput Paint(T Parameters, BuildingOutput Input)
        {
            return Input;
        }
        
        public virtual DesignTemplate SelectType(T Parameters, DesignTemplate[] Designs)
        {
            return Designs[Parameters.Rng.Next(0, Designs.Length)];
        }
        
        public virtual BuildingOutput Build(T Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var transformationMatrix = BuildTransformation(Parameters);
            var shapes = Cache.GrabShapes(Design.Path);
            shapes.ForEach(shape => shape.Transform(transformationMatrix));
            return new BuildingOutput
            {
                Models = new[] { Cache.GrabModel(Design.Path) },
                LodModels = Design.LodPath != null ? new[] { Cache.GrabModel(Design.LodPath) } : null,
                TransformationMatrices = new[] { transformationMatrix },
                Shapes = shapes,
                GraduateColors = GraduateColor,
            };
        }
        
        protected void AddBeds(T Parameters, BedTemplate[] Beds, Matrix4 Transformation, BuildingOutput Output)
        {
            for (var i = 0; i < Beds.Length; ++i)
            {
                var template = Beds[i];
                Output.Structures.Add(
                    new SleepingPad(Vector3.TransformPosition(template.Position * Parameters.Design.Scale, Transformation) + Parameters.Position)
                );
            }
        }

        protected void AddDoors(T Parameters, VillageCache Cache, DoorTemplate[] Doors, Matrix4 Transformation, BuildingOutput Output)
        {
            for (var i = 0; i < Doors.Length; ++i)
            {
                var doorTemplate = Doors[i];
                var vertexData = Cache.GetOrCreate(doorTemplate.Path, Vector3.One * Parameters.Design.Scale);
                var rotationPoint = Vector3.TransformPosition(Door.GetRotationPointFromMesh(vertexData, doorTemplate.Inverted), Transformation);
                vertexData.Center();
                vertexData.Transform(Transformation);
                var offset = Vector3.TransformPosition(doorTemplate.Position * Parameters.Design.Scale, Transformation);
                Output.Structures.Add(
                    new Door(
                        vertexData,
                        rotationPoint,
                        Parameters.Position + offset,
                        Structure
                    )
                );
            }
        }
        
        protected Matrix4 BuildTransformation(T Parameters)
        {
            var rotationMatrix = LookAtCenter ? Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian) : Matrix4.Identity;
            return rotationMatrix * Matrix4.CreateTranslation(Parameters.Position);
        }

        /// <summary>
        /// Called as a last step to setup the remaining objects e.g. merchants, lights, etc
        /// </summary>
        /// <param name="Parameters">The placement parameters</param>
        public virtual void Polish(T Parameters, Random Rng)
        {
            
        }

        protected IHumanoid SpawnHumanoid(HumanType Type, Vector3 Position)
        {
            var human = World.WorldBuilding.SpawnHumanoid(Type, Position);
            VillageObject.AddHumanoid(human);
            return human;
        }

        protected IHumanoid SpawnVillager(Vector3 Position, bool Move)
        {
            var human = World.WorldBuilding.SpawnVillager(Position, Move);
            VillageObject.AddHumanoid(human);
            return human;
        }
        
        protected IEntity SpawnMob(MobType Mob, Vector3 Position)
        {
            var mob = World.SpawnMob(Mob, Position, Utils.Rng);
            VillageObject.AddMob(mob);
            return mob;
        }
        

        protected float ModelRadius(T Parameters, VillageCache Cache)
        {
            return Cache.GrabSize(Parameters.Design.Path).Xz.LengthFast;
        }

        protected GroundworkItem CreateGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return new GroundworkItem
            {
                Plateau = new RoundedPlateau(Position, Radius * 1.5f),
                Groundwork = new RoundedGroundwork(Position, Radius, Type)
            };
        }
        
        protected bool PlaceGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return this.PushGroundwork(this.CreateGroundwork(Position, Radius, Type));
        }
        
        protected bool PushGroundwork(GroundworkItem Item)
        {
            Structure.AddPlateau(Item.Plateau);
            if (Item.Groundwork != null)
            {
                Structure.AddGroundwork(Item.Groundwork);
            }
            return true;
        }
        
        public class GroundworkItem
        {
            public BasePlateau Plateau { get; set; }
            public IGroundwork Groundwork { get; set; }
        }
    }
}