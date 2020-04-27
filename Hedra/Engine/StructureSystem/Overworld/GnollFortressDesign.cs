using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GnollFortressDesign : SimpleCompletableStructureDesign<GnollFortress>
    {
        public override int PlateauRadius => 480;
        public override string DisplayName => Translations.Get("structure_gnoll_fortress");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GnollFortressIcon);
        public override bool CanSpawnInside => false;
        protected override int StructureChance => StructureGrid.GnollFortressChance;
        protected override CacheItem? Cache => CacheItem.GnollFortress;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            for (var i = 0; i < GnollFortressCache.DoorSettings.Length; ++i)
            {
                var settings = GnollFortressCache.DoorSettings[i];
                AddDoor(
                    AssetManager.PLYLoader($"Assets/Env/Structures/GnollFortress/GnollFortress0-Door{i}.ply", settings.Scale),
                    settings.Position + Vector3.UnitY * 0.05f * GnollFortressCache.Scale,
                    Rotation,
                    Structure,
                    settings.InvertedRotation,
                    settings.InvertedPivot
                );
            }
        }

        protected override GnollFortress Create(Vector3 Position, float Size)
        {
            return new GnollFortress(Position);
        }

        protected override string GetDescription(GnollFortress Structure)
        {
            throw new System.NotImplementedException();
        }

        protected override string GetShortDescription(GnollFortress Structure)
        {
            throw new System.NotImplementedException();
        }
    }
}