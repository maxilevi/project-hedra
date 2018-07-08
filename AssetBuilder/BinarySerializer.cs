﻿using System.Collections.Generic;
using System.IO;

namespace AssetBuilder
{
    public class BinarySerializer : Serializer
    {
        public override Dictionary<string, object> Serialize(string[] Files)
        {
            var sortedInput = new List<string>(Files);
            sortedInput.Sort(new FileSizeComparer());
            var output = new Dictionary<string, object>();      
            for (var i = 0; i < sortedInput.Count; i++)
            {
                var data = File.ReadAllBytes(sortedInput[i]);
                output.Add(sortedInput[i], data);
            }
            return output;
        }
    }

    public class FileSizeComparer : IComparer<string>
    {
        public int Compare(string A, string B)
        {
            long s1 = new FileInfo(A).Length;
            long s2 = new FileInfo(B).Length;

            if (s1 == s2) return 0;
            if (s1 > s2) return 1;

            return -1;
        }
    }
}