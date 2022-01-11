using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class BinarySerializer : Serializer
    {

        private const long MB1 = 1024 * 1024;
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

            var sortedFiles = Files.OrderBy(F => new FileInfo(F).Length).ToArray();
            var tasks = new List<Task<AssetBuild>>();
            for (var i = 0; i < sortedFiles.Length; i++)
            {
                var info = new FileInfo(sortedFiles[i]);
                var k = i;
                tasks.Add(info.Length < MB1 
                    ? Task.Run(() => SerializeFile(sortedFiles[k]))
                    : Task.FromResult(SerializeFile(sortedFiles[k])));
                //if(info.Length > MB1)
                //    Console.WriteLine($"Launching file {sortedFiles[k]} as a task");
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
