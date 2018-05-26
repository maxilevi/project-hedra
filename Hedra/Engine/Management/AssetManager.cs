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
	public static class AssetManager
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

        private static bool _filesDecompressed;
	    private static Dictionary<string, VertexData> _hitboxCache;

	    public static string ShaderCode { get; private set; }

        public static void Load(){
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
	                Arguments = $"\"{AppPath}/Shaders/\" \"{AppPath}/data1.db\" text",
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
        private static void SetupFiles(){
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

		    _filesDecompressed = true;
		}

	    public static void Dispose()
	    {
	        if (Directory.Exists(TemporalFolder)) Directory.Delete(TemporalFolder, true);
        }

	    public static byte[] ReadPath(string Path)
	    {
	        bool external = !Path.Contains("$DataFile$");
	        if (!external || Path.StartsWith("Assets/"))
	        {
	            Path = Path.Replace("$DataFile$", "");
	            if (Path.StartsWith("/"))
                    Path = Path.Substring(1, Path.Length-1);

                return ReadBinary(Path, DataFile3);
	        }
	        return Encoding.ASCII.GetBytes( File.ReadAllText(Path.Replace("$GameFolder$", AppPath)) );
	    }
		
		public static byte[] ReadBinary(string Name, string DataFile)
		{
		    AssetManager.SetupFiles();
            using (FileStream Fs = File.OpenRead(TemporalFolder + Path.GetFileNameWithoutExtension(DataFile))){
				using (BinaryReader Reader = new BinaryReader(Fs))
				{
				    while (Reader.BaseStream.Position < Reader.BaseStream.Length)
       				{
				    	string Header = Reader.ReadString();
				    	int ChunkSize = Reader.ReadInt32();

				    	byte[] Data = Reader.ReadBytes(ChunkSize);
				    	if( Path.GetFileName(Header).Equals(Path.GetFileName(Name))) 
				    		return Data;    	
				    }			    
				}
			}
			return null;	
		}
		
		public static string[] GetFileNames(string DataFile){
			List<string> Files = new List<string>();
			using (FileStream Fs = File.OpenRead(AppPath + DataFile)){
				using (BinaryReader Reader = new BinaryReader(Fs))
				{
				    while (Reader.BaseStream.Position < Reader.BaseStream.Length)
       				{
				    	string Header = Reader.ReadString();
				    	int ChunkSize = Reader.ReadInt32();
				    	byte[] Data = Reader.ReadBytes(ChunkSize);
				    	Files.Add(Header);
				    }				    
				}
			}
			return Files.ToArray();
		}
		
		public static string ReadShader(string Name){
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
	        var slashRegex = "\\\\*\\/*";
            return Name.Replace("/", slashRegex).Replace(".", "\\.");
	    }
		
		public static Icon LoadIcon(string path){
			using(var ms = new MemoryStream(AssetManager.ReadBinary(path, DataFile3))){
				return new Icon(ms);
			}
		}
		
		public static List<CollisionShape> LoadCollisionShapes(string filename, int Count, Vector3 Scale){
			List<CollisionShape> Shapes = new List<CollisionShape>();
			string Name = Path.GetFileNameWithoutExtension(filename);
			for(int i = 0; i < Count; i++){
				VertexData Data = AssetManager.PlyLoader("Assets/Env/Colliders/"+Name+"_Collider"+i+".ply", Scale, Vector3.Zero, Vector3.Zero, false);

			    var newShape = new CollisionShape(new List<Vector3>(Data.Vertices))
			    {
			        UseCache = true,
#if DEBUG
			        Indices = new List<uint>(Data.Indices)
#endif
			    };

			    Shapes.Add(newShape);
                Data.Dispose();
			}
			
			return Shapes;
		}
		
		public static VertexData PlyLoader(string file, Vector3 Scale){
			return AssetManager.PlyLoader(file, Scale, Vector3.Zero, Vector3.Zero);
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
		    var offset = new Vector3(
                (data.SupportPoint(Vector3.UnitX).X + data.SupportPoint(-Vector3.UnitX).X) * .5f,
                0,
                (data.SupportPoint(Vector3.UnitZ).Z + data.SupportPoint(-Vector3.UnitZ).Z) * .5f
                );
            var minus = Math.Min(data.SupportPoint(-Vector3.UnitX).X - offset.X, data.SupportPoint(-Vector3.UnitZ).Z - offset.Z);
		    var plus = Math.Max(data.SupportPoint(Vector3.UnitX).X - offset.X, data.SupportPoint(Vector3.UnitZ).Z - offset.Z);
            return new Box(
		        new Vector3(minus, data.SupportPoint(-Vector3.UnitY).Y, minus),
		        new Vector3(plus, data.SupportPoint(Vector3.UnitY).Y, plus)
                );
		}
		
		public static VertexData PlyLoader(string File, Vector3 Scale, Vector3 Position, Vector3 Rotation, bool HasColors = true){
			byte[] dataArray = AssetManager.ReadBinary(File, DataFile3);
		    string fileContents = Encoding.ASCII.GetString(dataArray);

		    int endHeader = fileContents.IndexOf("element vertex", StringComparison.Ordinal);
            fileContents = fileContents.Substring(endHeader, fileContents.Length - endHeader);
            MatchCollection verts = Regex.Matches(fileContents, @"-?[\d]+\.[\d]+|[\d]+\.[\d]+|[\d]+");

            var vertexData = new List<Vector3>();
			var colors = new List<Vector4>();
			var normals = new List<Vector3>();
			var indices = new List<uint>();
			var offset = 0;

		    const int vertexCountIndex = 0;
            const int faceCountIndex = 1;
		    const int startDataIndex = 2;

			if(HasColors){
				int i = startDataIndex;
				while(true){
					vertexData.Add( new Vector3(float.Parse(verts[i].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+1].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+2].Value, CultureInfo.InvariantCulture) ));
					normals.Add( new Vector3(float.Parse(verts[i+3].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+4].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+5].Value, CultureInfo.InvariantCulture) ));
					                            
					colors.Add( new Vector4(float.Parse(verts[i+6].Value) / 255f, float.Parse(verts[i+7].Value) / 255f, float.Parse(verts[i+8].Value) / 255f, 1));
					if(vertexData.Count >= Int32.Parse(verts[vertexCountIndex].Value)){
						i += 9;
						break;
					}
					i += 9;
				}

				while(true){
					indices.Add(uint.Parse(verts[i+1].Value));
					indices.Add(uint.Parse(verts[i+2].Value));
					indices.Add(uint.Parse(verts[i+3].Value));
					
					if(indices.Count / 3 >= uint.Parse(verts[faceCountIndex].Value)){
						break;
					}
					
					i+=4;
				}
			}else{
				var i = startDataIndex;
				while(true){
					vertexData.Add( new Vector3(float.Parse(verts[i].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+1].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+2].Value, CultureInfo.InvariantCulture) ));
					normals.Add( new Vector3(float.Parse(verts[i+3].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+4].Value, CultureInfo.InvariantCulture), float.Parse(verts[i+5].Value, CultureInfo.InvariantCulture) ));
					                            
					if(vertexData.Count >= uint.Parse(verts[vertexCountIndex].Value)){
						i += 6;
						break;
					}
					i += 6;
				}
				while(true){
					indices.Add(uint.Parse(verts[i+1].Value));
					indices.Add(uint.Parse(verts[i+2].Value));
					indices.Add(uint.Parse(verts[i+3].Value));
					
					if(indices.Count / 3 >= uint.Parse(verts[faceCountIndex].Value))
						break;
					
					i+=4;
				}
			}
			
			Matrix4 ScaleMat = Matrix4.CreateScale(Scale);
			Matrix4 PositionMat = Matrix4.CreateTranslation(Position);
			Matrix4 RotationMat = Matrix4.CreateRotationY(Rotation.Y);
			RotationMat *= Matrix4.CreateRotationX(Rotation.X);
			RotationMat *= Matrix4.CreateRotationZ(Rotation.Z);
			for(int j = 0; j < vertexData.Count; j++){
				vertexData[j] = Vector3.TransformPosition(vertexData[j], ScaleMat);
				vertexData[j] = Vector3.TransformPosition(vertexData[j], RotationMat);
				vertexData[j] = Vector3.TransformPosition(vertexData[j], PositionMat);
			}
			
			for(int j = 0; j < normals.Count; j++){
				normals[j] = Vector3.TransformNormal(normals[j], RotationMat);
			}

			VertexData Data = new VertexData();
			Data.Vertices = vertexData;
			Data.Indices = indices;
			Data.Normals = normals;
			Data.Colors = colors;
			Data.UseCache = true;
			return Data;
		}
        
		public static VAO<Vector3, Vector4, Vector3> PlyLoader(string file, out VBO<uint> IndicesVBO, Vector3 Scale, Vector3 Position, Vector3 Rotation){
			VertexData Data = PlyLoader(file, Scale, Position, Rotation);
			
			VBO<Vector3> VBOVerts = new VBO<Vector3>(Data.Vertices.ToArray(), Data.Vertices.Count * Vector3.SizeInBytes, VertexAttribPointerType.Float);
			VBO<Vector4> VBOColors = new VBO<Vector4>(Data.Colors.ToArray(), Data.Colors.Count * Vector4.SizeInBytes, VertexAttribPointerType.Float);
			VBO<Vector3> VBONorms = new VBO<Vector3>(Data.Normals.ToArray(), Data.Normals.Count * Vector3.SizeInBytes, VertexAttribPointerType.Float);
			IndicesVBO = new VBO<uint>(Data.Indices.ToArray(), Data.Indices.Count * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
			return new VAO<Vector3, Vector4, Vector3>(VBOVerts, VBOColors, VBONorms);
		}

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
