using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public abstract class Builder
    {
        public abstract void Build(Dictionary<string, object> Input, string Output);

        public byte[] Zip(string Str)
        {
            var bytes = Encoding.UTF8.GetBytes(Str);
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        this.CopyTo(msi, gs);
                    }

                    return mso.ToArray();
                }
            }
        }

        public byte[] ZipBytes(byte[] Bytes)
        {
            using (var msi = new MemoryStream(Bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        this.CopyTo(msi, gs);
                    }
                    return mso.ToArray();
                }
            }
        }

        public void CopyTo(Stream Src, Stream Dest)
        {
            var bytes = new byte[4096];
            int cnt;
            while ((cnt = Src.Read(bytes, 0, bytes.Length)) != 0)
            {
                Dest.Write(bytes, 0, cnt);
            }
        }
    }
}
