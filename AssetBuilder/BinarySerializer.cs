using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class BinarySerializer : Serializer
    {

        private AssetBuild SerializeFile(string Path)
        {
            var data = this.Process(Path);
            return new AssetBuild
            {
                Path = Path,
                Data = data,
                Checksum = AssetBuild.CreateHash(Path)
            };
        }

        public override SerializationOutput Serialize(string[] Files)
        {
            var sw = new Stopwatch();
            sw.Start();
            var tasks = new List<Task<AssetBuild>>();
            for (var i = 0; i < Files.Length; i++)
            {
                var k = i;
                tasks.Add(Task.Run(() => SerializeFile(Files[k])));
            }
            Console.WriteLine($"Launched {tasks.Count} tasks");

            tasks.ForEach(T => T.Wait());

            var builds = tasks
                .Select(T => T.Result)
                .OrderBy(A => A.Size);

            sw.Stop();
            Console.WriteLine($"All tasks finished, took {sw.ElapsedMilliseconds / 1000f} seconds");
            return new SerializationOutput
            {
                Results = builds.ToDictionary(A => A.Path, A => (object)A.Data),
                History = new BuildHistory
                {
                    Builds = builds.ToArray()
                }
            };
        }

        private byte[] Process(string Filename)
        {
            switch (Path.GetExtension(Filename)?.ToLowerInvariant())
            {
                 case ".ply":
                     return PLYProcessor.Process(Filename);
                 default:
                     return File.ReadAllBytes(Filename);
            }
        }
    }
}
