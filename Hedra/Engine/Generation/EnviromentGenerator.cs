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
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PlantSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of HerbGenerator.
	/// </summary>
	public class EnviromentGenerator
	{
		
		public void GeneratePlant(Vector3 Position, BiomeSystem.Region BiomeRegion, PlantDesign Design)
        {
            Chunk underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return;

            Random rng = underChunk.Landscape.RandomGen;
            Matrix4 transMatrix =  Design.TransMatrix(Position, rng);

            if(transMatrix == Matrix4.Identity) return;

            VertexData modelData = Design.Model;
            VertexData modelDataClone = modelData.Clone();

            Design.Paint(transMatrix.ExtractTranslation(), modelDataClone, rng);
            Design.AddShapes(underChunk, transMatrix);

            if (!Design.HasCustomPlacement)
            {
                var data = new InstanceData
                {
                    MeshCache = modelData,
                    Colors = modelDataClone.Colors.Clone(),
                    ExtraData = modelDataClone.ExtraData.Clone(),
                    TransMatrix = transMatrix
                };
                CacheManager.Check(data);
                underChunk.StaticBuffer.AddInstance(data);
                modelDataClone.Dispose();
            }
            else
            {
                Design.CustomPlacement(modelDataClone, transMatrix);
            }
        }
	}
}
