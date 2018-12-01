using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public abstract class Builder<T> where T : IBuildingParameters
    {
        protected virtual bool LookAtCenter => true;
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
            for (var i = 0; i < Input.Models.Count; i++)
            {
            //    Input.Models[i].GraduateColor(Vector3.UnitY);
            }
            return Input;
        }
        
        public virtual DesignTemplate SelectType(T Parameters, DesignTemplate[] Designs)
        {
            return Designs[Parameters.Rng.Next(0, Designs.Length)];
        }
        
        /* Called via reflection */
        public virtual BuildingOutput Build(T Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var transformationMatrix = BuildTransformation(Parameters);
            var model = Cache.GrabModel(Parameters.Design.Path);
            model.Transform(transformationMatrix);

            var shapes = Cache.GrabShapes(Parameters.Design.Path);
            shapes.ForEach(shape => shape.Transform(transformationMatrix));
            return new BuildingOutput
            {
                Models = new[] { model },
                Shapes = shapes
            };
        }

        protected void AddDoors(T Parameters, DoorTemplate[] Doors, Matrix4 Transformation, BuildingOutput Output)
        {
            for (var i = 0; i < Doors.Length; ++i)
            {
                var doorTemplate = Doors[i];
                var vertexData = AssetManager.PLYLoader(doorTemplate.Path, Vector3.One * Parameters.Design.Scale);
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
        public virtual void Polish(T Parameters)
        {
            
        }

        protected void SpawnHumanoid(HumanType Type, Vector3 Position)
        {
            var human = World.WorldBuilding.SpawnHumanoid(Type, Position);
            VillageObject.AddHumanoid(human);
        }

        protected void SpawnVillager(Vector3 Position, bool Move)
        {
            var human = World.WorldBuilding.SpawnVillager(Position, Move);
            VillageObject.AddHumanoid(human);
        }

        protected float ModelRadius(T Parameters, VillageCache Cache)
        {
            return Cache.GrabSize(Parameters.Design.Path).Xz.LengthFast;
        }

        protected IGroundwork CreateGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return new RoundedGroundwork(Position, Radius, Type);
        }
        
        protected bool PlaceGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return this.PushGroundwork(this.CreateGroundwork(Position, Radius, Type));
        }
        
        protected bool PushGroundwork(IGroundwork Item)
        {
            if (Item != null)
            {
                Structure.AddGroundwork(Item);
            }
            return true;
        }
        
        protected bool IntersectsWithAnyPath(Vector2 Point, float Radius)
        {
            var paths = World.WorldBuilding.Groundworks.Where(G => G.IsPath).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                if (paths[i].Affects(Point)) return true;
            }
            return false;
        }    
    }
}