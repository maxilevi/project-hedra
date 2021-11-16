using System;
using System.Collections.Generic;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Engine.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.EntitySystem;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public abstract class Builder
    {
        public static void SpawnVillager(Vector3 Position, Random Rng, VillageGraph Graph, out IHumanoid Humanoid)
        {
            Humanoid = NPCCreator.SpawnVillager(Position, Rng);
            Humanoid.SearchComponent<DamageComponent>().Immune = true;
            Humanoid.AddComponent(new TalkComponent(Humanoid));
            Humanoid.AddComponent(new RoamingVillagerAIComponent(Humanoid, Graph));
            Humanoid.AddComponent(new VillagerThoughtsComponent(Humanoid));
        }
    }
    
    public abstract class Builder<T> : Builder where T : IBuildingParameters
    {
        protected virtual bool LookAtCenter => true;
        protected virtual bool GraduateColor => false;
        protected CollidableStructure Structure { get; }
        private BaseStructure VillageObject { get; }
        
        protected Builder(CollidableStructure Structure)
        {
            this.Structure = Structure;
            this.VillageObject = Structure.WorldObject;
        }

        public virtual bool Place(T Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .75f);
        }

        protected bool IsPlateauNeeded(BasePlateau Plateau)
        {
            var mount = Structure.Mountain;
            var squared = Plateau.ToBoundingBox();
            return mount.Density(squared.LeftCorner) < 1 || mount.Density(squared.RightCorner) < 1 ||
                   mount.Density(squared.FrontCorner) < 1 || mount.Density(squared.BackCorner) < 1;
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
        
        public virtual BuildingOutput Build(T Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 VillageCenter)
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
        
        private static Vector3 TransformPosition(T Parameters, Vector3 Position, Matrix4x4 Transformation)
        {
            return Vector3.Transform((Position + Parameters.Design.Offset) * Parameters.Design.Scale, Transformation) + Parameters.Position;
        }
        
        private static void AddStructure<U>(T Parameters, U[] Templates, Matrix4x4 Transformation, BuildingOutput Output, Func<Vector3, U, BaseStructure> Get) where U : IPositionable
        {
            for (var i = 0; i < Templates.Length; ++i)
            {
                Output.Structures.Add(Get(TransformPosition(Parameters, Templates[i].Position, Transformation), Templates[i]));
            }
        }
        
        protected void AddBeds(T Parameters, BedTemplate[] Beds, Matrix4x4 Transformation, BuildingOutput Output)
        {
            AddStructure(Parameters, Beds, Transformation, Output, (P, T) => new SleepingPad(P));
        }

        protected void AddChimneys(T Parameters, ChimneyTemplate[] Chimneys, Matrix4x4 Transformation, BuildingOutput Output)
        {
            AddStructure(Parameters, Chimneys, Transformation, Output, (P, T) => new Chimney(P));
        }
        
        protected void AddGenericStructure(T Parameters, StructureTemplate[] Templates, Matrix4x4 Transformation, BuildingOutput Output)
        {
            AddStructure(Parameters, Templates, Transformation, Output, (P, T) => StructureTemplate.FromType(T.Type, P));
        }
        
        protected void AddLights(T Parameters, LightTemplate[] Lights, Matrix4x4 Transformation, BuildingOutput Output)
        {
            AddStructure(Parameters, Lights, Transformation, Output, (P, T) => new WorldLight(P)
            {
                Radius = T.Radius,
                LightColor = WorldLight.DefaultColor
            });
        }

        protected void AddDoors(T Parameters, VillageCache Cache, DoorTemplate[] Doors, Matrix4x4 Transformation, BuildingOutput Output)
        {
            var doors = new List<Door>();
            for (var i = 0; i < Doors.Length; ++i)
            {
                doors.Add(
                    CreateDoor(
                        Cache.GetOrCreate(Doors[i].Path, Vector3.One),
                        Parameters.Position,
                        Parameters.Design.Offset + Doors[i].Position,
                        Parameters.Design.Scale * Vector3.One,
                        Transformation,
                        Structure,
                        Doors[i].InvertedRotation,
                        Doors[i].InvertedPivot
                    )
                );
            }

            Output.Structures.AddRange(doors.ToArray());
        }
        
        public static Door CreateDoor(VertexData Model, Vector3 Position, Vector3 DoorPosition, Vector3 Scale,
            Matrix4x4 Transformation, CollidableStructure Structure, bool InvertedRotation = false, bool InvertedPivot = false)
        {
            var vertexData = Model.Scale(Scale);
            var rotationPoint = Vector3.Transform(Door.GetRotationPointFromMesh(vertexData, InvertedPivot), Transformation);
            vertexData.AverageCenter();
            vertexData.Transform(Transformation);
            var doorPosition = Vector3.Transform(DoorPosition * Scale, Transformation);
            return new Door(
                vertexData,
                rotationPoint,
                Position + doorPosition,
                InvertedRotation
            );
        }
        
        protected Matrix4x4 BuildTransformation(T Parameters)
        {
            var rotationMatrix = LookAtCenter ? Matrix4x4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian) : Matrix4x4.Identity;
            return rotationMatrix * Matrix4x4.CreateTranslation(Parameters.Position);
        }

        /// <summary>
        /// Called as a last step to setup the remaining objects e.g. merchants, lights, etc
        /// </summary>
        /// <param name="Parameters">The placement parameters</param>
        public virtual void Polish(T Parameters, VillageRoot Root, Random Rng)
        {
            
        }

        protected IHumanoid SpawnHumanoid(HumanType Type, Vector3 Position)
        {
            var human = NPCCreator.SpawnHumanoid(Type, Position);
            human.SetWeapon(null);
            VillageObject.AddNPCs(human);
            return human;
        }

        protected IHumanoid SpawnVillager(Vector3 Position, Random Rng)
        {
            if(!(VillageObject is Village village))
                throw new ArgumentOutOfRangeException($"In order to spawn a villager, the structure needs to be a village but is a '{VillageObject.ToString()}'");
            SpawnVillager(Position, Rng, village.Graph, out var humanoid);
            VillageObject.AddNPCs(humanoid);
            return humanoid;
        }

        protected IEntity SpawnMob(MobType Mob, Vector3 Position)
        {
            var mob = World.SpawnMob(Mob, Position, Utils.Rng);
            VillageObject.AddNPCs(mob);
            return mob;
        }
        

        protected float ModelRadius(T Parameters, VillageCache Cache)
        {
            return Cache.GrabSize(Parameters.Design.Path).Xz().LengthFast();
        }

        protected GroundworkItem CreateGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            var plateau = new RoundedPlateau(Position.Xz(), Radius * 1.5f);
            return new GroundworkItem
            {
                Groundwork =  new RoundedGroundwork(Position, Radius, Type),
                Plateau = IsPlateauNeeded(plateau) ? plateau : null
            };
        }
        
        protected bool PlaceGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return this.PushGroundwork(this.CreateGroundwork(Position, Radius, Type));
        }
        
        protected bool PushGroundwork(GroundworkItem Item)
        {
            if(Item.Plateau != null)
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