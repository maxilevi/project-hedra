using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public abstract class Builder<T> where T : IBuildingParameters
    {
        protected virtual bool LookAtCenter => true;

        public virtual bool Place(T Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .75f);
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
            var rotationMatrix = LookAtCenter ? Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian) : Matrix4.Identity;
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

        protected GroundworkItem CreateGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return new GroundworkItem
            {
                Plateau = new Plateau(Position, Radius * 1.25f),
                Groundwork = new RoundedGroundwork(Position, Radius, Type)
            };
        }
        
        protected bool PlaceGroundwork(Vector3 Position, float Radius, BlockType Type = BlockType.Path)
        {
            return this.PushGroundwork(this.CreateGroundwork(Position, Radius, Type));
        }
        
        protected bool PushGroundwork(GroundworkItem Item)
        {
            if (World.WorldBuilding.CanAddPlateau(Item.Plateau))
            {
                World.WorldBuilding.AddPlateau(Item.Plateau);
                if (Item.Groundwork != null)
                {
                    World.WorldBuilding.AddGroundwork(Item.Groundwork);
                }
                return true;
            }
            return false;
        }
    }
    
    public class GroundworkItem {
        public Plateau Plateau { get; set; }
        public IGroundwork Groundwork { get; set; }
    }
}