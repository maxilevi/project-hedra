using System.Net;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public class P2PHost
    {
        public IPEndPoint IP;
        public int PlayerCount = 0;
        public int LastPing = 0;
        public string UUID = "";
        public List<IPEndPoint> Players = new List<IPEndPoint>();

        //google uh¿?
        private static readonly string[] Alphabet = new string[]{"a","b","c","d","e","f","g","h","i","j","k","l","m",
            "n","o","p","q","r","s","t","u","v","w","x","y","z"};
        public static string Identifier(int Seed, int ID)
        {
            Random Rng = new Random(Seed);
            StringBuilder Builder = new StringBuilder();
            Builder.Append(ID);
            int Length = Builder.Length;
            for (int i = 0; i < 5-Length; i++)
            {
                //string Code = Alphabet[Rng.Next(0, Alphabet.Length)];
                //if (Rng.Next(0, 2) == 0) Code = Code.ToUpperInvariant();
                Builder.Append(Rng.Next(0, 10));

            }
            return Builder.ToString();
        }
    }
}