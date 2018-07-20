using System;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal abstract class Builder<T> where T : IBuildingParameters
    {
        protected virtual bool LookAtCenter => true;

        public virtual void Place(T Parameters, VillageCache Cache)
        {
            this.PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .75f);
        }

        public virtual BuildingOutput Paint(T Parameters, BuildingOutput Input)
        {
            return Input;
        }
        
        public virtual DesignTemplate SelectType(T Parameters, DesignTemplate[] Designs)
        {
            return Designs[Parameters.Rng.Next(0, Designs.Length)];
        }
        
        public virtual BuildingOutput Build(T Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var rotY = Physics.DirectionToEuler((Center - Parameters.Position).NormalizedFast()).Y;
            var rotationMatrix = LookAtCenter ? Matrix4.CreateRotationY(rotY * Mathf.Radian) : Matrix4.Identity;
            var transformationMatrix = rotationMatrix * Matrix4.CreateTranslation(Parameters.Position);
            var model = Cache.GrabModel(Parameters.Design.Path);
            model.Transform(transformationMatrix);

            var shapes = Cache.GrabShapes(Parameters.Design.Path);
            shapes.ForEach(shape => shape.Transform(transformationMatrix));
            return new BuildingOutput
            {
                Model = model,
                Shapes = shapes
            };
        }

        /// <summary>
        /// Called as a last step to setup the remaining objects e.g. merchants, lights, etc
        /// </summary>
        /// <param name="Parameters">The placement parameters</param>
        public virtual void Polish(T Parameters)
        {
            
        }

        protected float ModelRadius(T Parameters, VillageCache Cache)
        {
            return Cache.GrabSize(Parameters.Design.Path).Xz.LengthFast;
        }

        protected void PlaceGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            float height = World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out _);
            
            World.QuestManager.AddPlateau(new Plateau(Position, Radius * 1.25f, 10000f, height));
            World.QuestManager.AddGroundwork(new RoundedGroundwork(Position, Radius, Type));
        }
    }
}