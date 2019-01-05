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
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PlantSystem;
using OpenTK;
using Region = Hedra.BiomeSystem.Region;

namespace Hedra.Engine.Generation
{
    /// <summary>
    /// Description of HerbGenerator.
    /// </summary>
    public class EnvironmentGenerator
    {
        
        public void GeneratePlant(Vector3 Position, Region BiomeRegion, PlantDesign Design)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return;

            var rng = underChunk.Landscape.RandomGen;
            var transMatrix = Design.TransMatrix(Position, rng);

            if (transMatrix == Matrix4.Identity) return;

            var modelData = Design.Model;
            var modelDataClone = modelData.ShallowClone();

            Design.Paint(modelDataClone, BiomeRegion, rng);
            Design.AddShapes(underChunk, transMatrix);         
            if (!Design.HasCustomPlacement)
            {
                var data = new InstanceData
                {
                    OriginalMesh = modelData,
                    Colors = modelDataClone.Colors.Clone(),
                    ExtraData = modelDataClone.Extradata.Clone(),
                    TransMatrix = transMatrix
                };
                CacheManager.Check(data);
                underChunk.AddInstance(data, Design.AffectedByLod);
                modelDataClone.Dispose();
            }
            else
            {
                Design.CustomPlacement(modelDataClone, transMatrix, underChunk);
            }
        }
    }
}
