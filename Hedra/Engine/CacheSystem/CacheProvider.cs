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
using Hedra.Engine.IO;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    /// <summary>
    /// Description of CacheManager.
    /// </summary>
    public class CacheProvider : ICacheProvider
    {
        public Dictionary<float, List<CompressedValue<float>>> CachedExtradata { get; private set; }
        public Dictionary<Vector4, List<CompressedValue<Vector4>>> CachedColors { get; private set; }
        private readonly Dictionary<string, CacheType> _caches = new Dictionary<string, CacheType>();
        private readonly object _colorLock = new object();
        private readonly object _extradataLock = new object();

        public void Load()
        {
            CachedExtradata = new Dictionary<float, List<CompressedValue<float>>>();
            CachedColors =  new Dictionary<Vector4, List<CompressedValue<Vector4>>>();
            var foundTypes = new HashSet<CacheItem>();
            var typeList = Assembly.GetExecutingAssembly().GetLoadableTypes(this.GetType().Namespace).ToArray();
            foreach (var type in typeList)
            {
                if(!type.IsSubclassOf(typeof(CacheType)) || Attribute.GetCustomAttribute(type, typeof(CacheIgnore)) != null) continue;

                var cache = (CacheType) Activator.CreateInstance(type);
                foundTypes.Add(cache.Type);
                Log.WriteLine($"Loading {cache.GetType().Name} into cache as {cache.Type.ToString()}...", LogType.System);
                _caches.Add(cache.Type.ToString().ToLowerInvariant(), cache);
            }

            for (var i = 0; i < (int)CacheItem.MaxEnums; i++)
            {
                var item = (CacheItem)i;
                if (!foundTypes.Contains(item))
                    throw new ArgumentException($"No valid cache type found for {item}");        
            }
            Log.WriteLine("Finished building cache.");
        }

        public VertexData GetModel(string Type)
        {
            return _caches[Type].GrabModel();

        }

        public List<CollisionShape> GetShape(VertexData Model)
        {
            foreach (var pair in _caches)
            {
                var result = GetShape(pair.Key, Model);
                if (result != null) return result;

            }
            return null;
        }

        public List<CollisionShape> GetShape(string Type, VertexData Data)
        {
            return _caches[Type].GetShapes(Data);
        }

        public void Discard()
        {
            CachedColors.Clear();
            CachedExtradata.Clear();
        }

        public void Check(InstanceData Data)
        {
            lock (_colorLock)
            {
                Vector4 cHash = MakeHash(Data.Colors);
                if (CachedColors.ContainsKey(cHash))
                {
                    goto COLOR_EXISTS;
                }

                var cache = new List<CompressedValue<Vector4>>();
                Data.Colors.Compress(cache);
                CachedColors.Add(cHash, cache);

                COLOR_EXISTS:
                Data.ColorCache = cHash;
                Data.Colors = new List<Vector4>();
            }
            if (Data.HasExtraData)
            {
                lock (_extradataLock)
                {
                    float eHash = MakeHash(Data.ExtraData);
                    if (CachedExtradata.ContainsKey(eHash))
                    {
                        goto EXTRADATA_EXISTS;
                    }
                    var cache = new List<CompressedValue<float>>();
                    Data.ExtraData.Compress(cache);
                    CachedExtradata.Add(eHash, cache);

                    EXTRADATA_EXISTS:
                    Data.ExtraDataCache = eHash;
                    Data.ExtraData = new List<float>();
                }
            }
        }

        private static Vector4 MakeHash(List<Vector4> L)
        {
            Vector4 hash = Vector4.Zero;
            for (var i = 0; i < L.Count; i++)
            {
                hash += L[i];
            }
            hash /= (L.Count+1);
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
