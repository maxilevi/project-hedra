using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;

namespace Hedra.Engine.Management
{
    public class CompressedAssetProvider : IAssetProvider
    {
	    private readonly PrivateFontCollection _boldFonts;
        private readonly PrivateFontCollection _normalFonts;
        private List<ResourceHandler> _registeredHandlers;
        private bool _filesDecompressed;
	    private Dictionary<string, VertexData> _hitboxCache;
	    private readonly object _hitboxCacheLock = new object();
        private readonly object _handlerLock = new object();

	    public string ShaderCode { get; private set; }
	    public string AppPath { get; private set; }
	    public string AppData { get; private set; }
	    public string TemporalFolder { get; private set; }
	    public FontFamily BoldFamily => _boldFonts.Families[0];
	    public FontFamily NormalFamily => _normalFonts.Families[0];
	    public string ShaderResource => "data1.db";
	    public string SoundResource => "data2.db";
	    public string AssetsResource => "data3.db";
	    
	    public CompressedAssetProvider()
	    {
	        _boldFonts = new PrivateFontCollection();
	        _normalFonts = new PrivateFontCollection();
        }

        public void Load()
        {
			var sansBold = AssetManager.ReadBinary("Assets/ClearSans-Bold.ttf", AssetManager.DataFile3);
			_boldFonts.AddMemoryFont(Utils.IntPtrFromByteArray(sansBold), sansBold.Length);

			var sansRegular = AssetManager.ReadBinary("Assets/ClearSans-Regular.ttf", AssetManager.DataFile3);          	
			_normalFonts.AddMemoryFont(Utils.IntPtrFromByteArray(sansRegular), sansRegular.Length);
	        
            AssetManager.ReloadShaderSources();
	        lock (_hitboxCacheLock)
	        {
		        _hitboxCache = new Dictionary<string, VertexData>();
	        }
        }

	    public void ReloadShaderSources()
	    {
	        ShaderCode = ZipManager.UnZip(File.ReadAllBytes(AppPath + ShaderResource));
        }

	    private void CopyShaders()
	    {
	        var compatibleAppPath = AppPath.Replace("/", @"\");

	        Log.Write($"[DEBUG] Copying shader files to executable...{Environment.NewLine}", ConsoleColor.Magenta);
            var proc = System.Diagnostics.Process.Start("cmd.exe",
                $"/C xcopy \"{compatibleAppPath}\\..\\..\\Shaders\" \"{compatibleAppPath}\\Shaders\\\"  /s /e /y");
	        proc?.WaitForExit();
	    }
	    
	    public void GrabShaders()
	    {
            this.CopyShaders();
	        Log.Write($"[DEBUG] Rebuilding shader bundles...{Environment.NewLine}", ConsoleColor.Magenta);
            var pProcess = new System.Diagnostics.Process
	        {
	            StartInfo =
	            {
	                FileName = $@"{AppPath}/../../../utilities/AssetBuilder.exe",
	                Arguments = $"\"{AppPath}/Shaders/\" \"{AppPath}/data1.db\" normal text",
	                UseShellExecute = false,
	                RedirectStandardOutput = true,
	                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
	                CreateNoWindow = true
	            }
	        };
	        pProcess.Start();
	        Log.Write($"{Environment.NewLine}[DEBUG]{pProcess.StandardOutput.ReadToEnd()}{Environment.NewLine}", ConsoleColor.Magenta);
	        pProcess.WaitForExit();
	        pProcess.Dispose();
	    }

        private void DecompileAssets()
        {
            if(_filesDecompressed) return;		    

			AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + "Project Hedra/";
			AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ "/";

		    TemporalFolder = AppData + "/Temp/";

            if ( Directory.Exists(TemporalFolder) ) Directory.Delete(TemporalFolder, true);

		    DirectoryInfo info = Directory.CreateDirectory(TemporalFolder);
		    info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

		    //Decompress binary contents in there so there is not much ram usage
		    File.WriteAllBytes(TemporalFolder + Path.GetFileNameWithoutExtension(AssetsResource),
                ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + AssetsResource))  );

