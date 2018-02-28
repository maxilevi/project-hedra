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
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace ShaderCompiler
{
    /// <summary>
    /// Description of Zip.
    /// </summary>
    public static class ZipManager
    {

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string UnZip(byte[] bytes)
        {
            byte[] dataBuffer = new byte[4096];

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (GZipInputStream gzipStream = new GZipInputStream(ms))
                {
                    using (MemoryStream mso = new MemoryStream())
                    {
                        StreamUtils.Copy(gzipStream, mso, dataBuffer);
                        return Encoding.UTF8.GetString(mso.ToArray());
                    }
                }
            }
        }

        public static byte[] ZipBytes(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static byte[] UnZipBytes(byte[] bytes)
        {
            byte[] dataBuffer = new byte[4096];

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (GZipInputStream gzipStream = new GZipInputStream(ms))
                {
                    using (MemoryStream mso = new MemoryStream())
                    {
                        StreamUtils.Copy(gzipStream, mso, dataBuffer);
                        return mso.ToArray();
                    }
                }
            }
        }

    }
}
