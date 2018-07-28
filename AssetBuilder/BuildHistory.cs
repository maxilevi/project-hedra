using System;
using System.Collections.Generic;
using System.Text;

namespace AssetBuilder
{
    public class BuildHistory
    {
        public AssetBuild[] Builds { get; set; }

        public static BuildHistory From(string[] Lines)
        {
            var list = new List<AssetBuild>();
            for (var i = 0; i < Lines.Length; i++)
            {
                var parts = Lines[i].Split('=');
                if (parts.Length != 2) return null;
                list.Add(new AssetBuild
                {
                    Path = parts[0].Trim(),
                    Checksum = parts[1].Trim()
                });
            }
            return new BuildHistory
            {
                Builds = list.ToArray()
            };
        }

        public static string To(BuildHistory History)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < History.Builds.Length; i++)
            {
                var build = History.Builds[i];
                builder.AppendLine($"{build.Path} = {build.Checksum}");
            }
            return builder.ToString();
        }
    }
}