		    File.WriteAllBytes(TemporalFolder + Path.GetFileNameWithoutExtension(SoundResource),
		        ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + SoundResource))  );

            _registeredHandlers = new List<ResourceHandler>();
            _filesDecompressed = true;
		}

	    public byte[] ReadPath(string Path)
	    {
	        bool external = !Path.Contains("$DataFile$");
	        if (!external || Path.StartsWith("Assets/"))
	        {
	            Path = Path.Replace("$DataFile$", string.Empty);
	            if (Path.StartsWith("/"))
                    Path = Path.Substring(1, Path.Length-1);

                return ReadBinary(Path, AssetsResource);
	        }
	        return Encoding.ASCII.GetBytes( File.ReadAllText(Path.Replace("$GameFolder$", AppPath)) );
	    }

		public byte[] ReadBinary(string Name, string DataFile)
		{
		    this.DecompileAssets();
            ResourceHandler selectedHandler = null;
		    lock (_handlerLock)
		    {
		        for (var i = 0; i < _registeredHandlers.Count; i++)
		        {
		            if (!_registeredHandlers[i].Locked && _registeredHandlers[i].Id == DataFile)
		            {
		                selectedHandler = _registeredHandlers[i];
		                selectedHandler.Locked = true;
		                break;
		            }
		        }
		    }
		    if (selectedHandler == null)
		    {
		        lock (_handlerLock)
		        {
		            selectedHandler =new ResourceHandler(File.OpenRead(TemporalFolder + Path.GetFileNameWithoutExtension(DataFile)), DataFile);
		            _registeredHandlers.Add(selectedHandler);
		        }
		        Log.WriteLine($"Registered resource handler... (Total = {_registeredHandlers.Count})", LogType.IO);
		    }
		    try
		    {
		        lock (_handlerLock)
		        {
		            return this.ReadBinaryFromStream(selectedHandler.Stream, Name);
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

	    private byte[] ReadBinaryFromStream(Stream Stream, string Name)
	    {
	        if (!Stream.CanRead) return null;
	        var reader = new BinaryReader(Stream); // .Dispose closes the stream, something we dont want.
	        while (reader.BaseStream.Position < reader.BaseStream.Length)
	        {
	            string header = reader.ReadString();
	            int chunkSize = reader.ReadInt32();

	            if (Path.GetFileName(header).Equals(Path.GetFileName(Name)))
	            {
	                return reader.ReadBytes(chunkSize);
	            }
	            reader.BaseStream.Seek(reader.BaseStream.Position + chunkSize, SeekOrigin.Begin);
	        }
	        return null;
        }
		
		public string ReadShader(string Name)
        {
			var builder  = new StringBuilder();
			var save = false;
		    var regex = $"^<.*{this.BuildNameRegex(Name)}>$";
            foreach (var line in ShaderCode.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)){
				var next = false;
				if(Regex.IsMatch(line, regex))
                {
					save = true;
					next = true;
				}
		        if (line.Contains("<end>"))
		        {
		            save = false;
		        }

		        if (save && !next)
		        {
		            builder.Append(line + Environment.NewLine);
		        }
		    }

            return builder.ToString();
		}

	    private string BuildNameRegex(string Name)
	    {
	        const string slashRegex = "\\\\*\\/*";
	        return Name.Replace("/", slashRegex).Replace(".", "\\.");
	    }
		
		public Icon LoadIcon(string path){
			using(var ms = new MemoryStream(AssetManager.ReadBinary(path, AssetsResource))){
				return new Icon(ms);
			}
		}
		
		public List<CollisionShape> LoadCollisionShapes(string Filename, int Count, Vector3 Scale)
		{
			var shapes = new List<CollisionShape>();
			var name = Path.GetFileNameWithoutExtension(Filename);
			for(var i = 0; i < Count; i++){
				var data = AssetManager.PLYLoader("Assets/Env/Colliders/"+name+"_Collider"+i+".ply", Scale, Vector3.Zero, Vector3.Zero, false);
			    var newShape = new CollisionShape(data.Vertices, data.Indices);
			    shapes.Add(newShape);
                data.Dispose();
			}
			
			return shapes;
		}
		
		public List<CollisionShape> LoadCollisionShapes(string Filename, Vector3 Scale)
		{
			var shapes = new List<CollisionShape>();
			string name = Path.GetFileNameWithoutExtension(Filename);
			var iterator = 0;
			while(true)
			{
				var path = $"Assets/Env/Colliders/{name}_Collider{iterator}.ply";
				var data = ReadBinary(path, AssetsResource);
				if(data == null) return shapes;
				var vertexInformation = AssetManager.PLYLoader(path, Scale, Vector3.Zero, Vector3.Zero, false);
				var newShape = new CollisionShape(vertexInformation.Vertices, vertexInformation.Indices);
				shapes.Add(newShape);
				vertexInformation.Dispose();
				iterator++;
			}
		}

	    private VertexData LoadModelVertexData(string ModelFile)
	    {
	        lock (_hitboxCacheLock)
	        {
	            if (!_hitboxCache.ContainsKey(ModelFile))
	            {
	                string fileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ModelFile));
	                var entityData = ColladaLoader.LoadColladaModel(fileContents, GeneralSettings.MaxWeights);
	                var vertexData = new VertexData
	                {
	                    Vertices = entityData.Mesh.Vertices.ToList(),
	                    Colors = new List<Vector4>()
	                };
	                entityData.Mesh.Colors.ToList().ForEach(Vector =>
	                    vertexData.Colors.Add(new Vector4(Vector.X, Vector.Y, Vector.Z, 1)));
	                _hitboxCache.Add(ModelFile, vertexData);
	            }
	            return _hitboxCache[ModelFile];
	        }
	    }

	    public Box LoadHitbox(string ModelFile)
	    {        
	        return Physics.BuildBroadphaseBox(this.LoadModelVertexData(ModelFile));
	    }

	    public Box LoadDimensions(string ModelFile)
	    {
	        return Physics.BuildDimensionsBox(this.LoadModelVertexData(ModelFile));
        }

        public VertexData PLYLoader(string file, Vector3 Scale){
			return AssetManager.PLYLoader(file, Scale, Vector3.Zero, Vector3.Zero);
		}

		public VertexData PLYLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true)
		{
			var data = AssetManager.ReadBinary(File, AssetsResource);
			if (data == null) throw new ArgumentException($"Failed to found file '{File}' in the Assets folder.");
			return PLYLoader(data, Scale, Position, Rotation, HasColors);
		}

		public VertexData PLYLoader(byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true)
		{
			const string header = "PROCESSEDPLY";
			var size = Encoding.ASCII.GetByteCount(header);
			if (HasHeader(Data, header, size))
			{
				return PLYUnserialize(Data, size, Scale, Position, Rotation, HasColors);
			}
			return PLYParser(Data, Scale, Position, Rotation, HasColors);
		}

		public VertexData PLYUnserialize(byte[] Data, int HeaderSize, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors)
		{
			using (var ms = new MemoryStream(Data))
			{
				var vertices = new List<Vector3>();
				var normals = new List<Vector3>();
				var colors = new List<Vector4>();
				var indices = new List<uint>();
				ms.Seek(HeaderSize+1, SeekOrigin.Begin);
				using (var reader = new BinaryReader(ms))
				{
					var indicesLength = reader.ReadInt32();
					for (var i = 0; i < indicesLength; i++)
					{
						indices.Add(reader.ReadUInt32());
					}
					var vertexLength = reader.ReadInt32();
					for (var i = 0; i < vertexLength; i++)
					{
						vertices.Add(
							new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
						);
					}
					var normalLength = reader.ReadInt32();
					for (var i = 0; i < normalLength; i++)
					{
						normals.Add(
							new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
							);
					}
					var colorLength = reader.ReadInt32();
					for (var i = 0; i < colorLength; i++)
					{
						colors.Add(
							new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
							);
					}
				}
				return HandlePLYTransforms(vertices, normals, colors, indices, Scale, Position, Rotation);
			}
		}
		
		public VertexData PLYParser(byte[] Data, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors)
        {
		    string fileContents = Encoding.ASCII.GetString(Data);

		    int endHeader = fileContents.IndexOf("element vertex", StringComparison.Ordinal);
            fileContents = fileContents.Substring(endHeader, fileContents.Length - endHeader);
            var numbers = Regex.Matches(fileContents, @"-?[\d]+\.[\d]+|[\d]+\.[\d]+|[\d]+").Cast<Match>().Select(M => M.Value).ToArray();

            const int vertexCountIndex = 0;
            const int faceCountIndex = 1;
            const int startDataIndex = 2;
            var vertexCount = int.Parse(numbers[vertexCountIndex]);
            var faceCount = int.Parse(numbers[faceCountIndex]);

            var vertices = new List<Vector3>(vertexCount);
			var colors = new List<Vector4>();
			var normals = new List<Vector3>();
			var indices = new List<uint>(faceCount * 3);
            var offset = 0;

            var numberOffset = HasColors ? 9 : 6;
			int accumulatedOffset = startDataIndex;
            for(; vertices.Count < vertexCount; accumulatedOffset += numberOffset)
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
				{
				    colors.Add(
                        new Vector4(
                            float.Parse(numbers[accumulatedOffset + 6]) / 255f,
                            float.Parse(numbers[accumulatedOffset + 7]) / 255f, 
                            float.Parse(numbers[accumulatedOffset + 8]) / 255f,
                            1.0f
                            )
                    );
				}
			}
            for (; indices.Count / 3 < faceCount; accumulatedOffset += 4)
            {
                indices.Add(uint.Parse(numbers[accumulatedOffset + 1]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 2]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 3]));
            }
	        return HandlePLYTransforms(vertices, normals, colors, indices, Scale, Position, Rotation);
        }

		private VertexData HandlePLYTransforms(List<Vector3> Vertices, List<Vector3> Normals,
			List<Vector4> Colors, List<uint> Indices, Vector3 Scale, Vector3 Position, Vector3 Rotation)
		{
			var scaleMat = Matrix4.CreateScale(Scale);
			var positionMat = Matrix4.CreateTranslation(Position);
			var rotationMat = Matrix4.CreateRotationY(Rotation.Y);
			rotationMat *= Matrix4.CreateRotationX(Rotation.X);
			rotationMat *= Matrix4.CreateRotationZ(Rotation.Z);
			for(var j = 0; j < Vertices.Count; j++)
			{
				Vertices[j] = Vector3.TransformPosition(Vertices[j], scaleMat);
				Vertices[j] = Vector3.TransformPosition(Vertices[j], rotationMat);
				Vertices[j] = Vector3.TransformPosition(Vertices[j], positionMat);
			}
			
			for(var j = 0; j < Normals.Count; j++)
			{
				Normals[j] = Vector3.TransformNormal(Normals[j], rotationMat);
			}
			
			return new VertexData
			{
				Vertices = Vertices,
				Indices = Indices,
				Normals = Normals,
				Colors = Colors,
				UseCache = true
			};
		}

		private bool HasHeader(byte[] Data, string Header, int HeaderSize)
		{
		    var fileHeader = Encoding.ASCII.GetString(Data, 1, HeaderSize);
            return fileHeader == Header;
		}
		
	    public void Dispose()
	    {
	        foreach (var handler in _registeredHandlers)
	        {
	            handler.Stream.Dispose();
	        }
	        if (Directory.Exists(TemporalFolder)) Directory.Delete(TemporalFolder, true);
	    }
	    
    }
}