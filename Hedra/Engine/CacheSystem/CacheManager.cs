/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 11:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering;
using Hedra.Engine.PhysicsSystem;
using System.Reflection;

namespace Hedra.Engine.CacheSystem
{
	/// <summary>
	/// Description of CacheManager.
	/// </summary>
	public static class CacheManager
	{
		public static Dictionary< float, List<float> > CachedExtradata = new Dictionary< float, List<float> >();
		public static Dictionary< Vector4, List<Vector4> > CachedColors = new Dictionary< Vector4, List<Vector4> >();
        private static readonly Dictionary<string, CacheType> _caches = new Dictionary<string, CacheType>();

        public static void Load()
        {
            Type[] typeList = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(CacheManager).Namespace).ToArray();
            foreach (Type type in typeList)
            {
                if(!type.Name.EndsWith("Cache") || Attribute.GetCustomAttribute(type, typeof(CacheIgnore)) != null) continue;

                var item = CacheItem.MaxEnums;
                for (var i = 0; i < (int) CacheItem.MaxEnums; i++)
                {
                    var cache = (CacheItem) i;
                    if (string.Equals(cache + "Cache", type.Name))
                    {
                        item = cache;
                        break;
                    }
                }
                //Log.WriteLine("Loading {0} into cache...", type);
                if(item == CacheItem.MaxEnums) throw new ArgumentException("No valid cache type found for "+type);
                _caches.Add(item.ToString().ToLowerInvariant(), (CacheType) Activator.CreateInstance(type));
            }
            Log.WriteLine("Finished building cache.");
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
	            Vector4 cHash = MakeHash(Data.Colors);
	            if (CachedColors.ContainsKey(cHash))
	            {
	                goto COLOR_EXISTS;
	            }
	            CachedColors.Add(cHash, Data.Colors);

	            COLOR_EXISTS:
	            Data.ColorCache = cHash;
	            Data.Colors = new List<Vector4>();
	        }

	        lock (CachedExtradata)
	        {
	            float eHash = MakeHash(Data.ExtraData);
	            if (CachedExtradata.ContainsKey(eHash))
	            {
	                goto EXTRADATA_EXISTS;
	            }
	            CachedExtradata.Add(eHash, Data.ExtraData);

	            EXTRADATA_EXISTS:
	            Data.ExtraDataCache = eHash;
	            Data.ExtraData = new List<float>();
	        }
	    }

	    private static Vector4 MakeHash(List<Vector4> L)
	    {
	        Vector4 hash = Vector4.Zero;
	        for (int i = 0; i < L.Count; i++)
	        {
	            hash += L[i];
	        }
	        hash /= L.Count;
	        return hash;
	    }

	    private static float MakeHash(List<float> L)
	    {
	        float hash = 0;
	        for (int i = 0; i < L.Count; i++)
	        {
	            hash += L[i];
	        }
	        hash /= L.Count;
	        return hash - 10;
	    }
    }
}
