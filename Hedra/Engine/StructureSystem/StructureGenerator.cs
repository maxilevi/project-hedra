  /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.StructureSystem
{
	/// <summary>
	/// Description of StructureGenerator.
	/// </summary>
	public class StructureGenerator
	{
		private readonly object _lock = new object();
		public Vector3 MerchantPosition { get; set; }
		public bool MerchantSpawned { get; set; }
		public Voronoi SeedGenerator { get; }
		private readonly List<StructureWatcher> _items;
		private CollidableStructure[] _structures;
		private bool _dirtyStructures;

		public StructureGenerator()
		{
			_items = new List<StructureWatcher>();
			SeedGenerator = new Voronoi();
		}

		public static void CheckStructures(Vector2 ChunkOffset)
		{
			if (!World.IsChunkOffset(ChunkOffset))
				throw new ArgumentException("Provided paramater does not represent a valid offset");

			var distribution = new RandomDistribution();
			var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
			var region = underChunk != null
				? underChunk.Biome
				: World.BiomePool.GetRegion(ChunkOffset.ToVector3());
			var designs = region.Structures.Designs;
			for (var i = 0; i < designs.Length; i++)
			{
				if (designs[i].MeetsRequirements(ChunkOffset))
					designs[i].CheckFor(ChunkOffset, region, distribution);
			}
		}

		public void Build(CollidableStructure Struct)
		{
			Struct.Design.Build(Struct);
		}

		public void AddStructure(CollidableStructure Structure)
		{
			lock (_lock)
			{
				_items.Add(new StructureWatcher(Structure));
				_dirtyStructures = true;
			}
		}

		public void Discard()
		{
			this.MerchantPosition = Vector3.Zero;
			this.MerchantSpawned = false;
			lock (_lock)
			{
				for (var i = _items.Count - 1; i > -1; i--)
				{
					_items[i].Dispose();
					_items.RemoveAt(i);
				}
				_items.Clear();
				_dirtyStructures = true;
			}
		}

		public CollidableStructure[] Structures
		{
			get
			{
				lock (_lock)
				{
					if (_dirtyStructures || _structures == null)
					{
						_structures = _items.Select(I => I.Structure).ToArray();
						_dirtyStructures = false;
					}
					return _structures;
				}
			}
		}

		public StructureWatcher[] Watchers
		{
			get
			{
				lock(_lock) 
					return _items.ToArray();
			}
		}

		public static Type[] GetTypes()
		{
			var types = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(StructureGenerator).Namespace).ToArray();
			return types.Where(T => T.IsSubclassOf(typeof(StructureDesign)) && !T.IsAbstract).ToArray();
		}
	}
}
