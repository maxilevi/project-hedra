/*
 * Author: Zaphyk
 * Date: 09/02/2016
 * Time: 03:24 a.m.
 *
 */
using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Text;
using System.Reflection;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of AssetManager.
	/// </summary>
	internal static class AssetManager
	{
		public static Color[] Palette = new Color[256];
		public const string DataFile1 = "data1.db";
		public const string DataFile2 = "data2.db";
		public const string DataFile3 = "data3.db";
		public static Vector4 ColorCode0 { get; } = new Vector4(.0f,.0f,.0f,1f);
		public static Vector4 ColorCode1 { get; } = new Vector4(.2f,.2f,.2f,1f);
		public static Vector4 ColorCode2 { get; } = new Vector4(.4f,.4f,.4f,1f);
		public static Vector4 ColorCode3 { get; } = new Vector4(.6f,.6f,.6f,1f);
        public static Vector4[] ColorCodes { get; } = { ColorCode0, ColorCode1, ColorCode2, ColorCode3};
		public static PrivateFontCollection Fonts = new PrivateFontCollection();
		public static string AppPath { get; private set; }
	    public static string AppData { get; private set; }
        public static string TemporalFolder { get; private set; }
	    public static FontFamily BoldFamily => Fonts.Families[0];
	    private static List<ResourceHandler> _registeredHandlers;
        private static bool _filesDecompressed;
	    private static Dictionary<string, VertexData> _hitboxCache;

	    public static string ShaderCode { get; private set; }

        public static void Load()
        {
			byte[] sansBold = AssetManager.ReadBinary("Assets/ClearSans-Bold.ttf", AssetManager.DataFile3);
			Fonts.AddMemoryFont(Utils.IntPtrFromByteArray(sansBold), sansBold.Length);

            AssetManager.ReloadShaderSources();
            _hitboxCache = new Dictionary<string, VertexData>();
        }

	    public static void ReloadShaderSources()
	    {
	        ShaderCode = ZipManager.UnZip(File.ReadAllBytes(AppPath + DataFile1));
        }

#if DEBUG
	    private static void CopyShaders()
	    {
	        var compatibleAppPath = AppPath.Replace("/", @"\");

	        Log.Write($"[DEBUG] Copying shader files to executable...{Environment.NewLine}", ConsoleColor.Magenta);
            var proc = System.Diagnostics.Process.Start("cmd.exe",
                $"/C xcopy \"{compatibleAppPath}\\..\\..\\Shaders\" \"{compatibleAppPath}\\Shaders\\\"  /s /e /y");
	        proc?.WaitForExit();
	    }
	    public static void GrabShaders()
	    {
            AssetManager.CopyShaders();
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
#endif
        private static void DecompileAssets(){
            if(_filesDecompressed) return;		    

			AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + "Project Hedra/";
			AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ "/";

		    TemporalFolder = AppData + "/Temp/";

            if ( Directory.Exists(TemporalFolder) ) Directory.Delete(TemporalFolder, true);

		    DirectoryInfo info = Directory.CreateDirectory(TemporalFolder);
		    info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

		    //Decompress binary contents in there so there is not much ram usage
		    File.WriteAllBytes(TemporalFolder + Path.GetFileNameWithoutExtension(DataFile3),
                ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + DataFile3))  );

		    File.WriteAllBytes(TemporalFolder + Path.GetFileNameWithoutExtension(DataFile2),
		        ZipManager.UnZipBytes(File.ReadAllBytes(AppPath + DataFile2))  );

            _registeredHandlers = new List<ResourceHandler>();
            _filesDecompressed = true;
		}

	    public static byte[] ReadPath(string Path)
	    {
	        bool external = !Path.Contains("$DataFile$");
	        if (!external || Path.StartsWith("Assets/"))
	        {
	            Path = Path.Replace("$DataFile$", string.Empty);
	            if (Path.StartsWith("/"))
                    Path = Path.Substring(1, Path.Length-1);

                return ReadBinary(Path, DataFile3);
	        }
	        return Encoding.ASCII.GetBytes( File.ReadAllText(Path.Replace("$GameFolder$", AppPath)) );
	    }

		public static byte[] ReadBinary(string Name, string DataFile)
		{
		    AssetManager.DecompileAssets();
            ResourceHandler selectedHandler = null;
		    for (var i = 0; i < _registeredHandlers.Count; i++)
		    {
		        if (!_registeredHandlers[i].Locked && _registeredHandlers[i].Id == DataFile)
		        {
		            selectedHandler = _registeredHandlers[i];
		            selectedHandler.Locked = true;
                    break;
		        }
		    }
		    if (selectedHandler == null)
		    {
                selectedHandler = new ResourceHandler(File.OpenRead(TemporalFolder + Path.GetFileNameWithoutExtension(DataFile)), DataFile);
		        _registeredHandlers.Add(selectedHandler);
                Log.WriteLine($"[IO] Registered resource handler... (Total = {_registeredHandlers.Count})");
		    }
		    try
		    {
		        return AssetManager.ReadBinaryFromStream(selectedHandler.Stream, Name);
		    }
		    finally
		    {
		        selectedHandler.Stream.Position = 0;
		        selectedHandler.Locked = false;
            }
		}

	    private static byte[] ReadBinaryFromStream(Stream Stream, string Name)
	    {
	        if (!Stream.CanRead) return null;
	        var reader = new BinaryReader(Stream); // .Dispose closes the stream, something we dont want.
	        while (reader.BaseStream.Position < reader.BaseStream.Length)
	        {
	            string header = reader.ReadString();
	            int chunkSize = reader.ReadInt32();

	            byte[] data = reader.ReadBytes(chunkSize);
	            if (Path.GetFileName(header).Equals(Path.GetFileName(Name)))
                    return data;
	            
	        }
	        return null;
        }
		
		public static string ReadShader(string Name)
        {
			var builder  = new StringBuilder();
			var save = false;
		    var regex = $"^<.*{AssetManager.BuildNameRegex(Name)}>$";
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

	    private static string BuildNameRegex(string Name)
	    {
	        const string slashRegex = "\\\\*\\/*";
	        return Name.Replace("/", slashRegex).Replace(".", "\\.");
	    }
		
		public static Icon LoadIcon(string path){
			using(var ms = new MemoryStream(AssetManager.ReadBinary(path, DataFile3))){
				return new Icon(ms);
			}
		}
		
		public static List<CollisionShape> LoadCollisionShapes(string filename, int Count, Vector3 Scale){
			var shapes = new List<CollisionShape>();
			string name = Path.GetFileNameWithoutExtension(filename);
			for(var i = 0; i < Count; i++){
				var data = AssetManager.PlyLoader("Assets/Env/Colliders/"+name+"_Collider"+i+".ply", Scale, Vector3.Zero, Vector3.Zero, false);

			    var newShape = new CollisionShape(data.Vertices, data.Indices);
			    shapes.Add(newShape);
                data.Dispose();
			}
			
			return shapes;
		}

	    public static Box LoadHitbox(string ModelFile)
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
	            entityData.Mesh.Colors.ToList().ForEach(Vector => vertexData.Colors.Add(new Vector4(Vector.X, Vector.Y, Vector.Z, 1)));
	            _hitboxCache.Add(ModelFile, vertexData);
	        }
	        var data = _hitboxCache[ModelFile];
	        return Physics.BuildBroadphaseBox(data);
	    }

        public static VertexData PlyLoader(string file, Vector3 Scale){
			return AssetManager.PlyLoader(file, Scale, Vector3.Zero, Vector3.Zero);
		}
		
		public static VertexData PlyLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true)
        {
			byte[] dataArray = AssetManager.ReadBinary(File, DataFile3);
            if (dataArray == null) throw new ArgumentException($"Failed to found file '{File}' in the Assets folder.");
		    string fileContents = Encoding.ASCII.GetString(dataArray);

		    int endHeader = fileContents.IndexOf("element vertex", StringComparison.Ordinal);
            fileContents = fileContents.Substring(endHeader, fileContents.Length - endHeader);
            var numbers = Regex.Matches(fileContents, @"-?[\d]+\.[\d]+|[\d]+\.[\d]+|[\d]+");

            const int vertexCountIndex = 0;
            const int faceCountIndex = 1;
            const int startDataIndex = 2;
            var vertexCount = int.Parse(numbers[vertexCountIndex].Value);
            var faceCount = int.Parse(numbers[faceCountIndex].Value);

            var vertexData = new List<Vector3>(vertexCount);
			var colors = new List<Vector4>();
			var normals = new List<Vector3>();
			var indices = new List<uint>(faceCount * 3);
            var offset = 0;

            var numberOffset = HasColors ? 9 : 6;
			int accumulatedOffset = startDataIndex;
            for(; vertexData.Count < vertexCount; accumulatedOffset += numberOffset)
            {
				vertexData.Add( 
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 0].Value, CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 1].Value, CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 2].Value, CultureInfo.InvariantCulture) 
                        )
                );
				normals.Add( 
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 3].Value, CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 4].Value, CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 5].Value, CultureInfo.InvariantCulture)
                        )
                );
				if (HasColors)
				{
				    colors.Add(
                        new Vector4(
                            float.Parse(numbers[accumulatedOffset + 6].Value) / 255f,
                            float.Parse(numbers[accumulatedOffset + 7].Value) / 255f, 
                            float.Parse(numbers[accumulatedOffset + 8].Value) / 255f,
                            1.0f
                            )
                    );
				}
			}
            for (; indices.Count / 3 < faceCount; accumulatedOffset += 4)
            {
                indices.Add(uint.Parse(numbers[accumulatedOffset + 1].Value));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 2].Value));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 3].Value));
            }

			var scaleMat = Matrix4.CreateScale(Scale);
			var positionMat = Matrix4.CreateTranslation(Position);
			var rotationMat = Matrix4.CreateRotationY(Rotation.Y);
			rotationMat *= Matrix4.CreateRotationX(Rotation.X);
			rotationMat *= Matrix4.CreateRotationZ(Rotation.Z);
			for(var j = 0; j < vertexData.Count; j++)
            {
				vertexData[j] = Vector3.TransformPosition(vertexData[j], scaleMat);
				vertexData[j] = Vector3.TransformPosition(vertexData[j], rotationMat);
				vertexData[j] = Vector3.TransformPosition(vertexData[j], positionMat);
			}
			
			for(var j = 0; j < normals.Count; j++)
            {
				normals[j] = Vector3.TransformNormal(normals[j], rotationMat);
			}

            return new VertexData
            {
                Vertices = vertexData,
                Indices = indices,
                Normals = normals,
                Colors = colors,
                UseCache = true
            };
		}

	    public static void Dispose()
	    {
	        foreach (var handler in _registeredHandlers)
	        {
	            handler.Stream.Dispose();
	        }
	        if (Directory.Exists(TemporalFolder)) Directory.Delete(TemporalFolder, true);
	    }
    }

    internal class ResourceHandler
    {
        public FileStream Stream { get; }
        public string Id { get; }
        public bool Locked { get; set; }

        public ResourceHandler(FileStream Stream, string Id)
        {
            this.Stream = Stream;
            this.Id = Id;
        }
    }
}
