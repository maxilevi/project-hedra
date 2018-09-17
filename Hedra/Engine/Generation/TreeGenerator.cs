/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 04:26 a.m.
 *
 */
using System;
using Hedra.Engine.Rendering;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.TreeSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of TreeGenerator.
	/// </summary>
	public class TreeGenerator
	{
	    private readonly Vector3[] _previousTrees = new Vector3[8];

		public void GenerateTree(Vector3 Position, BiomeSystem.Region BiomeRegion, TreeDesign Design)
		{
		    Chunk underChunk = World.GetChunkAt(Position);
            if(underChunk == null) return;

			Random rng = new Random(World.Seed + 24121);
			var addon = new Vector3(rng.NextFloat() * 32f - 16f, 0, rng.NextFloat() * 32f - 16f);
		    Vector3 blockSpace = World.ToBlockSpace(Position);

		    const float valueFactor = 1.05f;
		    float spaceBetween;
		    float noiseValue;

            if (World.MenuSeed == World.Seed)
            {
                //This old noise doesnt support negative coordinates
                //And I will leave it here because the menu looks good with it.
			    spaceBetween = Position.X > 0 && Position.Z > 0 
                    ? SimplexNoise.Noise.Generate(Position.X * .001f, (Position.Z + 100) * .001f) * 75f
                    : int.MaxValue;
                noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 75f) * valueFactor)+.3f, 1.0f);
            }
            else
			{
			    spaceBetween = SpaceNoise(Position.X, Position.Z);
			    noiseValue = Math.Min(Math.Max(0, Math.Abs(spaceBetween / 40f) * valueFactor) + .3f, 1.0f);
            }
			if(spaceBetween < 0) spaceBetween = -spaceBetween * 16f;

		    if (World.MenuSeed != World.Seed)
		        spaceBetween += BiomeRegion.Trees.PrimaryDesign.Spacing;
		    else
		        spaceBetween = 80f;


			for(var i = 0; i < _previousTrees.Length; i++){
				if( (Position - _previousTrees[i] ).LengthSquared < spaceBetween * spaceBetween)
					return;	
			}

		    if (blockSpace.X + addon.X / Chunk.BlockSize > 15) addon.X = 0;
		    if (blockSpace.Z + addon.Z / Chunk.BlockSize > 15) addon.Z = 0;

		    float height = Physics.HeightAtPosition(Position + addon);
		    Vector3 normal = Physics.NormalAtPosition(Position + addon + Vector3.UnitY * height);

		    if (Vector3.Dot(normal, Vector3.UnitY) <= .2f)
                return;

            for (var i = _previousTrees.Length-1; i > 0; i--)
		    {
		        _previousTrees[i] = _previousTrees[i - 1];
		    }
		    _previousTrees[0] = Position;
			
			for(var x = -2; x < 2; x++)
			{
				for(var z = -2; z < 2; z++)
				{
					var bDens = Physics.HeightAtPosition(
                        new Vector3( (blockSpace.X+ x) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockSpace.Z+ z) * Chunk.BlockSize + underChunk.OffsetZ)
                        );

					var difference = Math.Abs(bDens - height);

					if(difference > 10)
						return;
				}
			}

            float extraScale = new Random(World.Seed + 1111).NextFloat() * 5 + 4;
			float scale = 10 + rng.NextFloat() * 3.5f;

			scale *= extraScale * .5f;
			scale += 8 + rng.NextFloat() * 4f;
		    scale *= noiseValue;

		    VertexData originalModel = Design.Model;
            VertexData model = originalModel.Clone();

		    Matrix4 transMatrix = Matrix4.CreateScale(new Vector3(scale, scale, scale) * 1.5f );
			transMatrix *=  Matrix4.CreateRotationY( rng.NextFloat() * 360f);
			transMatrix *= Matrix4.CreateTranslation( new Vector3(Position.X, height, Position.Z) + addon );

			float windRng = Utils.Rng.NextFloat(); 
			model.Extradata.AddRange( model.GenerateWindValues(AssetManager.ColorCode1, windRng) );
		    model.AddExtraData(AssetManager.ColorCode2, model.GenerateWindValues(AssetManager.ColorCode2, windRng));

            var shadow = new DropShadow
			{
			    Position = transMatrix.ExtractTranslation() + Vector3.UnitY * .1f,
                DepthTest = true,
			    DeleteWhen = () => underChunk.Disposed
            };
		    shadow.Rotation = new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(shadow.Position)));
            shadow.Scale *= 5f;

			Vector4 woodColor = rng.Next(0, 5) != 1
                ? BiomeRegion.Colors.WoodColors[new Random(World.Seed + 5232).Next(0, BiomeRegion.Colors.WoodColors.Length)] 
                : BiomeRegion.Colors.WoodColor;

		    Vector4 leafColor = rng.Next(0, 5) != 1 
		        ? BiomeRegion.Colors.LeavesColors[new Random(World.Seed + 42132).Next(0, BiomeRegion.Colors.LeavesColors.Length)] 
		        : BiomeRegion.Colors.LeavesColor;

		    model = Design.Paint(model, woodColor, leafColor);
		    model.GraduateColor(Vector3.UnitY);

            List<CollisionShape> shapes = Design.GetShapes(originalModel);
			foreach (CollisionShape originalShape in shapes)
			{
			    var shape = (CollisionShape) originalShape.Clone();
			    shape.Transform(transMatrix);
			    underChunk.AddCollisionShape( shape );
			}

		    var data = new InstanceData
		    {
		        ExtraData = model.Extradata,
		        MeshCache = originalModel,
		        Colors = model.Colors,
		        TransMatrix = transMatrix
		    };

		    CacheManager.Check(data);
			underChunk?.StaticBuffer?.InstanceElements?.Add(data);
		}

	    public float SpaceNoise(float X, float Z)
	    {
	        return (float) OpenSimplexNoise.Evaluate(X * .004f, (Z + 100) * .004f) * 40f;
	    }
    }
	
	public enum TreeType{
		Pine,
		Cypress,
		Oak,
		Tall,
        Apple,
		Dead
	}
}