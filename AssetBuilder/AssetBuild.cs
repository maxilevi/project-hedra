using System;
using System.IO;
using System.Security.Cryptography;

namespace AssetBuilder
{
    public class AssetBuild
    {
        public string Path { get; set; }
        public string Checksum { get; set; }

        public static string CreateHash(string Path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Path))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}