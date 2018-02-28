/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/06/2016
 * Time: 01:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using GLSLOptimizerSharp;

namespace ShaderCompiler
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	static class Compiler
	{
		static void Main(string[] args)  {
			string[] Inputs = Directory.GetFiles(args[0],"*",SearchOption.AllDirectories);
			if(args[2] == "true")
				CompileText(Inputs, args[1]);
			else
				CompileBinary(Inputs, args[1]);
			Directory.Delete(args[0], true);
		}
		static void CompileText(string[] Input, string Output){
            //var optimizer = new GLSLOptimizer(Target.OpenGL);
			StringBuilder Builder = new StringBuilder();
			for(int i = 0; i< Input.Length; i++){
				string fileText = File.ReadAllText(Input[i]);
			    //fileText = fileText.Replace("#version 330 compatibility", "");
			    //fileText = fileText.Replace("#version 330 core", "");
                /*var result = optimizer.Optimize(Input[i].EndsWith(".vert") ? ShaderType.Vertex : ShaderType.Fragment,
			        fileText, OptimizationOptions.None);*/

                Builder.AppendLine("//"+Input[i]);
			    Builder.AppendLine(fileText);
                //Builder.AppendLine("#version 330 compatibility");
				//Builder.AppendLine(result.OutputCode);
				Builder.AppendLine("//#End File#");
			}
			string Out = Builder.ToString();
			byte[] zipped = ZipManager.Zip(Out);
			File.WriteAllBytes(Output, zipped);
		}
		
		static void CompileBinary(string[] Input, string Output){
			List<string> SortedInput = new List<string>(Input);
			SortedInput.Sort(new FileSizeComparer());
			
			using (MemoryStream ms = new MemoryStream()){
				using (BinaryWriter Bw = new BinaryWriter(ms))
				{
				    for(int i = 0; i< SortedInput.Count; i++)
				    {
				        Bw.Write(SortedInput[i]);
				        byte[] Data = File.ReadAllBytes(SortedInput[i]);
				        Bw.Write(Data.Length);
				        Bw.Write(Data);
				    }
				}
			    File.WriteAllBytes(Output, ZipManager.ZipBytes(ms.ToArray()));
            }
		}
	}
	public class FileSizeComparer : IComparer<string>
	{
	    public int Compare(string a, string b)
	    {
	    	long s1 = new FileInfo(a).Length;
	    	long s2 = new FileInfo(b).Length;
	    	
	        if (s1 == s2) return 0;
	        if (s1 > s2) return 1;
	
	        return -1;
	    }
	}
}