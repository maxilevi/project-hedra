using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Framework;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Engine.Management
{
    public class CompressedAssetProvider : IAssetProvider
    {
        private readonly object _handlerLock = new object();
        private readonly object _hitboxCacheLock = new object();
        private string _appData;
        private string _appPath;
        private bool _filesDecompressed;
        private Dictionary<string, VertexData> _hitboxCache;
        private List<ResourceHandler> _registeredHandlers;
        private string _uniqueId;

        public string ShaderCode { get; private set; }
        public string TemporalFolder { get; private set; }
        public string ShaderResource => "data1.db";
        public string SoundResource => "data2.db";
        public string AssetsResource => "data3.db";

        public void Load()
        {
            var boldFonts = new FontCollection();
            var normalFonts = new FontCollection();

            var sansBold = ReadBinary("Assets/ClearSans-Bold.ttf", AssetsResource);
            boldFonts.Install(new MemoryStream(sansBold));

            var sansRegular = ReadBinary("Assets/ClearSans-Regular.ttf", AssetsResource);
            normalFonts.Install(new MemoryStream(sansRegular));

            FontCache.SetFonts(normalFonts.Families.First(), boldFonts.Families.First());

            ReloadShaderSources();
            lock (_hitboxCacheLock)
            {
                _hitboxCache = new Dictionary<string, VertexData>();
            }
        }

        public void ReloadShaderSources()
        {
            ShaderCode = ZipManager.UnZip(File.ReadAllBytes(AppPath + ShaderResource));
        }

        public void GrabShaders()
        {
            CopyShaders();
            Log.Write($"[DEBUG] Rebuilding shader bundles...{Environment.NewLine}", ConsoleColor.Magenta);
            var pProcess = new Process
            {
                StartInfo =
                {
                    FileName = $@"{AppPath}../../../../AssetBuilder/bin/Release/netcoreapp6.0/AssetBuilder.exe",
                    Arguments = $"\"{AppPath}/Shaders/\" \"{AppPath}/data1.db\" normal text",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            pProcess.Start();
            Log.Write($"{Environment.NewLine}[DEBUG]{pProcess.StandardOutput.ReadToEnd()}{Environment.NewLine}",
                ConsoleColor.Magenta);
            pProcess.WaitForExit();
            pProcess.Dispose();
        }

        public byte[] ReadPath(string Path, bool Text = true)
        {
            var external = !Path.Contains("$DataFile$");
            if (!external || Path.StartsWith("Assets/") || Path.StartsWith("Assets\\"))
            {
                Path = Path.Replace("$DataFile$", string.Empty);
                if (Path.StartsWith("/"))
                    Path = Path.Substring(1, Path.Length - 1);

                return ReadBinary(Path, Text ? AssetsResource : SoundResource);
            }
#if DEBUG
            if (!GameSettings.TestingMode)
                throw new ArgumentOutOfRangeException("You shouldn't read from external paths when on debug mode.");
#endif
            var finalPath = Path.Replace("$GameFolder$", GameLoader.AppPath);
            return Text
                ? Encoding.ASCII.GetBytes(File.ReadAllText(finalPath))
                : File.ReadAllBytes(finalPath);
        }

        public byte[] ReadBinary(string Name, string DataFile)
        {
            DecompileAssets();
            ResourceHandler selectedHandler = null;
            lock (_handlerLock)
            {
                for (var i = 0; i < _registeredHandlers.Count; i++)
                    if (!_registeredHandlers[i].Locked && _registeredHandlers[i].Id == DataFile)
                    {
                        selectedHandler = _registeredHandlers[i];
                        selectedHandler.Locked = true;
                        break;
                    }
            }

            if (selectedHandler == null)
            {
                lock (_handlerLock)
                {
                    selectedHandler = new ResourceHandler(
                        File.OpenRead(GetResourceName(DataFile)),
                        DataFile
                    );
                    selectedHandler.Stream.Position = 0;
                    selectedHandler.Locked = true;
                    _registeredHandlers.Add(selectedHandler);
                }

                Log.WriteLine($"Registered resource handler... (Total = {_registeredHandlers.Count})", LogType.IO);
            }

            try
            {
                lock (_handlerLock)
                {
                    return ReadBinaryFromStream(selectedHandler.Stream, Name);
                }
            }
            finally
            {
                lock (_handlerLock)
                {
                    selectedHandler.Stream.Position = 0;
                    selectedHandler.Locked = false;
                }
            }
        }

        public string ReadShader(string Name)
        {
            var builder = new StringBuilder();
            var save = false;
            var regex = $"^<.*{BuildNameRegex(Name)}>$";
            foreach (var line in ShaderCode.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries))
            {
                var next = false;
                if (Regex.IsMatch(line, regex))
                {
                    save = true;
                    next = true;
                }

                if (line.Contains("<end>")) save = false;

                if (save && !next) builder.Append(line + Environment.NewLine);
            }

            if (builder.Length == 0) throw new ArgumentNullException($"Failed to find shader '{Name}'");
            return builder.ToString();
        }

        public byte[] LoadIcon(string Path, out int Width, out int Height)
        {
            using var ms = new MemoryStream(AssetManager.ReadBinary(Path, AssetsResource));
            using var original = Image.Load<Rgba32>(ms);
            
            Width = original.Width;
            Height = original.Height;
            var pixels = new byte[Width * Height * 4];
            var k = 0;
            for (var x = 0; x < Width; ++x)
            {
                for (var y = 0; y < Height; ++y)
                {
                    pixels[x * 4 + y * Width * 4 + 0] = original[x, y].R;
                    pixels[x * 4 + y * Width * 4 + 1] = original[x, y].G;
                    pixels[x * 4 + y * Width * 4 + 2] = original[x, y].B;
                    pixels[x * 4 + y * Width * 4 + 3] = original[x, y].A;
                }
            }
            return pixels;
        }

        public List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale)
        {
            var shapes = new List<CollisionShape>();
            var name = Path.GetFileNameWithoutExtension(Filename);
            for (var i = 0; i < Count; i++)
            {
                var data = PLYLoader($"Assets/Env/Colliders/{name}_Collider{i}.ply", Scale, Vector3.Zero, Vector3.Zero,
                    false);
                AssertCorrectShapeFormat(data);
                var newShape = new CollisionShape(data.Vertices, data.Indices);
                shapes.Add(newShape);
                data.Dispose();
            }

            return shapes;
        }

        public List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale)
        {
            var shapes = new List<CollisionShape>();
            if (!LoadCollisionShapesNew(Filename, Scale, shapes)) LoadCollisionsShapesLegacy(Filename, Scale, shapes);
            return shapes;
        }

        public Box LoadHitbox(string ModelFile)
        {
            return Physics.BuildBroadphaseBox(LoadModelVertexData(ModelFile));
        }

        public Box LoadDimensions(string ModelFile)
        {
            return Physics.BuildDimensionsBox(LoadModelVertexData(ModelFile));
        }

        public VertexData LoadPLYWithLODs(string Filename, Vector3 Scale)
        {
            var model = PLYLoader(Filename, Scale);
            var name = Path.GetFileNameWithoutExtension(Filename);
            var dir = Path.GetDirectoryName(Filename);
            for (var i = 1; i < 4; ++i)
            {
                var iterator = (int)Math.Pow(2, i);
                var path = $"{dir}/{name}_Lod{iterator}.ply";
                var data = ReadBinary(path, AssetsResource);
                if (data == null) return model;
                var vertexInformation = PLYLoader(path, data, Scale, Vector3.Zero, Vector3.Zero);
                model.AddLOD(vertexInformation, iterator);
            }

            return model;
        }

        public VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation,
            bool HasColors = true)
        {
            var data = ReadPath(File);
            if (data == null) throw new ArgumentException($"Failed to find file '{File}' in the Assets folder.");
            return PLYLoader(File, data, Scale, Position, Rotation, HasColors);
        }

        public ModelData DAELoader(string File)
        {
            var data = ReadPath(File);
            if (data == null) throw new ArgumentException($"Failed to find file '{File}' in the Assets folder.");
            var fileContents = Encoding.ASCII.GetString(data);
            var model = ColladaLoader.LoadModel(fileContents);
            model.Name = File;
            return model;
        }

        public void Dispose()
        {
            foreach (var handler in _registeredHandlers) handler.Stream.Dispose();
        }

        public string AppData
        {
            get
            {
                if (_appData != null) return _appData;
                return _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" +
                                  "Project Hedra/";
            }
        }

        public string AppPath
        {
            get
            {
                if (_appPath != null) return _appPath;
                return _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            }
        }

        private void CopyShaders()
        {
            var compatibleAppPath = AppPath.Replace("/", @"\");

            Log.Write($"[DEBUG] Copying shader files to executable...{Environment.NewLine}", ConsoleColor.Magenta);
            Log.WriteLine($"[DEBUG] /C xcopy \"{compatibleAppPath}\\..\\..\\..\\Shaders\" \"{compatibleAppPath}\\Shaders\\\"  /s /e /y");
            var proc = Process.Start("cmd.exe",
                $"/C xcopy \"{compatibleAppPath}\\..\\..\\..\\Shaders\" \"{compatibleAppPath}\\Shaders\\\"  /s /e /y");
            proc?.WaitForExit();
        }

        private void DecompileAssets()
        {
            if (_filesDecompressed) return;

            _uniqueId = Guid.NewGuid().ToString();
            TryCleanupTemp(TemporalFolder = $"{AppData}/Temp/");

            var soundBytes = ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + SoundResource));
            var zipBytes = ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + AssetsResource));

            File.WriteAllBytes(GetResourceName(AssetsResource), zipBytes);
            File.WriteAllBytes(GetResourceName(SoundResource), soundBytes);

            _registeredHandlers = new List<ResourceHandler>();
            _filesDecompressed = true;
        }

        private string GetResourceName(string Resource)
        {
            return $"{TemporalFolder}{Path.GetFileNameWithoutExtension(Resource)}-{_uniqueId}";
        }

        private void TryCleanupTemp(string Temp)
        {
            try
            {
                if (Directory.Exists(TemporalFolder)) Directory.Delete(TemporalFolder, true);
                Directory.CreateDirectory(TemporalFolder);
            }
            catch (IOException e)
            {
                Log.WriteLine("Failed to clean temp directory.");
            }
        }

        private static byte[] ReadBinaryFromStream(Stream Stream, string Name)
        {
            if (!Stream.CanRead) return null;
            var reader = new BinaryReader(Stream);
            var similarPath = default(string);
            var sanitizedName = Name.Replace(@"\", "/").Replace("$DataFile$", string.Empty).Trim();
            var length = reader.BaseStream.Length;
            var list = new List<string>();
            while (reader.BaseStream.Position < length)
            {
                var header = reader.ReadString();
                if (header.Equals("<end_header>"))
                    break;

                var dataPosition = reader.ReadInt64();
                if (Path.GetFileName(header).Equals(Path.GetFileName(Name))) similarPath = header;

                var sanitizedHeader = header.Replace(@"\", "/");
                if (Path.GetFileName(sanitizedHeader).Equals(Path.GetFileName(sanitizedName)) &&
                    sanitizedHeader.Contains(sanitizedName))
                {
                    reader.BaseStream.Seek(dataPosition, SeekOrigin.Begin);
                    return reader.ReadBytes(reader.ReadInt32());
                }

                list.Add(header);
            }

            if (similarPath != null)
                Log.WriteLine(
                    $"Failed to find path '{sanitizedName}' but found similar path '{similarPath}'. Was it a typo?");
            return null;
        }

        private string BuildNameRegex(string Name)
        {
            const string slashRegex = "\\\\*\\/*";
            return Name.Replace("/", slashRegex).Replace(".", "\\.");
        }

        private bool LoadCollisionShapesNew(string Filename, Vector3 Scale, List<CollisionShape> Shapes)
        {
            var name = Path.GetFileNameWithoutExtension(Filename);
            var dir = Path.GetDirectoryName(Filename);
            var path = $"{dir}/{name}-Colliders.ply";
            var bin = ReadBinary(path, AssetsResource);
            if (bin == null) return false;
            AddShape(path, bin, Scale, Shapes);
            return true;
        }

        private void LoadCollisionsShapesLegacy(string Filename, Vector3 Scale, List<CollisionShape> Shapes)
        {
            var name = Path.GetFileNameWithoutExtension(Filename);
            var iterator = 0;
            while (true)
            {
                var path = $"Assets/Env/Colliders/{name}_Collider{iterator}.ply";
                var data = ReadBinary(path, AssetsResource);
                if (data == null) return;
                AddShape(path, data, Scale, Shapes);
                iterator++;
            }
        }

        private void AddShape(string Name, byte[] Binary, Vector3 Scale, List<CollisionShape> Shapes)
        {
            var vertexInformation = PLYLoader(Name, Binary, Scale, Vector3.Zero, Vector3.Zero, false);
            AssertCorrectShapeFormat(vertexInformation);
            var newShape = new CollisionShape(vertexInformation.Vertices, vertexInformation.Indices);
            Shapes.Add(newShape);
            vertexInformation.Dispose();
        }

        private static void AssertCorrectShapeFormat(VertexData VertexInformation)
        {
            /*const int limit = 256;
            if(VertexInformation.Vertices.Count > limit)
                throw new ArgumentOutOfRangeException($"CollisionShape has {VertexInformation.Vertices.Count} vertices but limit is '{limit}'");*/
        }

        private VertexData LoadModelVertexData(string ModelFile)
        {
            lock (_hitboxCacheLock)
            {
                if (!_hitboxCache.ContainsKey(ModelFile))
                {
                    var fileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ModelFile));
                    var entityData = ColladaLoader.LoadColladaModel(fileContents, LoadOptions.Default);
                    var vertexData = new VertexData
                    {
                        Vertices = entityData.Mesh.Vertices.ToList(),
                        Colors = new List<Vector4>()
                    };
                    entityData.Mesh.Colors.ToList().ForEach(Vector =>
                        vertexData.Colors.Add(new Vector4(Vector.X, Vector.Y, Vector.Z, 1)));
                    /* Scale needs to be applied because its applied in the shader for the models and not when loading */
                    vertexData.Transform(Matrix4x4.CreateScale(entityData.Scale));
                    _hitboxCache.Add(ModelFile, vertexData);
                }

                return _hitboxCache[ModelFile];
            }
        }

        private VertexData PLYLoader(string File, Vector3 Scale)
        {
            return PLYLoader(File, Scale, Vector3.Zero, Vector3.Zero);
        }

        private VertexData PLYLoader(string Name, byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation,
            bool HasColors = true)
        {
            const string header = "PROCESSEDPLY";
            var size = Encoding.ASCII.GetByteCount(header);
            if (HasHeader(Data, header, size))
                return PLYUnserialize(Name, Data, size, Scale, Position, Rotation, HasColors);
            return PLYParser(Name, Data, Scale, Position, Rotation, HasColors);
        }

        private VertexData PLYUnserialize(string Name, byte[] Data, int HeaderSize, Vector3 Scale, Vector3 Position,
            Vector3 Rotation, bool HasColors)
        {
            using (var ms = new MemoryStream(Data))
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var colors = new List<Vector4>();
                var indices = new List<uint>();
                ms.Seek(HeaderSize + 1, SeekOrigin.Begin);
                using (var reader = new BinaryReader(ms))
                {
                    var indicesLength = reader.ReadInt32();
                    for (var i = 0; i < indicesLength; i++) indices.Add(reader.ReadUInt32());
                    var vertexLength = reader.ReadInt32();
                    for (var i = 0; i < vertexLength; i++)
                        vertices.Add(
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                        );
                    var normalLength = reader.ReadInt32();
                    for (var i = 0; i < normalLength; i++)
                        normals.Add(
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                        );
                    var colorLength = reader.ReadInt32();
                    for (var i = 0; i < colorLength; i++)
                        colors.Add(
                            new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                                reader.ReadSingle())
                        );
                }

                return HandlePLYTransforms(vertices, normals, colors, indices, Scale, Position, Rotation, Name);
            }
        }

        private VertexData PLYParser(string Name, byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation,
            bool HasColors)
        {
            var fileContents = Encoding.ASCII.GetString(Data);

            var endHeader = fileContents.IndexOf("element vertex", StringComparison.Ordinal);
            fileContents = fileContents.Substring(endHeader, fileContents.Length - endHeader);
            var numbers = Regex.Matches(fileContents, @"-?[\d]+\.[\d]+|[\d]+\.[\d]+|[\d]+")
                .Select(M => M.Value).ToArray();

            const int vertexCountIndex = 0;
            const int faceCountIndex = 1;
            const int startDataIndex = 2;
            var hasAlpha = fileContents.IndexOf("property uchar alpha", StringComparison.Ordinal) != -1;
            var vertexCount = int.Parse(numbers[vertexCountIndex]);
            var faceCount = int.Parse(numbers[faceCountIndex]);

            var vertices = new List<Vector3>(vertexCount);
            var colors = new List<Vector4>();
            var normals = new List<Vector3>();
            var indices = new List<uint>(faceCount * 3);
            var offset = 0;

            var numberOffset = HasColors ? hasAlpha ? 10 : 9 : 6;
            var accumulatedOffset = startDataIndex;
            for (; vertices.Count < vertexCount; accumulatedOffset += numberOffset)
            {
                vertices.Add(
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 0], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 1], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 2], CultureInfo.InvariantCulture)
                    )
                );
                normals.Add(
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 3], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 4], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 5], CultureInfo.InvariantCulture)
                    )
                );
                if (HasColors)
                    colors.Add(
                        new Vector4(
                            float.Parse(numbers[accumulatedOffset + 6]) / 255f,
                            float.Parse(numbers[accumulatedOffset + 7]) / 255f,
                            float.Parse(numbers[accumulatedOffset + 8]) / 255f,
                            hasAlpha ? float.Parse(numbers[accumulatedOffset + 9]) / 255f : 1.0f
                        )
                    );
            }

            for (; indices.Count / 3 < faceCount; accumulatedOffset += 4)
            {
                indices.Add(uint.Parse(numbers[accumulatedOffset + 1]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 2]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 3]));
            }

            return HandlePLYTransforms(vertices, normals, colors, indices, Scale, Position, Rotation, Name);
        }

        private VertexData HandlePLYTransforms(List<Vector3> Vertices, List<Vector3> Normals,
            List<Vector4> Colors, List<uint> Indices, Vector3 Scale, Vector3 Position, Vector3 Rotation, string Name)
        {
            var scaleMat = Matrix4x4.CreateScale(Scale);
            var positionMat = Matrix4x4.CreateTranslation(Position);
            var rotationMat = Matrix4x4.CreateRotationY(Rotation.Y);
            rotationMat *= Matrix4x4.CreateRotationX(Rotation.X);
            rotationMat *= Matrix4x4.CreateRotationZ(Rotation.Z);
            for (var j = 0; j < Vertices.Count; j++)
            {
                Vertices[j] = Vector3.Transform(Vertices[j], scaleMat);
                Vertices[j] = Vector3.Transform(Vertices[j], rotationMat);
                Vertices[j] = Vector3.Transform(Vertices[j], positionMat);
            }

            var invertedMat = rotationMat.Inverted().Transposed();
            for (var j = 0; j < Normals.Count; j++) Normals[j] = Vector3.TransformNormal(Normals[j], invertedMat);

            var data = new VertexData
            {
                Vertices = Vertices,
                Indices = Indices,
                Normals = Normals,
                Colors = Colors,
                UseCache = true,
                Name = Name
            };
            data.Trim();
            using (var allocator = new HeapAllocator(data.SizeInBytes * 4))
            {
                data.Optimize(allocator);
            }

            return data;
        }

        private bool HasHeader(byte[] Data, string Header, int HeaderSize)
        {
            var fileHeader = Encoding.ASCII.GetString(Data, 1, HeaderSize);
            return fileHeader == Header;
        }
    }
}