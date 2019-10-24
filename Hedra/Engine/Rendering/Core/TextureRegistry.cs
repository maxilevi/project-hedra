using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public static class TextureRegistry
    {
        private const uint DefaultId = 0;
        private static readonly Dictionary<uint, TextureInformation> Textures = new Dictionary<uint, TextureInformation>();

        public static void Use(uint Id)
        {
            if (Id == 0) return;
            Textures[Id].Uses++;
        }

        public static void MarkStatic(uint Id)
        {
            Use(Id);
            Textures[Id].Static = true;
        }

        public static void Register(uint Id)
        {
            Textures.Add(Id, new TextureInformation
            {
                Path = TextureInformation.Default.Path,
                Uses = 0
            });
        }
        
        public static void Unregister(uint Id)
        {
            Textures.Remove(Id);
        }
        
        public static bool Contains(string Path, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap, out uint Id)
        {
            Id = 0;
            var cache = Textures.FirstOrDefault(P => P.Value.IsSame(Path, Min, Mag, Wrap));
            if (cache.Value == null) return false;
            Id = cache.Key;
            return true;
        }

        public static bool IsKnown(uint Id)
        {
            return DefaultId == Id || Textures[Id].IsDefault || Textures[Id].Uses > 0;
        }
        
        public static void Add(uint Id, string Path, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
        {
            if (Id == 0) throw new ArgumentOutOfRangeException();
            var information = new TextureInformation
            {
                Path = Path,
                Min = Min,
                Mag = Mag,
                Wrap = Wrap,
                Uses = 0
            };
            if (Textures.ContainsKey(Id))
            {
                if (Textures[Id].IsDefault)
                    Textures[Id] = information;
                else
                    Textures[Id].Uses++;
            }
            else
            {
                Textures.Add(Id, information);
            }
        }

        public static void Remove(uint Id)
        {
            if (Id == 0) return;
            Textures[Id].Uses--;
            if (Textures[Id].Uses == 0)
            {
                if(Textures[Id].Static)
                    throw new ArgumentOutOfRangeException();
                Dispose(Id);
            }
        }

        private static void Dispose(uint Id)
        {
            if (TextCache.Exists(Id)) throw new ArgumentOutOfRangeException("Textures should not be in any cache when disposing of them.");
            void DisposeProcess()
            {
                Renderer.TextureHandler.Delete(Id);
                Textures.Remove(Id);
            }
            if(System.Threading.Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                Executer.ExecuteOnMainThread(DisposeProcess);
            else
                DisposeProcess();
        }
        
        public static int Count => Textures.Count;
        public static uint[] All => Textures.Keys.ToArray();

        private class TextureInformation
        {
            private const string DefaultPath = "UNASSIGNED";
            public static readonly TextureInformation Default = new TextureInformation
            {
                Path = DefaultPath
            };
            
            public string Path { get; set; }
            public TextureMinFilter Min { get; set; }
            public TextureMagFilter Mag { get; set; }
            public TextureWrapMode Wrap { get; set; }
            public int Uses { get; set; }
            public bool Static { get; set; }
            private StackTrace _trace = new StackTrace();

            public bool IsSame(string Path, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
            {
                return this.Path == Path && this.Min == Min && this.Mag == Mag && this.Wrap == Wrap;
            }

            public bool IsDefault => Path == DefaultPath;
            
            public override string ToString()
            {
                return $"{Path}|{Uses}";
            }
        }
    }
}