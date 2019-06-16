/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/12/2016
 * Time: 03:44 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Management;
using System.Drawing;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of GIF.
    /// </summary>
    public class GIF
    {
        public float Speed {get; set;}
        private uint[] Textures;
        private float Frame;
        
        public GIF(string File){
            Speed = 24;// 24 frames per second
            
            Dictionary<string, byte[]> Frames = new Dictionary<string, byte[]>();
            List<string> FrameNames = new List<string>();
            
            using (MemoryStream Ms = new MemoryStream( AssetManager.ReadBinary(File,AssetManager.AssetsResource) ))
            {
                using (BinaryReader Reader = new BinaryReader(Ms))
                {
                    while (Reader.BaseStream.Position < Reader.BaseStream.Length)
                       {
                        string Header = Reader.ReadString();
                        int ChunkSize = Reader.ReadInt32();
                        byte[] Data = Reader.ReadBytes(ChunkSize);
                        Frames.Add(Header, Data);
                        FrameNames.Add(Header);
                        
                    }                
                }
            }
            FrameNames.Sort( new FrameSorter() );
            Textures = new uint[FrameNames.Count];
            for(int i = 0; i < FrameNames.Count; i++){
                Bitmap Bmp = new Bitmap( new MemoryStream(Frames[ FrameNames[i] ]) );
                //Textures[i] = Graphics2D.LoadTexture( Bmp );
            }
        }
        
        public uint CurrentFrame{
            get{
                Frame += Time.IndependentDeltaTime * Speed;
                if( (int) Frame >= Textures.Length)
                    Frame = 0;
                return Textures[ (int) Frame ];
            }
        }
    }
    
    public class FrameSorter : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            if ( int.Parse(Path.GetFileNameWithoutExtension(a)) == int.Parse(Path.GetFileNameWithoutExtension(b)) )
                return 0;
                
            if ( int.Parse(Path.GetFileNameWithoutExtension(a)) < int.Parse(Path.GetFileNameWithoutExtension(b)) )
                return -1;
    
            return 1;
        }
    }
}
