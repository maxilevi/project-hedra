using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal abstract class Builder<T> where T : IBuildingParameters
    {   
        public virtual void Place(T Parameters, VillageCache Cache)
        {
            this.PlaceGroundwork(Parameters.Position, Cache.GrabSize(Parameters.Design.Path).Xz.LengthFast);
        }

        public virtual BuildingOutput Paint(T Parameters, BuildingOutput Input)
        {
            return Input;
        }
        
        public virtual DesignTemplate SelectType(T Parameters, DesignTemplate[] Designs)
        {
            return Designs[Parameters.Rng.Next(0, Designs.Length)];
        }
        
        public virtual BuildingOutput Build(T Parameters, VillageCache Cache)
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

        public virtual void BuildNPCs(T Parameters)
        {
            
        }

        protected void PlaceGroundwork(Vector3 Position, float Radius)
        {
            BlockType type;
            float height = World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out type);
            
            World.QuestManager.AddPlateau(new Plateau(Position, Radius, 0f, height));
            World.QuestManager.AddVillagePosition(Position, Radius);
        }
    }
}