using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal abstract class Builder
    {   
        public abstract void Place(BuildingParameters Parameters);

        public virtual BuildingOutput Paint(BuildingOutput Input)
        {
            return Input;
        }
        
        public virtual DesignTemplate SelectType(BuildingParameters Parameters, DesignTemplate[] Designs)
        {
            return Designs[Parameters.Rng.Next(0, Designs.Length)];
        }
        
        public virtual BuildingOutput Build(BuildingParameters Parameters, VillageCache Cache)
        {
            var positionMatrix = Matrix4.CreateTranslation(Parameters.Position);
            var model = Cache.GrabModel(Parameters.Design.Path);
            model.Transform(positionMatrix);

            var shapes = Cache.GrabShapes(Parameters.Design.Path);
            shapes.ForEach(shape => shape.Transform(positionMatrix));
            return new BuildingOutput
            {
                Model = model,
                Shapes = shapes
            };
        }

        protected void PlaceGroundwork(Vector3 Position, float Radius)
        {
            World.QuestManager.AddPlateau(new Plateau(Position, Radius, 0f));
            World.QuestManager.AddVillagePosition(Position, Radius);
        }
    }
}