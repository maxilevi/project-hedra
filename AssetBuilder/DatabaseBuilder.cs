using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetBuilder
{
    public class DatabaseBuilder : Builder
    {
        public override void Build(Dictionary<string, object> Input, string Output)
        {
            Dao.BuildDatabase(Output);
            var dao = Dao.From(Output);
            dao.Query("CREATE TABLE Assets (Id VARCHAR(80), Data BLOB);");
            dao.Query("CREATE UNIQUE INDEX `Identifier` ON `Assets` (`Id`);");
            var baseQuery = "INSERT INTO Assets (Id, Data) VALUES";
            var command = dao.Connection.CreateCommand();
            var i = 0;
            foreach (var pair in Input)
            {
                var parameter0 = command.CreateParameter();
                parameter0.ParameterName = $"key{i}";
                parameter0.Value = PathToKey(pair.Key);

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = $"value{i}";
                parameter1.Value = pair.Value;

                command.Parameters.Add(parameter0);
                command.Parameters.Add(parameter1);
                i++;
                baseQuery += $"({parameter0.ParameterName}, {parameter1.ParameterName}), ";
            }
            File.WriteAllText("log.txt", $@"{baseQuery};");
            command.CommandText = baseQuery;
            command.ExecuteNonQuery();
            dao.Connection.Close();
        }

        private static string PathToKey(string Path)
        {
            return Path.Replace("/", "-").Replace("\\", "-");
        }
    }
}
