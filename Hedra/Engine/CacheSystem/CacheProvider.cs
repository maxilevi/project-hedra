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
        public Dictionary<string, List<CompressedValue<float>>> CachedExtradata { get; private set; }
        public Dictionary<string, List<CompressedValue<Vector4>>> CachedColors { get; private set; }
        private readonly Dictionary<string, CacheType> _caches = new Dictionary<string, CacheType>();
        private readonly object _colorLock = new object();
        private readonly object _hashLock = new object();
        private readonly object _extradataLock = new object();
        private MD5 _hasher;
        private object _asd;

        public void Load()
        {
            CachedExtradata = new Dictionary<string, List<CompressedValue<float>>>(StringComparer.OrdinalIgnoreCase);
            CachedColors =  new Dictionary<string, List<CompressedValue<Vector4>>>(StringComparer.OrdinalIgnoreCase);
            _hasher = MD5.Create();
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
                    var eHash = MakeHash(Data.ExtraData);
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

        public string MakeHash(List<Vector4> Colors)
        {
            var floatArray = Colors.SelectMany(C => new []
            {
                C.X, C.Y, C.Z, C.W
            }).ToArray();
            var byteArray = new byte[floatArray.Length * sizeof(float)];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return MakeHash(byteArray);
        }

        public string MakeHash(List<float> Extradata)
        {
            var byteArray = new byte[Extradata.Count * sizeof(float)];
            Buffer.BlockCopy(Extradata.ToArray(), 0, byteArray, 0, byteArray.Length);
            return MakeHash(byteArray);
        }

        private string MakeHash(byte[] Bytes)
        {
            var hash = (byte[]) null;
            lock (_hashLock)
                hash = _hasher.ComputeHash(Bytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
