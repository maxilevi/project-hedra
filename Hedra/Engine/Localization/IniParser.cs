using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Localization
{
    public static class IniParser
    {
        public static Dictionary<string, string> Parse(string Contents)
        {
            var dict = new Dictionary<string, string>();
            var lines = Contents.Split(Environment.NewLine.ToCharArray()).Where(S => !string.IsNullOrEmpty(S)).ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                if(lines[i].Trim().StartsWith("#")) continue;
                var parts = lines[i].Split('=');
                var key = parts[0].Trim();
                var val = parts[1].Trim();
                dict.Add(key, val);
            }
            return dict;
        }
    }
}