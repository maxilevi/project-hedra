/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/09/2016
 * Time: 09:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hedra.Engine.EntitySystem
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SaveAttribute : Attribute
	{
		
	}
	
	public class SaveInfo{
		public string FieldName;
		public object Value;
	}
}
