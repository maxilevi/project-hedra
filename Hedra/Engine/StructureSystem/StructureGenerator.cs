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

		public StructureGenerator()
		{
			_items = new List<StructureWatcher>();
			SeedGenerator = new Voronoi();
		}

		public void CheckStructures(Vector2 ChunkOffset)
		{
			if (!World.IsChunkOffset(ChunkOffset))
				throw new ArgumentException("Provided paramater does not represent a valid offset");

			var underChunk = World.GetChunkAt(ChunkOffset.ToVector3());
			var region = underChunk != null
				? underChunk.Biome
				: World.BiomePool.GetRegion(ChunkOffset.ToVector3());
			var designs = region.Structures.Designs;
			for (var i = 0; i < designs.Length; i++)
			{
				if (designs[i].MeetsRequirements(ChunkOffset))
					designs[i].CheckFor(ChunkOffset, region);
			}
		}

		public void Build(CollidableStructure Struct)
		{
			Struct.Generated = true;
			Struct.Design.Build(Struct);
		}

		public void AddStructure(CollidableStructure Structure)
		{
			lock (_lock)
			{
				_items.Add(new StructureWatcher(Structure));
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
			}
		}

		public CollidableStructure[] Structures
		{
			get
			{
				lock(_lock) 
					return _items.Select(I => I.Structure).ToArray();
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
