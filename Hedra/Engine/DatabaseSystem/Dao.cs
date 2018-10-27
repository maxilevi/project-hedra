using System;
/*using System.Data;
using System.Data.SQLite;*/
using System.Text.RegularExpressions;

namespace Hedra.Engine.DatabaseSystem
{
    public class Dao
    {
        /*public SQLiteConnection Connection { get; }

        protected Dao(SQLiteConnection Connection)
        {
            this.Connection = Connection;
        }

        public T Get<T>(string QueryString, CommandBehavior Behaviour)
        {
            var fieldName = this.ExtractFieldName(QueryString);
            var command = Connection.CreateCommand();
            command.CommandText = QueryString;
            using (var reader = command.ExecuteReader(Behaviour))
            {
                if (reader.FieldCount > 1)
                    throw new NotSupportedException($"Database implementation only supports single returning queries.");
                return (T) Convert.ChangeType(reader[fieldName], typeof(T));
            }
        }

        public string ExtractFieldName(string Query)
        {
            var firstMatch = Regex.Match(Query, @"^[a-zA-Z]+\s+(.*?)\s+");
            return firstMatch.Value;
        }

        public void Query(string QueryString)
        {
            var command = Connection.CreateCommand();
            command.CommandText = QueryString;
            command.ExecuteNonQuery();
        }

        public static Dao From(string DatabasePath)
        {
            var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            connection.Open();
            return new Dao(connection);
        }*/
    }
}
