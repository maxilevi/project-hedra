    /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 11:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Hedra.Engine.Rendering;
using Hedra.Engine.PhysicsSystem;
using System.Reflection;
using Hedra.Engine.IO;
using Hedra.Rendering;
using System.Security.Cryptography;
using System.Text;

namespace Hedra.Engine.CacheSystem
{
    /// <summary>
    /// Description of CacheManager.
    /// </summary>
    public class CacheProvider : ICacheProvider
    {
        public Dictionary<object, List<float>> CachedExtradata { get; private set; }
        public Dictionary<object, List<Vector4>> CachedColors { get; private set; }
        private readonly Dictionary<string, CacheType> _caches = new Dictionary<string, CacheType>();
        private readonly object _colorLock = new object();
        private readonly object _extradataLock = new object();

        public void Load()
        {
            CachedExtradata = new Dictionary<object, List<float>>();
            CachedColors =  new Dictionary<object, List<Vector4>>();
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

        public VertexData GetPart(string Type, VertexData Model)
        {
            return _caches[Type].GetPart(Model);
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
                var cHash = MakeHash(Data.Colors);
                lock (_colorLock)
                {
                    if (CachedColors.ContainsKey(cHash))
                    {
                        goto COLOR_EXISTS;
                    }

                    CachedColors.Add(cHash, new List<Vector4>(Data.Colors));
                }

                COLOR_EXISTS:
                Data.ColorCache = cHash;
                Data.Colors = null;
            }

            if (Data.HasExtraData)
            {
                var eHash = MakeHash(Data.ExtraData);
                lock (_extradataLock)
                {
                    if (CachedExtradata.ContainsKey(eHash))
                    {
                        goto EXTRADATA_EXISTS;
                    }

                    CachedExtradata.Add(eHash, new List<float>(Data.ExtraData));
                }

                EXTRADATA_EXISTS:
                Data.ExtraDataCache = eHash;
                Data.ExtraData = null;
            }
        }

        public static object MakeHash(IList<Vector4> Colors)
        {
            var sum = default(Vector4);
            for (var i = 0; i < Colors.Count; ++i)
            {
                sum += Colors[i];
            }
            return sum * Colors.Count;
        }

        public static object MakeHash(IList<float> Extradata)
        {
            var sum = default(float);
            for (var i = 0; i < Extradata.Count; ++i)
            {
                sum += Extradata[i];
            }
            return sum * Extradata.Count;
        }
    }
}
