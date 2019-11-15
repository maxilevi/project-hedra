using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class BaseDungeonDesign<T> : SimpleCompletableStructureDesign<T> where T : BaseStructure, ICompletableStructure
    {
        public override string DisplayName => Translations.Get("structure_dungeon");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        
        protected override string GetDescription(T Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(T Structure) => throw new System.NotImplementedException();
        
        protected Lever AddLever(CollidableStructure Structure, Vector3 Position, Matrix4x4 Rotation)
        {
            var lever = new Lever(Vector3.Transform((Position + StructureOffset) * StructureScale, Rotation) + Structure.Position, StructureScale);
            var axisAngle = Rotation.ExtractRotation().ToAxisAngle();
            lever.Rotation = axisAngle.Xyz() * axisAngle.W * Mathf.Degree;
            Structure.WorldObject.AddChildren(lever);
            return lever;
        }
    }
}