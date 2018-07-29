/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/07/2016
 * Time: 08:38 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Hedra.Engine.WorldBuilding
{
	/// <summary>
	/// Description of NameGenerator.
	/// </summary>
	internal static class NameGenerator
	{
		public static string[] MaleNames = new string[]{"Keith", "Isaac", "Sylvester", "Alden", "Greg", "Levi", "Tyron", "Elliot"};
		
		public static string[] Names = new string[]{"Adara", "Adena", "Adrianne", "Alarice", "Alvita", "Amara", "Ambika", "Antonia", "Basha",
		"Beryl", "Bryn", "Callia", "Caryssa", "Cassandra", "Chatha", "Ciara", "Cynara", "Cytheria", "Debra", "Darcei",
		"Deandra", "Delores", "Desdomna", "Devi", "Dominique", "Duvessa", "Ebony", "Fantine", "Fuscienne",
		"Gabi", "Gallia", "Hanna", "Hedda", "Jerica", "Jetta", "Joby", "Kacila", "Kagami", "Kala", "Kallie", "Keelia", "Kerry",
	    "Kimberly", "Killian", "Kory", "Lilith", "Lucretia", "Lysha", "Mercedes", "Mia", "Maura", "Perdita", "Quella",
		"Riona", "Salina", "Severin", "Sidonia", "Sirena", "Solita", "Tempest", "Thea", "Treva", "Trista", "Vala", "Winta","Kenneth","Trowsky", "Thallen",
		"Keith", "Isaac", "Sylvester", "Alden", "Greg", "Levi", "Tyron", "Elliot"};
		
		public static Dictionary<char, List<char>> Table = new Dictionary<char, List<char>>();
		
		public static void Load(){
			//First pass add all the different characters
			for(int i = 0; i < Names.Length; i++){
				char[] Chars = Names[i].ToCharArray();
				for(int j = 0; j < Chars.Length; j++){
					if(!Table.ContainsKey( char.ToLowerInvariant(Chars[j]) )){
						Table.Add( char.ToLowerInvariant(Chars[j]) , new List<char>());
					}
				}
			}
			//2nd Pass
			for(int i = 0; i < Names.Length; i++){
				char[] Chars = Names[i].ToCharArray();
				for(int j = 0; j < Chars.Length; j++){
					if(j+1 == Chars.Length) break;
					
					if(!Table[ char.ToLowerInvariant(Chars[j]) ].Contains( char.ToLowerInvariant(Chars[j+1])) )
						Table[ char.ToLowerInvariant(Chars[j]) ].Add( char.ToLowerInvariant(Chars[j+1]) );
				}
			}
		}
		
		public static string Generate(int Seed){
			var rng = new Random(Seed);
			int length = rng.Next(3, 6);
			int entry = rng.Next(0, Table.Keys.Count);

		    var iterator = 0;
		    var startingChar = 'k';
			foreach(char Char in Table.Keys){
				if(iterator == entry)
					startingChar = Char;
				iterator++;
			}
			var builder = new StringBuilder();
			builder.Append(startingChar);
			char pChar = startingChar;
			for(int i = 0; i < length; i++){
				if(Table[pChar].Count == 0)break;
				
				char nextChar = pChar;
				var times = 0;
				while(nextChar == pChar){
					times++;			
					nextChar = Table[pChar][rng.Next(0, Table[pChar].Count)];
					if(times > 4)
						goto END;
				}
					
				builder.Append(nextChar);
				pChar = nextChar;
			}
			END:
			return builder.ToString().First().ToString().ToUpper() + builder.ToString().Substring(1);
		}
		
		public static string PickMaleName(Random Rng){
			return MaleNames[Rng.Next(0, MaleNames.Length)];
		}
	}
}
