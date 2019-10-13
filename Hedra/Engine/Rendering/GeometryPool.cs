/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/10/2017
 * Time: 05:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Hedra.Engine.IO;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// One huge VBO with reduced fragmentation
    /// </summary>
    public class GeometryPool<T> : IDisposable where T : unmanaged
    {
        private readonly int _poolSize;
        public List<MemoryEntry> ObjectMap { get; }
        public VBO<T> Buffer { get; }
        
        public int AvailableMemory => TotalMemory - UsedMemory;

        public int UsedMemory => ObjectMap.Count == 0 ? 0 : ObjectMap[ObjectMap.Count-1].Offset + ObjectMap[ObjectMap.Count-1].Length;

        public int TotalMemory => _poolSize * TypeSizeInBytes;

        public int Count => UsedMemory / TypeSizeInBytes;

        public int TypeSizeInBytes { get; }
        
        
        public GeometryPool(int SizeInBytes, int TypeSizeInBytes, VertexAttribPointerType PointerType, BufferTarget BufferTarget = BufferTarget.ArrayBuffer, BufferUsageHint Hint = BufferUsageHint.StaticDraw)
        {
            ObjectMap = new List<MemoryEntry>();
            Buffer = new VBO<T>(new T[0], 0, PointerType, BufferTarget, Hint);

            this._poolSize = (int) SizeInBytes;
            this.TypeSizeInBytes = TypeSizeInBytes;
            
            Buffer.Update(new T[0], TotalMemory);
            
            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine($"GLError when creating GeometryPool<{typeof(T)}> {error}");
        }

        public void Bind()
        {
            Buffer.Bind();
        }
        
        public void Unbind()
        {
            Buffer.Unbind();
        }

        public Bitmap Draw()
        {
            int size = 18000 * TypeSizeInBytes;
            var bmp = new Bitmap(TotalMemory / size, 4);

            for (int i = 0; i < ObjectMap.Count; i++)
            {
                var entry = ObjectMap[i];
                var len = (entry.Offset + entry.Length) / size;
                var rng = new Random(i);
                var color = Color.FromArgb(255, rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256));

                for (int x = entry.Offset / size; x < Mathf.Clamp(len, 0, bmp.Width); x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        bmp.SetPixel(x, y, color);
                    }
                }
            }
            return bmp;
        }

        public void DrawAndSave()
        {
            var bmp = this.Draw();
            Directory.CreateDirectory(AssetManager.AppPath + "/Snapshots/");
            bmp.Save(AssetManager.AppPath + "Snapshots/"+typeof(T)+"___"+DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png", ImageFormat.Png);
            bmp.Dispose();
        }

        public MemoryEntry Update(NativeArray<T> Data, int SizeInBytes, MemoryEntry Entry)
        {            
            if(Entry.Length != SizeInBytes)
            {
                int Offset = 0;
                //Find a gap were this data can be inserted
                ObjectMap.Sort( MemoryEntry.Compare  );
                for (var i = 0; i < ObjectMap.Count; i++)
                {
                    
                    if(i == 0 && ObjectMap[i].Offset > SizeInBytes)
                    {
                        Offset = 0;
                        break;
                    }
                    
                    if(i == ObjectMap.Count-1)
                    {
                        Offset = ObjectMap[ObjectMap.Count-1].Offset + ObjectMap[ObjectMap.Count-1].Length;
                        break; 
                    }
                    
                    if(ObjectMap[i] != Entry && ObjectMap[i+1].Offset - (ObjectMap[i].Offset + ObjectMap[i].Length) >= SizeInBytes)
                    {
                        Offset = ObjectMap[i].Offset + ObjectMap[i].Length;
                        break;
                    }
                }
                if (Offset + SizeInBytes > TotalMemory)
                {
                    this.DrawAndSave();
                    throw new OutOfMemoryException("GeometryPool<T> is out of memory");
                }
                Entry.Offset = Offset;
                Entry.Length = SizeInBytes;
                if(ObjectMap.Contains(Entry))
                    ObjectMap.Remove(Entry);
                
                ObjectMap.Add(Entry);
            }
            
            Buffer.Update(Data.Pointer, Entry.Offset, SizeInBytes);
            return Entry;
        }

        public void UpdateBuffer(NativeArray<T> Data, int Offset, int SizeInBytes)
        {
            Buffer.Update(Data.Pointer, Offset, SizeInBytes);
        }
        
        public MemoryEntry Allocate(NativeArray<T> Data, int SizeInBytes)
        {
            return Update(Data, SizeInBytes, new MemoryEntry());
        }

        public void Discard()
        {
            ObjectMap.Clear();
            Buffer.Update(new T[0], TotalMemory);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
