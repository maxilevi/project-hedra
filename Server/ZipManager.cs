/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 06:03 a.m.
 *
 */
using System;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace Server
{
	/// <summary>
	/// Description of Zip.
	/// </summary>
	public static class ZipManager
	{
				
		public static void CopyTo(Stream src, Stream dest) {
		    byte[] bytes = new byte[4096];
		
		    int cnt;
		
		    while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
		        dest.Write(bytes, 0, cnt);
		    }
		}
		
		public static byte[] Zip(string str) {
		    var bytes = Encoding.UTF8.GetBytes(str);
		
		    using (var msi = new MemoryStream(bytes))
		    using (var mso = new MemoryStream()) {
		        using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
		            //msi.CopyTo(gs);
		            CopyTo(msi, gs);
		        }
		
		        return mso.ToArray();
		    }
		}
		
		public static string UnZip(byte[] bytes) {
		    using (var msi = new MemoryStream(bytes))
		    using (var mso = new MemoryStream()) {
		        using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
		            //gs.CopyTo(mso);
		            CopyTo(gs, mso);
		        }
		
		        return Encoding.UTF8.GetString(mso.ToArray());
		    }
		}
		
				
		
		public static byte[] ZipBytes(byte[] bytes) {
		    using (var msi = new MemoryStream(bytes))
		    using (var mso = new MemoryStream()) {
		        using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
		            //msi.CopyTo(gs);
		            CopyTo(msi, gs);
		        }
		
		        return mso.ToArray();
		    }
		}
		
		public static byte[] UnZipBytes(byte[] bytes) {
		    using (var msi = new MemoryStream(bytes))
		    using (var mso = new MemoryStream()) {
		        using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
		            //gs.CopyTo(mso);
		            CopyTo(gs, mso);
		        }
		
		        return mso.ToArray();
		    }
		}
		
		public static byte[] ToByteArray(this Int32 intValue){
			byte[] intBytes = BitConverter.GetBytes(intValue);
			if (BitConverter.IsLittleEndian)
			    Array.Reverse(intBytes);
			byte[] result = intBytes;
			
			return result;
		}
		
		public static int ToInt32(this byte[] ArrayB){
			if (BitConverter.IsLittleEndian)
			    Array.Reverse(ArrayB);
			return BitConverter.ToInt32(ArrayB,0);
		}
		
	}
}
