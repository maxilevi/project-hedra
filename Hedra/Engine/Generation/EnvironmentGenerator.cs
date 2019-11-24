/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/11/2016
 * Time: 09:24 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PlantSystem;
using System.Numerics;
using Hedra.Framework;
using Hedra.Framework;
using Region = Hedra.BiomeSystem.Region;

namespace Hedra.Engine.Generation
{
    /// <summary>
    /// Description of HerbGenerator.
    /// </summary>
    public class EnvironmentGenerator
    {
        public void GeneratePlant(IAllocator Allocator, Vector3 Position, Region BiomeRegion, PlantDesign Design)
        {
            GeneratePlant(Allocator, Position, BiomeRegion, Design, new Matrix4x4());
        }

        public void GeneratePlant(IAllocator Allocator, Vector3 Position, Region BiomeRegion, PlantDesign Design, Matrix4x4 CustomTransformationMatrix)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return;

            var rng = underChunk.Landscape.RandomGen;
            var transMatrix = CustomTransformationMatrix == new Matrix4x4() ? Design.TransMatrix(Position, rng) : CustomTransformationMatrix;

            if (transMatrix == new Matrix4x4()) return;

            var modelData = Design.Model;
            var modelDataClone = modelData.NativeClone(Allocator);
            /* Only simplify objects that are affected by LOD */
            var canSimplify = Design.AffectedByLod;

            Design.Paint(modelDataClone, BiomeRegion, rng);
            Design.AddShapes(underChunk, transMatrix);         
            if (!Design.HasCustomPlacement)
            {
                var data = new InstanceData
                {
                    OriginalMesh = modelData,
                    Colors = modelDataClone.Colors,
                    ExtraData = modelDataClone.Extradata,
                    TransMatrix = transMatrix,
                    CanSimplifyProgramatically = canSimplify
                };
                CacheManager.Check(data);
                underChunk.AddInstance(data, Design.AffectedByLod);
            }
            else
            {
                Design.CustomPlacement(modelDataClone, transMatrix, underChunk);
            }
            modelDataClone.Dispose();
        }
    }
}
