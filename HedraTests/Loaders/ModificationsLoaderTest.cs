using System.IO;
using System.Net;
using Hedra.API;
using Hedra.Engine.Loader;
using NUnit.Framework;

namespace HedraTests.Loaders
{
    [TestFixture]
    public class ModificationsLoaderTest
    {
        private string[] _dirs;
        private string[] _files;
        
        
        [SetUp]
        public void Setup()
        {
            var path = ModificationsLoader.Path;
            _dirs = new[]
            {
                $"{path}/test/",
                $"{path}/test2/"
            };
            _files = new[]
            {
                $"{path}/test/test2.json",
                $"{path}/test2/test3.json",
                $"{path}/test/test4.json",
                $"{path}/test/test4.meta",
                $"{path}/test5.json",
                $"{path}/test/test6.json",
                $"{path}/test/test6.meta"
                
            };
            foreach (var dir in _dirs)
            {
                Directory.CreateDirectory(dir);
            }
            foreach (var file in _files)
            {
                File.WriteAllText(file, string.Empty);
            }
        }
        
        [Test]
        public void TestLoadersLoadsTheExpectedFiles()
        {
            Assert.AreEqual(new []
            {
               $"{ModificationsLoader.Path}test/test2.json",
               $"{ModificationsLoader.Path}test/test4.json",
               $"{ModificationsLoader.Path}test/test4.meta",
               $"{ModificationsLoader.Path}test/test6.json",
               $"{ModificationsLoader.Path}test/test6.meta"
            },
                ModificationsLoader.Get("/test/")
            );
        }
        
        [Test]
        public void TestLoadersLoadsTheExpectedFilesByExtension()
        {
            Assert.AreEqual(new []
                {
                    $"{ModificationsLoader.Path}test5.json",
                    $"{ModificationsLoader.Path}test/test2.json",
                    $"{ModificationsLoader.Path}test/test4.json",
                    $"{ModificationsLoader.Path}test/test6.json",
                    $"{ModificationsLoader.Path}test2/test3.json"
                },
                ModificationsLoader.Get(".json")
            );
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var file in _files)
            {
                File.Delete(file);
            }
            foreach (var dir in _dirs)
            {
                Directory.Delete(dir);
            }
        }
    }
}