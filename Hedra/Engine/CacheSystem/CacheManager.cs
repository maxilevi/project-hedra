/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 11:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Rendering;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.CacheSystem
{
	/// <summary>
	/// Description of CacheManager.
	/// </summary>
	public static class CacheManager
	{
		public static Dictionary< float, List<float> > CachedExtradata = new Dictionary< float, List<float> >();
		public static Dictionary< Vector4, List<Vector4> > CachedColors = new Dictionary< Vector4, List<Vector4> >();
        private static Dictionary<string, CacheType> _caches = new Dictionary<string, CacheType>();

        public static void Load(){
            _caches.Add(CacheItem.Grass.ToString().ToLowerInvariant(), new GrassCache());
            _caches.Add(CacheItem.AppleTrees.ToString().ToLowerInvariant(), new AppleTreesCache());
            _caches.Add(CacheItem.Bushes.ToString().ToLowerInvariant(), new BushCache());
            _caches.Add(CacheItem.DeadTrees.ToString().ToLowerInvariant(), new DeadTreeCache());
            _caches.Add(CacheItem.OakTrees.ToString().ToLowerInvariant(), new OakTreeCache());
            _caches.Add(CacheItem.PineTrees.ToString().ToLowerInvariant(), new PineTreesCache());
            _caches.Add(CacheItem.Rock.ToString().ToLowerInvariant(), new RockCache());
            _caches.Add(CacheItem.TallTrees.ToString().ToLowerInvariant(), new TallTreesCache());
            _caches.Add(CacheItem.Campfire.ToString().ToLowerInvariant(), new CampfireCache());
            _caches.Add(CacheItem.CypressTree.ToString().ToLowerInvariant(), new CypressTreesCache());
            _caches.Add(CacheItem.Ferns.ToString().ToLowerInvariant(), new FernCache());
            _caches.Add(CacheItem.Wheat.ToString().ToLowerInvariant(), new WheatCache());
            _caches.Add(CacheItem.Farms.ToString().ToLowerInvariant(), new FarmCache());
            _caches.Add(CacheItem.Cloud.ToString().ToLowerInvariant(), new CloudCache());
            _caches.Add(CacheItem.Berries.ToString().ToLowerInvariant(), new BerriesCache());
            _caches.Add(CacheItem.KnockedIcon.ToString().ToLowerInvariant(), new KnockedIconCache());
            _caches.Add(CacheItem.AttentionMark.ToString().ToLowerInvariant(), new AttentionCache());
            _caches.Add(CacheItem.BerryBush.ToString().ToLowerInvariant(), new BerryBushCache());
            _caches.Add(CacheItem.Mat.ToString().ToLowerInvariant(), new MatCache());
            _caches.Add(CacheItem.SleepingIcon.ToString().ToLowerInvariant(), new SleepingIconCache());
        }

	    public static VertexData GetModel(CacheItem Item)
	    {
	        return GetModel(Item.ToString().ToLowerInvariant());
	        
	    }

        public static VertexData GetModel(string Type)
        {
            return _caches[Type].GrabModel();

        }

	    public static List<CollisionShape> GetShape(VertexData Model)
	    {
	        foreach (var pair in _caches)
	        {
	            var result = GetShape(pair.Key, Model);
	            if (result != null) return result;

	        }
	        return null;
	    }


        public static List<CollisionShape> GetShape(CacheItem Item, VertexData Model)
		{
		    return GetShape(Item.ToString(), Model);
		}

	    public static List<CollisionShape> GetShape(string Type, VertexData Data)
	    {
	        return _caches[Type].GetShapes(Data);
	    }
		

	    public static void Check(InstanceData Data)
	    {
	        lock (CachedColors)
	        {
	            Vector4 CHash = MakeHash(Data.Colors);
	            if (CachedColors.ContainsKey(CHash))
	            {
	                goto COLOR_EXISTS;
	            }
	            CachedColors.Add(CHash, Data.Colors);

	            COLOR_EXISTS:
	            Data.ColorCache = CHash;
	            Data.Colors = new List<Vector4>();
	        }

	        lock (CachedExtradata)
	        {
	            float EHash = MakeHash(Data.ExtraData);
	            if (CachedExtradata.ContainsKey(EHash))
	            {
	                goto EXTRADATA_EXISTS;
	            }
	            CachedExtradata.Add(EHash, Data.ExtraData);

	            EXTRADATA_EXISTS:
	            Data.ExtraDataCache = EHash;
	            Data.ExtraData = new List<float>();
	        }
	    }

	    private static Vector4 MakeHash(List<Vector4> L)
	    {
	        Vector4 Hash = Vector4.Zero;
	        for (int i = 0; i < L.Count; i++)
	        {
	            Hash += L[i];
	        }
	        Hash /= L.Count;
	        return Hash;
	    }

	    private static float MakeHash(List<float> L)
	    {
	        float Hash = 0;
	        for (int i = 0; i < L.Count; i++)
	        {
	            Hash += L[i];
	        }
	        Hash /= L.Count;
	        return Hash - 10;
	    }
    }
}
