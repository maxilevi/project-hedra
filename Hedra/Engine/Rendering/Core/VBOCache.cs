using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Game;
using IronPython.Runtime;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using OpenToolkit.Graphics.EXT;
using Buffer = System.Buffer;

namespace Hedra.Engine.Rendering.Core
{
    public static class VBOCache
    {
        private static readonly HashSet<uint> _uncachedVBOs;
        private static readonly Dictionary<string, uint> _hashedReferences;
        private static readonly Dictionary<uint, int> _referenceCounter;
        /* Maybe use SHA256 since it does not have collisions */
        private static readonly MD5 _hasher;

        static VBOCache()
        {
            _hasher = MD5.Create();
            _hashedReferences = new Dictionary<string, uint>();
            _referenceCounter = new Dictionary<uint, int>();
            _uncachedVBOs = new HashSet<uint>();
        }
        
        public static bool Exists<T>(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTargetARB BufferTarget, BufferUsageARB Hint, out uint Id)
        {
            Id = 0;
            if (GameSettings.TestingMode) return true;
            var hash = Hash(Data, SizeInBytes, PointerType, BufferTarget, Hint);
            if (_hashedReferences.ContainsKey(hash))
            {
                Id = _hashedReferences[hash];
                _referenceCounter[Id]++;
                return true;
            }
            return false;
        }

        public static void Create<T>(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTargetARB BufferTarget, BufferUsageARB Hint, out uint Id) where T : unmanaged
        {
            var hash = Hash(Data, SizeInBytes, PointerType, BufferTarget, Hint);
            DoCreate(Data, SizeInBytes, PointerType, BufferTarget, Hint, out Id);
            _hashedReferences.Add(hash, Id);
        }
        
        private static void DoCreate<T>(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTargetARB BufferTarget, BufferUsageARB Hint, out uint Id) where T : unmanaged
        {
            Renderer.GenBuffers(1, out Id);
            DoUpdate(Data, SizeInBytes, BufferTarget, Hint, Id);
            _referenceCounter.Add(Id, 1);
        }

        public static void Update<T>(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTargetARB BufferTarget, BufferUsageARB Hint, ref uint Id) where T : unmanaged
        {
            var originalId = Id;
            if(Id == 0) throw new ArgumentOutOfRangeException($"VBO is invalid (disposed)");
            if (_referenceCounter[originalId] == 1)
            {
                var previousHash = _uncachedVBOs.Contains(originalId) ? null : HashFromId(originalId);
                DoUpdate(Data, SizeInBytes, BufferTarget, Hint, originalId);
                if (previousHash != null)
                {
                    _hashedReferences.Remove(previousHash);
                    _uncachedVBOs.Add(Id);
                }
            }
            else
            {
                Delete(ref originalId);
                DoCreate(Data, SizeInBytes, PointerType, BufferTarget, Hint, out Id);
                _uncachedVBOs.Add(Id);
            }
        }

        private static void DoUpdate<T>(T[] Data, int SizeInBytes, BufferTargetARB Target, BufferUsageARB Hint, uint Id) where T : unmanaged
        {
            Renderer.BindBuffer(Target, Id);
            Renderer.BufferData(Target, (IntPtr) SizeInBytes, IntPtr.Zero, Hint);
            Renderer.BufferSubData(Target, IntPtr.Zero, (IntPtr) SizeInBytes, Data);
            Renderer.BindBuffer(Target, 0);
        }

        private static string HashFromId(uint Id)
        {
            return _hashedReferences.First(P => P.Value == Id).Key;
        }

        public static void Delete(ref uint Id)
        {
            if(Id == 0) return;
            _referenceCounter[Id]--;
            if (_referenceCounter[Id] == 0)
            {
                Renderer.DeleteBuffers(1, ref Id);
                _referenceCounter.Remove(Id);
                if (!_uncachedVBOs.Contains(Id))
                    _hashedReferences.Remove(HashFromId(Id));
                else
                    _uncachedVBOs.Remove(Id);
            }
            Id = 0;
        }

        private static string Hash<T>(T[] Data, int SizeInBytes, VertexAttribPointerType PointerType, BufferTargetARB BufferTarget, BufferUsageARB Hint)
        {
            var size = Data.Length != 0 ? Marshal.SizeOf(Data[0]) : 0;
            var byteArray = new byte[size * Data.Length + 4 * sizeof(int)];
            CopyTo(Data, byteArray);
            Buffer.BlockCopy(
                new [] { SizeInBytes, (int)PointerType, (int)BufferTarget, (int)Hint },
                0,
                byteArray,
                size * Data.Length,
                sizeof(int) * 4
            );
            return Hash(byteArray);
        }

        private static string Hash(byte[] Bytes)
        {
            var hash = _hasher.ComputeHash(Bytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
        
        private static void CopyTo<T>(T[] Structures, byte[] Array)
        {
            if (Structures.Length == 0) return;
            var size = Marshal.SizeOf(Structures[0]);
            var ptr = Marshal.AllocHGlobal(size);
            var offset = 0;
            for (var i = 0; i < Structures.Length; ++i)
            {
                Marshal.StructureToPtr(Structures[i], ptr, true);
                Marshal.Copy(ptr, Array, offset, size);
                offset += size;
            }
            Marshal.FreeHGlobal(ptr);
        }

        public static int CachedVBOs => _referenceCounter.Count;
    }
}